module GraphDriver

open Shared
open ActorsCore
open ActorsTypes
open Actors

type GraphDB () as self =
    let nodeRegistryRegistry : EntityRegistryRegistry =
        createRegistryActor true (createRegistryActor true createNodeActor) None "nodeRegistryRegistry" self

    let relationRegistryRegistry : EntityRegistryRegistry =
        createRegistryActor true (createRegistryActor true createRelationActor) None "relationRegistryRegistry" self

    let queryRegistryRegistry : QueryRegistryRegistry =
        createRegistryActor false (createRegistryActor false createQueryActor) None "queryRegistryRegistry" self

    interface IGraphDb with
        member __.Run (request:QueryRequest) =
            async {
                let! (_, queryRegistry:QueryRegistry) = queryRegistryRegistry.PostAndAsyncReply(fun r -> GetOrSpawnActor("queryRegistry", None, r))
                let id = System.Guid.NewGuid().ToString()
                let! (_,queryActor) = queryRegistry.PostAndAsyncReply(fun r -> GetOrSpawnActor("query-" + id, None, r))
                return! queryActor.PostAndAsyncReply(fun r -> ProcessOnQueryNode(request, r))
            } |> Async.StartAsTask

        member __.GetOrSpawnNode (id:string) (state:Props) =
            async {
                let! _, nodeRegistry = nodeRegistryRegistry.PostAndAsyncReply(fun r -> GetOrSpawnActor("nodeRegistry", None, r))
                let! (_,ref) = nodeRegistry.PostAndAsyncReply(fun r -> GetOrSpawnActor(id, Some(state), r))
                return ref
            }

        member __.GetOrSpawnRelation (id:string) (state:Props) =
            async {
                let! _, relationRegistry = relationRegistryRegistry.PostAndAsyncReply(fun r -> GetOrSpawnActor("relationRegistry", None, r))
                let! (_,ref) = relationRegistry.PostAndAsyncReply(fun r -> GetOrSpawnActor(id, Some(state), r))
                return ref
            }

        member __.QueryNode (msgBuilder:IAsyncReplyChannel<int> -> RegistryActorMessage<EntityActorMessage, EntityRegistryState>) =
            async {
                let! _, nodeRegistry = nodeRegistryRegistry.PostAndAsyncReply(fun r -> GetOrSpawnActor("nodeRegistry", None, r))
                let! count = nodeRegistry.PostAndAsyncReply(msgBuilder)
                return count
            }

        member __.QueryRelation (msgBuilder:IAsyncReplyChannel<int> -> RegistryActorMessage<EntityActorMessage, EntityRegistryState>) =
            async {
                let! _, relationRegistry = relationRegistryRegistry.PostAndAsyncReply(fun r -> GetOrSpawnActor("relationRegistry", None, r))
                let! count = relationRegistry.PostAndAsyncReply(msgBuilder)
                return count
            }