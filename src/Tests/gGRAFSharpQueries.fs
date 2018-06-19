module gGRAFSharpQueries

open Expecto
open GraphActor.Core
open GraphActor.Core.gGRAFSharp
open System

[<AutoOpen>]
module AstConstructionDsl =
    let defProps =
        Entity.Empty.Properties

    let withLabel (label:string) (props:Props) =
        let labelValue =
            label.Split(":", StringSplitOptions.RemoveEmptyEntries)
            |> List.ofArray
            |> List.map (fun x -> LiteralExpr.String(x))
        props.Add("_label", LiteralExpr.List labelValue)

    let addProp (key, value) (props:Props) =
        props.Add(key, LiteralExpr.String value)

    let Entity(var, props:Props) =
        { Var=var; Properties = props }
    let EntityDefProps(var) = Entity(var, defProps)

    let returnVars names =
        names
        |> List.map (fun x -> Expr.Var(x))
        |> fun vars -> ReturnClause(vars)

let shouldBeEqualTo parser expectedAST (query:string) =
    Expect.equal (parser query) expectedAST "Parsed AST does not equal to expected AST"

let [<Tests>] parseLiteral =
    let shouldBeParsedTo expectedAST (str:string) =
        str |> shouldBeEqualTo (Parser.runParse Parser.pLiteral) expectedAST
    testList "gGRAF# Parse Literals" [
        test "number" {
            "42" |> shouldBeParsedTo (LiteralExpr.Number(42 |> double))
        }
        test "string" {
            "\"str\"" |> shouldBeParsedTo (LiteralExpr.String("str"))
        }
        test "datetime" {
            "\"2000 Jan 1\"" |> shouldBeParsedTo (LiteralExpr.DateTime(new DateTime(2000, 1, 1)))
        }
        test "bool - True" {
            "True" |> shouldBeParsedTo (LiteralExpr.Bool(true))
        }
        test "bool - False" {
            "False" |> shouldBeParsedTo (LiteralExpr.Bool(false))
        }
        test "guid" {
            let guid = Guid.NewGuid()
            sprintf "\"%O\"" guid
            |> shouldBeParsedTo (LiteralExpr.Guid(guid))
        }
        test "null" {
            "NULL" |> shouldBeParsedTo (LiteralExpr.Null)
        }
        test "list" {
            """[True False True]"""
            |> shouldBeParsedTo (
                LiteralExpr.List(
                    [
                        LiteralExpr.Bool(true)
                        LiteralExpr.Bool(false)
                        LiteralExpr.Bool(true)
                    ]
                )
            )

        }
        test "object" {
            """{p1:"A", p2:"B"}"""
            |> shouldBeParsedTo (
                LiteralExpr.Object(
                    Map.empty
                    |> addProp ("p1", "A")
                    |> addProp ("p2", "B")
                )
            )
        }
    ]


let [<Tests>] parseExpr =
    let shouldBeParsedTo expectedAST (str:string) =
        str |> shouldBeEqualTo (Parser.runParse Parser.pExpr) expectedAST
    testList "gGRAF# Parse Expr" [
        test "literalExpr - bool" {
            "True" |> shouldBeParsedTo (
                Expr.Literal(LiteralExpr.Bool(true))
            )
        }
        test "varExpr" {
            "a" |> shouldBeParsedTo (Expr.Var("a"))
        }
        test "functionExpr" {
            "call(a)" |> shouldBeParsedTo (
                Expr.Function("call", Expr.Var("a"))
            )
        }
        test "nameExpr" {
            "a AS x" |> shouldBeParsedTo (
                Expr.Name("x", Expr.Var("a"))
            )
        }
        test "complexExpr - NOT" {
            """NOT a"""
            |> shouldBeParsedTo (
                Expr.Not(
                    Expr.Var("a")
                )
            )
        }
        test "complexExpr - OR" {
            """a OR b"""
            |> shouldBeParsedTo (
                Expr.Or(
                    Expr.Var("a"),
                    Expr.Var("b")
                )
            )
        }
        test "complexExpr - AND" {
            """a AND b"""
            |> shouldBeParsedTo (
                Expr.And(
                    Expr.Var("a"),
                    Expr.Var("b")
                )
            )
        }
        test "complexExpr - ()" {
            """(NOT a) AND (b OR c)"""
            |> shouldBeParsedTo (
                Expr.And(
                    Expr.Not(
                        Expr.Var("a")
                    ),
                    Expr.Or(
                        Expr.Var("b"),
                        Expr.Var("c")
                    )
                )
            )
        }
        test "patternExpr" {
            """(a)-[b]->(c)"""
            |> shouldBeParsedTo (
                Expr.Pattern(
                    RelationPattern(
                        EntityDefProps("a"),
                        EntityDefProps("b"),
                        EntityDefProps("c")
                    )
                )
            )
        }
    ]



