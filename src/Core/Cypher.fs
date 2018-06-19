module Cypher

open System
open Antlr4.Runtime.Tree
open Antlr4.Runtime

open Shared
open Language

let inline notNull x =
    not <| isNull x
let inline getText parseTree =
    match box parseTree with
    | :? IParseTree as x -> x.GetText()
    | _ -> sprintf "%A" parseTree

let inline failIfNotNull getter tokenName =
    let value = getter()
    if notNull value then
        failwithf "no support for '%s' that ='%A'" tokenName (getText value)

type PropsVisitor () =
    inherit CypherBaseVisitor<Props>()

    /// properties : mapLiteral
    ///            | parameter
    ///            | legacyParameter
    override this.VisitProperties ctx =
        failIfNotNull ctx.parameter "parameter"
        failIfNotNull ctx.legacyParameter "legacyParameter"
        this.VisitMapLiteral <| ctx.mapLiteral()

    /// mapLiteral : '{' SP? ( propertyKeyName SP? ':' SP? expression SP? ( ',' SP? propertyKeyName SP? ':' SP? expression SP? )* )? '}' ;
    override this.VisitMapLiteral ctx =
        let properties =
            ctx.propertyKeyName()
            |> Array.map (fun x -> x.GetText())
        let values =
            ctx.expression()
            |> Array.map (fun x ->
                Literal.String <| x.GetText()) // TODO: incorrect
        Seq.zip properties values
        |> Map.ofSeq

type EntityVisitor () =
    inherit CypherBaseVisitor<Entity>()

    let propsVisitor = PropsVisitor()

    /// nodePattern : '(' SP? ( variable SP? )? ( nodeLabels SP? )? ( properties SP? )? ')' ;
    override this.VisitNodePattern ctx =
        // variable : symbolicName ;
        let variable =
            ctx.variable()
            |> Option.ofObj
            |> Option.map (fun x -> x.GetText()) // ???
            |> Option.defaultValue ""

        // nodeLabels : nodeLabel ( SP? nodeLabel )* ;
        // nodeLabel : ':' SP? labelName ;
        // labelName : schemaName ;
        let nodeLabel =
            ctx.nodeLabels()
            |> Option.ofObj
            |> Option.map (fun ctx' ->
                ctx'.nodeLabel()
                |> Array.map (fun x -> x.GetText()) // ???
                |> String.concat ":"
               )
            |> Option.defaultValue null

        let props =
            match ctx.properties() with
            | null -> Map.empty
            | props -> propsVisitor.VisitProperties props
            |> fun props ->
                if String.IsNullOrEmpty nodeLabel then props
                else
                    nodeLabel.Split([|':'|], StringSplitOptions.RemoveEmptyEntries)
                    |> List.ofArray
                    |> List.map (Literal.String)
                    |> fun labels ->
                        props |> Map.add "_label" (Literal.List labels)

        { Var = variable
          Properties = props}

    override this.VisitRelationshipPattern ctx =
        let details = ctx.relationshipDetail()

        let variable =
            details.variable()
            |> Option.ofObj
            |> Option.map (fun x -> x.GetText()) // ???
            |> Option.defaultValue ""

        // nodeLabels : nodeLabel ( SP? nodeLabel )* ;
        // nodeLabel : ':' SP? labelName ;
        // labelName : schemaName ;
        let nodeLabel =
            details.relationshipTypes()
            |> Option.ofObj
            |> Option.map (fun ctx' ->
                ctx'.relTypeName()
                |> Array.map (fun x -> x.GetText()) // ???
                |> String.concat ":"
               )
            |> Option.defaultValue null

        let props =
            match details.properties() with
            | null -> Map.empty
            | props -> propsVisitor.VisitProperties props
            |> fun props ->
                if String.IsNullOrEmpty nodeLabel then props
                else props |> Map.add "_label" (Literal.String nodeLabel)

        { Var = variable
          Properties = props}

type PatternVisitor () =
    inherit CypherBaseVisitor<Pattern>()

    let entityVisitor = EntityVisitor()

    /// patternPart : ( variable SP? '=' SP? anonymousPatternPart )
    ///             | anonymousPatternPart
    override this.VisitPatternPart ctx =
        failIfNotNull ctx.variable "variable"
        this.VisitAnonymousPatternPart <| ctx.anonymousPatternPart()

    /// anonymousPatternPart : shortestPathPattern
    ///                      | patternElement
    override this.VisitAnonymousPatternPart ctx =
        failIfNotNull ctx.shortestPathPattern "shortestPathPattern"
        this.VisitPatternElement <| ctx.patternElement()

    /// patternElement : ( nodePattern ( SP? patternElementChain )* )
    ///                | ( '(' patternElement ')' )
    override this.VisitPatternElement ctx =
        let patternElement = ctx.patternElement()
        if notNull patternElement
        then this.VisitPatternElement patternElement
        else
            let node = ctx.nodePattern() |> entityVisitor.VisitNodePattern
            let chain = ctx.patternElementChain()
            if chain.Length = 0 then NodePattern(node)
            else
                let a = chain.[0]
                let next = a.nodePattern() |> entityVisitor.VisitNodePattern
                let rel = a.relationshipPattern() |> entityVisitor.VisitRelationshipPattern
                RelationPattern(node, rel, next)



