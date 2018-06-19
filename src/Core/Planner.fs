module Planner

open Shared
open Language
open ActorsTypes

let rec mkPlan initPlan query =
    match query with
    | MatchClause(pat, inner) ->
        let plan = 
            match initPlan, pat with
            | Some(Single(Node(pnode))), NodePattern(node) -> 
                if pnode.Var = node.Var then Single(Node(pnode))
                else ParallelLeaf([Node(pnode); Node(node)])
            | Some(Single(Node(pnode))), RelationPattern(source, rel, target) -> 
                if pnode.Var = source.Var then SequenceLeaf([Relation(rel); Node(pnode); Node(target)])
                elif pnode.Var = target.Var then SequenceLeaf([Relation(rel); Node(source); Node(pnode)])
                else Parallel([Single(Node(pnode)); SequenceLeaf([Relation(rel); Node(source); Node(target)])])
            | Some(SequenceLeaf(pplans)), NodePattern(node) -> 
                if pplans |> List.exists (fun p -> match p with | Node(p) -> p.Var = node.Var | _ -> false) then SequenceLeaf(pplans)
                else SequenceLeaf(Node(node)::pplans)
            | Some(SequenceLeaf(pplans)), RelationPattern(source, rel, target) -> 
                failwith "not implemented"
            | Some(ParallelLeaf(pplans)), NodePattern(node) -> 
                if pplans |> List.exists (fun p -> match p with | Node(p) -> p.Var = node.Var | _ -> false) then ParallelLeaf(pplans)
                else ParallelLeaf(Node(node)::pplans)
            | Some(ParallelLeaf(pplans)), RelationPattern(source, rel, target) -> 
                failwith "not implemented"
            | Some(Sequence(pplans)), _ ->
                failwith "not implemented"
            | Some(Parallel(pplans)), _ ->
                failwith "not implemented"
            | None, NodePattern(node) -> Single(Node(node))
            | None, RelationPattern(source, rel, target) -> SequenceLeaf([Relation(rel); Node(source); Node(target)])
            | _ ->
                failwith "not implemented"

        mkPlan (Some(plan)) inner
    | CreateClause(pat, inner) ->
        match inner with
        | Some(inner) -> Some(Create(pat, initPlan, mkPlan None inner))
        | None -> Some(Create(pat, initPlan, None))
    | MergeClause(pat, inner) -> None
    | SetClause(var, property, exp, inner) ->
        match inner with
        | Some(inner) -> Some(Set(var, property, exp, initPlan, mkPlan None inner))
        | None -> Some(Set(var, property, exp, initPlan, None))
    | ReturnClause(exp) ->
        Some(Return(exp, [], initPlan, None))