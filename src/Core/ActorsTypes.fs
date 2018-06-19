module ActorsTypes

open System.Threading
open System.Collections.Generic

open Shared
open ActorsCore
open Language
open ActorsMailboxProccessor

type QueryContext = Map<string, string>
let EmptyContext:QueryContext = Map.empty

type QueryRequest =
    {
        Query: QueryAst
        Context: QueryContext
    }
and QueryResult =
    {
        Match: Expr list
        Context: QueryContext
    }

type PlanLeaf =
    | Node of Entity
    | Relation of Entity

type PlanStructure =
    | Single of PlanLeaf
    | ParallelLeaf of PlanLeaf list
    | SequenceLeaf of PlanLeaf list
    | Parallel of PlanStructure list
    | Sequence of PlanStructure list

type Plan =
    | Return of Expr list * Expr list * PlanStructure option * Agent<QueryActorMessage> option
    | Create of Pattern * PlanStructure option * Plan option
    | Set of string * string * Expr * PlanStructure option * Plan option

and QueryActorMessage =
    | ProcessOnQueryNode of QueryRequest * IAsyncReplyChannel<QueryResult>
    | Result of Expr list
    | MessagesResult of int
and EntityActorMessage =
    | Initialize of EntityRegistryState option
    | Query of Plan
and RegistryActorMessage<'a, 'b> =
    | GetOrSpawnActor of id:string * 'b option *  IAsyncReplyChannel<string*Agent<'a>>
    | GetActors of 'a * IAsyncReplyChannel<int>

and RegistryState<'a> = Dictionary<string, Agent<'a>>
and RegistryActor<'a, 'b> = Agent<RegistryActorMessage<'a, 'b>>

and EntityActor = Agent<EntityActorMessage>
and QueryActor = Agent<QueryActorMessage>

and EntityRegistryState = Props
and EntityRegistry = RegistryActor<EntityActorMessage, EntityRegistryState>
and EntityRegistryRegistry = RegistryActor<RegistryActorMessage<EntityActorMessage, EntityRegistryState>, RegistryState<EntityActorMessage>>

and QueryRegistry = RegistryActor<QueryActorMessage, unit>
and QueryRegistryRegistry = RegistryActor<RegistryActorMessage<QueryActorMessage, unit>, RegistryState<QueryActorMessage>>

and FactoryFunc<'a, 'b> = 'b -> string -> Agent<'a>

and Record = { Id : string }

[<Interface>]
type IGraphDb =
    abstract member Run : QueryRequest -> Tasks.Task<QueryResult>
    abstract member GetOrSpawnNode : string -> Props -> Async<EntityActor>
    abstract member GetOrSpawnRelation : string -> Props -> Async<EntityActor>
    abstract member QueryNode : (IAsyncReplyChannel<int> -> RegistryActorMessage<EntityActorMessage, EntityRegistryState>) -> Async<int>
    abstract member QueryRelation : (IAsyncReplyChannel<int> -> RegistryActorMessage<EntityActorMessage, EntityRegistryState>) -> Async<int>