type QueryAstVisitor () =
    inherit CypherBaseVisitor<QueryAst>()

    let patternVisitor = PatternVisitor()

    /// cypher : SP? queryOptions statement ( SP? ';' )? SP? EOF ;
    override this.VisitCypher ctx =
        //failIfNotNull ctx.queryOptions "queryOptions" // TODO !!!! ????
        this.VisitStatement <| ctx.statement()

    /// statement : command
    ///           | query
    override this.VisitStatement ctx =
        failIfNotNull ctx.command "command"
        this.VisitQuery <| ctx.query()

    /// query : regularQuery
    ///       | standaloneCall
    ///       | bulkImportQuery
    override this.VisitQuery ctx =
        failIfNotNull ctx.standaloneCall "standaloneCall"
        failIfNotNull ctx.bulkImportQuery "bulkImportQuery"
        this.VisitRegularQuery <| ctx.regularQuery()

    /// regularQuery : singleQuery ( SP? union )* ;
    override this.VisitRegularQuery ctx =
        this.VisitSingleQuery <| ctx.singleQuery()
        //TODO: support ctx.union()

    /// singleQuery : singlePartQuery
    ///             | multiPartQuery
    override this.VisitSingleQuery ctx =
        failIfNotNull ctx.multiPartQuery "multiPartQuery" // ???
        this.VisitSinglePartQuery <| ctx.singlePartQuery()

    /// singlePartQuery : readOnlyEnd
    ///                 | readUpdateEnd
    ///                 | updatingEnd
    override this.VisitSinglePartQuery ctx =
        failIfNotNull ctx.readOnlyEnd "readOnlyEnd"
        failIfNotNull ctx.readUpdateEnd "readUpdateEnd" // ???
        this.VisitUpdatingEnd <| ctx.updatingEnd()

    /// updatingEnd : updatingStartClause ( SP? updatingClause )* ( SP? return )? ;
    override this.VisitUpdatingEnd ctx =
        let rec embedQueryAst newInner ast =
            match ast with
            | CreateClause(patt, None) ->
                CreateClause(patt, newInner)
            | CreateClause(patt, Some inner) ->
                CreateClause(patt, Some <| embedQueryAst newInner inner)
            | _ -> failwithf "Cannot embed inner='%A' into query='%A'" newInner ast
        let innerQuery =
            Seq.foldBack
                (fun clause (innerAst: QueryAst option) ->
                    this.VisitUpdatingClause clause
                    |> embedQueryAst innerAst
                    |> Some)
                (ctx.updatingClause()) None
        ctx.updatingStartClause()
        |> this.VisitUpdatingStartClause
        |> embedQueryAst innerQuery
        // TODO: //


    /// updatingStartClause : create
    ///                     | merge
    ///                     | createUnique
    ///                     | foreach
    override this.VisitUpdatingStartClause ctx =
        failIfNotNull ctx.merge "merge"
        failIfNotNull ctx.createUnique "createUnique"
        failIfNotNull ctx.foreach "foreach"
        this.VisitCreate <| ctx.create()

    /// updatingClause : create
    ///                | merge
    ///                | createUnique
    ///                | foreach
    ///                | delete
    ///                | set
    ///                | remove
    override this.VisitUpdatingClause ctx =
        failIfNotNull ctx.merge "merge"
        failIfNotNull ctx.createUnique "createUnique"
        failIfNotNull ctx.foreach "foreach"
        failIfNotNull ctx.delete "delete"
        failIfNotNull ctx.set "set"
        failIfNotNull ctx.remove "remove"
        this.VisitCreate <| ctx.create()

    /// create : CREATE SP? pattern ;
    override this.VisitCreate ctx =
        this.VisitPattern <| ctx.pattern()

    /// pattern : patternPart ( SP? ',' SP? patternPart )* ;
    override this.VisitPattern ctx =
        let parts =
            ctx.patternPart()
            |> Array.map (patternVisitor.VisitPatternPart)
        Array.foldBack
            (fun pattern state ->
                Some <| CreateClause(pattern, state))
            parts None
        |> fun opRes -> opRes.Value

let private visitor = QueryAstVisitor()
let toGRAFSharpQuery (input:string) =
    input
    |> CharStreams.fromstring
    |> CypherLexer
    |> CommonTokenStream
    |> CypherParser
    |> fun parser ->
        let tree = parser.cypher()
        if isNull tree then
            failwith "Cannot parse query"
        visitor.Visit tree

let toGRAFSharpQueries (input:string) =
    // Queries are separated by ';'
    input.Split([|';'|], StringSplitOptions.RemoveEmptyEntries)
    |> Array.map (toGRAFSharpQuery)