let [<Tests>] parsePatterns =
    let shouldBeParsedTo expectedAST (str:string) =
        str |> shouldBeEqualTo (Parser.runParse Parser.pPattern) expectedAST
    testList "gGRAF# Parse Patterns" [
        test "node pattern" {
            """(a{Label:"A"})"""
            |> shouldBeParsedTo
                (NodePattern(
                    Entity("a", defProps |> addProp ("Label", "A"))
                 )
                )
        }
        test "relation pattern - out" {
            """(a{Label:"A"})-[r:Type]->(b{Label:"B"})"""
            |> shouldBeParsedTo
                (RelationPattern(
                        Entity("a", defProps |> addProp ("Label", "A")),
                        Entity("r", defProps |> withLabel "Type"),
                        Entity("b", defProps |> addProp ("Label", "B"))
                 )
                )
        }
        test "relation pattern - out (not relation props)" {
            """(a{Label:"A"})-->(b{Label:"B"})"""
            |> shouldBeParsedTo
                (RelationPattern(
                        Entity("a", defProps |> addProp ("Label", "A")),
                        gGRAFSharp.Entity.Empty,
                        Entity("b", defProps |> addProp ("Label", "B"))
                 )
                )
        }
        test "relation pattern - in" {
            """(a{Label:"A"})<-[r:Type]-(b{Label:"B"})"""
            |> shouldBeParsedTo
                (RelationPattern(
                        Entity("b", defProps |> addProp ("Label", "B")),
                        Entity("r", defProps |> withLabel "Type"),
                        Entity("a", defProps |> addProp ("Label", "A"))
                 )
                )
        }
        test "relation pattern - in (not relation props)" {
            """(a{Label:"A"})<--(b{Label:"B"})"""
            |> shouldBeParsedTo
                (RelationPattern(
                        Entity("b", defProps |> addProp ("Label", "B")),
                        gGRAFSharp.Entity.Empty,
                        Entity("a", defProps |> addProp ("Label", "A"))
                 )
                )
        }
        ptest "relation pattern - in or out" { // not supported by grammar
            """(a{Label:"A"})-[r:Type]-(b{Label:"B"})"""
            |> shouldBeParsedTo
                (RelationPattern(
                        Entity("a", defProps |> addProp ("Label", "A")),
                        Entity("r", defProps |> withLabel "Type"),
                        Entity("b", defProps |> addProp ("Label", "B"))
                 )
                )
        }
        ptest "relation pattern - in or out (no relation props)" { // not supported by grammar
            """(a{Label:"A"})--(b{Label:"B"})"""
            |> shouldBeParsedTo
                (RelationPattern(
                        Entity("a", defProps |> addProp ("Label", "A")),
                        gGRAFSharp.Entity.Empty,
                        Entity("b", defProps |> addProp ("Label", "B"))
                 )
                )
        }
    ]


