module Language

open System

open Shared

type QueryAst =
    | MatchClause of pattern:Pattern * inner:QueryAst
    | CreateClause of pattern:Pattern * inner:QueryAst option
    | MergeClause of pattern:Pattern * inner:QueryAst option
    | SetClause of var:string * property:string * expr:Expr * inner:QueryAst option
    | ReturnClause of Expr list

module Parser =
    open FParsec

    type UserState = unit
    type Parser<'t> = Parser<'t, UserState>

    // --------- Common Parsers

    let private str s = pstring s >>. spaces

    let private pIdentifierNoSpaces =
        let isIdentifierFirstChar c = isLetter c
        let isIdentifierChar c = isLetter c || isDigit c || c = '.' || c = '-' || c = '_'

        many1Satisfy2L isIdentifierFirstChar isIdentifierChar "propertyName"

    let private pIdentifier =
        pIdentifierNoSpaces .>> spaces

    // --------- LiteralExpr Parser

    let pLiteral, pLiteralImpl = createParserForwardedToRef()

    let pNumber =
        pfloat .>> spaces |>> fun n -> Number(n)

    let pString =
        let jUnescapedChar =
            satisfy (fun ch -> ch <> '\\' && ch <> '\"')
        let jEscapedChar =
            [
            ("\\\"",'\"')      // quote
            ("\\\\",'\\')      // reverse solidus
            ("\\/",'/')        // solidus
            ("\\b",'\b')       // backspace
            ("\\f",'\f')       // formfeed
            ("\\n",'\n')       // newline
            ("\\r",'\r')       // cr
            ("\\t",'\t')       // tab
            ]
            |> List.map (fun (toMatch,result) ->
                pstring toMatch >>% result)
            |> choice

        let jUnicodeChar =
            let backslash = pchar '\\'
            let uChar = pchar 'u'
            let hexdigit = anyOf (['0'..'9'] @ ['A'..'F'] @ ['a'..'f'])
            let convertToChar (((h1,h2),h3),h4) =
                let str = sprintf "%c%c%c%c" h1 h2 h3 h4
                Int32.Parse(str,Globalization.NumberStyles.HexNumber) |> char
            backslash  >>. uChar >>. hexdigit .>>. hexdigit .>>. hexdigit .>>. hexdigit
            |>> convertToChar

        let quote = pchar '\"'
        let jchar = jUnescapedChar <|> jEscapedChar <|> jUnicodeChar

        quote >>. manyChars jchar .>> quote
        |>> fun str -> String(str)

    let pStringOrDateTimeOrGuid =
        pString
        >>= function
            | String str ->
                match DateTime.TryParse str with
                | true, dt -> preturn (DateTime dt)
                | false, _ ->
                    match Guid.TryParse str with
                    | true, guid -> preturn (Guid guid)
                    | false, _ -> preturn (String str)
            | value -> preturn value

    let pBool =
        (str "True" >>. preturn (Bool(true))) <|> (str "False" >>. preturn (Bool(false)))

    let pNull =
        str "NULL"
        |>> fun _ -> Null

    let pList =
        str "[" >>. many pLiteral .>> str "]"
        |>> fun ls -> List(ls)

    let private pProperty =
        (pIdentifier .>> str ":" .>>. pLiteral) .>> spaces

    let private pProperties =
        let props = sepBy pProperty (str ",")
        (props |> between (str "{") (str "}")) .>> spaces

    let pObject =
        pProperties
        >>= fun map ->
            preturn (Object(map |> Map.ofList))

    pLiteralImpl :=
        pNumber <|> pBool <|> pStringOrDateTimeOrGuid <|> pNull <|> pList <|> pObject

    // --------- Pattern Parser

    let private pNodeImpl cFst cLst =
        let var_lab =
            (opt pIdentifier) .>>. (many (pstring ":" >>. pIdentifier))
        let var_lab_props = var_lab .>>. (opt pProperties)
        (var_lab_props |> between (str cFst) (str cLst)) .>> spaces
        |>> fun ((oVal, oLabel), oProps) ->
              let var = match oVal with | Some(oval) -> oval | _ -> ""
              let props = match oProps with | Some(oprop) -> oprop |> Map.ofList | _ -> Map.empty
              let props = props.Add("_label", List(oLabel |> List.map String))
              { Var=var; Properties=props }

    let private pNode =
        pNodeImpl "(" ")"
        |>> fun n -> NodePattern(n)

    let private pRelationOut =
        pNodeImpl "(" ")" .>>? pstring "-" .>>.? opt (pNodeImpl "[" "]") .>>? pstring "->" .>>.? pNodeImpl "(" ")"
        |>> fun ((n1, n2), n3) -> RelationPattern(n1, n2 |> Option.defaultValue Entity.Empty, n3)

    let private pRelationIn =
        pNodeImpl "(" ")" .>>? pstring "<-" .>>.? opt (pNodeImpl "[" "]") .>>? pstring "-" .>>.? pNodeImpl "(" ")"
        |>> fun ((n1, n2), n3) -> RelationPattern(n3, n2 |> Option.defaultValue Entity.Empty, n1)

    let pPattern =
        pRelationOut
        <|> pRelationIn
        <|> pNode

    // --------- Expr Parser

    let pLiteralExpr =
        pLiteral |>> fun l -> Literal(l)

    let pVar =
        pIdentifier |>> (fun var -> Var(var))

    let pPatternExpr =
        pPattern |>> fun p -> Pattern(p)


    let opp = new OperatorPrecedenceParser<Expr,_,_>()
    let pExpr = opp.ExpressionParser

    let pFunction =
        pIdentifierNoSpaces .>>. (pstring "(" >>. pExpr .>> str ")")
        |>> fun (name, exp) -> Function(name, exp)

    let pName =
        pExpr .>> str "AS" .>>. pIdentifier
        |>> fun (exp, name) -> Name(name, exp)

    opp.TermParser <-
        attempt pPatternExpr
        <|> attempt pFunction
        <|> pLiteralExpr
        <|> pVar
        <|> between (str "(") (str ")") pExpr

    opp.AddOperator(InfixOperator("OR", spaces, 1, Associativity.Left, fun e1 e2 -> Expr.Or(e1,e2) ))
    opp.AddOperator(InfixOperator("AND", spaces, 2, Associativity.Left, fun e1 e2 -> Expr.And(e1,e2) ))
    opp.AddOperator(PrefixOperator("NOT", spaces, 3, true, fun x -> Expr.Not(x)))
    opp.AddOperator(InfixOperator("AS", spaces, 4, Associativity.Left, fun e1 e2 ->
        match e2 with
        | Expr.Var(name) -> Expr.Name(name, e1)
        | _ -> Expr.Name(null, e1))) // TODO: This is error state


    // --------- QueryAst Parser

    let pQuery, pQueryImpl = createParserForwardedToRef()

    let private pMatch =
        str "MATCH" >>. pPattern .>>. pQuery
        |>> fun (pattern, query) -> MatchClause(pattern, query)

    let private pCreate =
        str "CREATE" >>. pPattern .>>. (opt pQuery)
        |>> fun (pattern, query) -> CreateClause(pattern, query)

    let private pMerge =
        str "MERGE" >>. pPattern .>>. (opt pQuery)
        |>> fun (pattern, query) -> MergeClause(pattern, query)

    let private pSet =
        str "SET" >>. pIdentifier .>> str "." .>>. pIdentifier .>> str "=" .>>. pExpr .>>. (opt pQuery)
        |>> fun (((var1, var2), expr), query) -> SetClause(var1, var2, expr, query)

    let private pReturn =
        str "RETURN" >>. sepBy pExpr (str ",")
        |>> fun expr -> ReturnClause(expr)

    pQueryImpl :=
        pMatch <|> pCreate <|> pMerge <|> pSet <|> pReturn

    // --------- Runners

    let runParse parser query =
        let p = spaces >>. parser .>> eof
        match run p query with
        | Success(result, _, _)   -> result
        | Failure(errorMsg, _, _) ->
            failwithf "Failure: %s" errorMsg

    let parse query =
        runParse pQuery query