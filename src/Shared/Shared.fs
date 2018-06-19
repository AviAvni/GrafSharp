namespace Shared

open System

type Props = Map<string, Literal>

and Entity =
    {
        Var:string;
        Properties:Props
    }
    override this.ToString() =
        sprintf "%A" this.Properties

    member this.Id =
        match this.Properties.TryFind("_id") with
        | Some(String id) -> id
        | _ -> ""

    member this.GetLabels () =
        match this.Properties.TryFind("_label") with
        | Some(List(labels)) -> labels |> List.map (fun l -> match l with | String(l) -> l | _ -> "")
        | _ -> []

    static member Empty =
        { Var=""; Properties = Map.empty.Add("_label", Literal.List [])}

and Pattern =
    | NodePattern of Entity
    | RelationPattern of source:Entity * relation:Entity * target:Entity
and Expr =
    | Literal of Literal
    | Var of string
    | Function of name:string * exp:Expr
    | Name of name:string *exp:Expr
    | Pattern of Pattern
    | And of Expr * Expr
    | Or of Expr * Expr
    | Not of Expr
and Literal =
    | Number of double
    | String of string
    | DateTime of DateTime
    | Bool of bool
    | Guid of Guid
    | Null
    | List of Literal list
    | Object of Map<string, Literal>