let [<Tests>] gGRAFSharpQueries =
    let shouldBeParsedTo expectedAST (query:string) =
        query |> shouldBeEqualTo (Parser.parse) expectedAST
    testList "gGRAF# Parse Query" [
        test "CREATE node" {
            """CREATE({Label:"C"})"""
            |> shouldBeParsedTo
                (CreateClause(
                    NodePattern(Entity("", defProps |> addProp ("Label", "C"))),
                    None
                    )
                )
        }
        test "CREATE relation" {
            """CREATE (a{Label:"A"})-[r:Type]->(b{Label:"B"})"""
            |> shouldBeParsedTo
                (CreateClause(
                    RelationPattern(
                        Entity("a", defProps |> addProp ("Label", "A")),
                        Entity("r", defProps |> withLabel "Type"),
                        Entity("b", defProps |> addProp ("Label", "B"))
                    ) , None
                 )
                )
        }
        test "MERGE (MATCH-or-CREATE)" {
            """MERGE (a{Label:"A"})-[r:Type]->(b{Label:"B"})"""
            |> shouldBeParsedTo
                (MergeClause(
                    RelationPattern(
                        Entity("a", defProps |> addProp ("Label", "A")),
                        Entity("r", defProps |> withLabel "Type"),
                        Entity("b", defProps |> addProp ("Label", "B"))
                    ), None
                 )
                )
        }
        test "MATCH with RETURN" {
            """MATCH ({Label:"A"}) RETURN NULL"""
            |> shouldBeParsedTo
                (MatchClause(
                    NodePattern(
                        Entity("", defProps |> addProp ("Label", "A"))),
                    ReturnClause([Literal(Null)])
                ))
        }
        test "MATCH with MERGE" {
            """MATCH (a{Label:"A"})-[]->(b{Label:"B"})
               MERGE (b)-[{x:"y"}]->(a)"""
            |> shouldBeParsedTo
                (MatchClause(
                    RelationPattern(
                        Entity("a", defProps |> addProp ("Label", "A")),
                        EntityDefProps(""),
                        Entity("b", defProps |> addProp ("Label", "B"))
                    ),
                    MergeClause(
                        RelationPattern(
                            EntityDefProps("b"),
                            Entity("", defProps |> addProp ("x", "y")),
                            EntityDefProps("a")
                        ), None
                    )
                 )
                )
        }
        // Test cases from Grammar (Semantics)
        test "MATCH node RETURN" {
            """MATCH (a)
               RETURN a"""
            |> shouldBeParsedTo
                (MatchClause(
                    NodePattern(EntityDefProps("a")),
                    returnVars ["a"]
                 )
                )
        }
        test "MATCH relation RETURN" {
            """MATCH (a)-[b]->(c)
               RETURN a, b, c"""
            |> shouldBeParsedTo
                (MatchClause(
                    RelationPattern(
                        EntityDefProps("a"),
                        EntityDefProps("b"),
                        EntityDefProps("c")
                    ),
                    returnVars ["a"; "b"; "c"]
                 )
                )
        }
        test "MATCH node MATCH node RETURN" {
            """MATCH (a)
               MATCH (b)
               RETURN a, b"""
            |> shouldBeParsedTo
                (MatchClause(
                    NodePattern(EntityDefProps("a")),
                    MatchClause(
                        NodePattern(EntityDefProps("b")),
                        returnVars ["a"; "b"]
                    )
                 )
                )
        }
        test "MATCH relation MATCH node RETURN" {
            """MATCH (a)-[b]->(c)
               MATCH (d)
               RETURN a, b, c, d"""
            |> shouldBeParsedTo
                (MatchClause(
                    RelationPattern(
                        EntityDefProps("a"),
                        EntityDefProps("b"),
                        EntityDefProps("c")
                    ),
                    MatchClause(
                        NodePattern(EntityDefProps("d")),
                        returnVars ["a"; "b"; "c"; "d"]
                    )
                 )
                )
        }
        test "MATCH node MATCH relation RETURN" {
            """MATCH (a)
               MATCH (b)-[c]->(d)
               RETURN a, b, c, d"""
            |> shouldBeParsedTo
                (MatchClause(
                    NodePattern(EntityDefProps("a")),
                    MatchClause(
                        RelationPattern(
                            EntityDefProps("b"),
                            EntityDefProps("c"),
                            EntityDefProps("d")
                        ),
                        returnVars ["a"; "b"; "c"; "d"]
                    )
                 )
                )
        }
        test "MATCH relation MATCH relation RETURN" {
            """MATCH (a)-[b]->(c)
               MATCH (d)-[e]->(f)
               RETURN a, b, c, d, e, f"""
            |> shouldBeParsedTo
                (MatchClause(
                    RelationPattern(
                        EntityDefProps("a"),
                        EntityDefProps("b"),
                        EntityDefProps("c")
                    ),
                    MatchClause(
                        RelationPattern(
                            EntityDefProps("d"),
                            EntityDefProps("e"),
                            EntityDefProps("f")
                        ),
                        returnVars ["a"; "b"; "c"; "d"; "e"; "f"]
                    )
                 )
                )
        }
        test "MATCH relation MATCH relation (same start node) RETURN" {
            """MATCH (a)-[b]->(c)
               MATCH (a)-[d]->(e)
               RETURN a, b, c, d, e"""
            |> shouldBeParsedTo
                (MatchClause(
                    RelationPattern(
                        EntityDefProps("a"),
                        EntityDefProps("b"),
                        EntityDefProps("c")
                    ),
                    MatchClause(
                        RelationPattern(
                            EntityDefProps("a"),
                            EntityDefProps("d"),
                            EntityDefProps("e")
                        ),
                        returnVars ["a"; "b"; "c"; "d"; "e"]
                    )
                 )
                )
        }
        test "MATCH relation MATCH relation (same start & end node) RETURN" {
            """MATCH (a)-[b]->(c)
               MATCH (a)-[d]->(c)
               RETURN a, b, c, d"""
            |> shouldBeParsedTo
                (MatchClause(
                    RelationPattern(
                        EntityDefProps("a"),
                        EntityDefProps("b"),
                        EntityDefProps("c")
                    ),
                    MatchClause(
                        RelationPattern(
                            EntityDefProps("a"),
                            EntityDefProps("d"),
                            EntityDefProps("c")
                        ),
                        returnVars ["a"; "b"; "c"; "d"]
                    )
                 )
                )
        }
    ]