module Actors

open Shared
open ActorsCore
open ActorsTypes
open ActorsMailboxProccessor
open Language
open Planner
open Storage

let zeroProps() = Map.empty<string, Literal>

let matchProps (propsSource:Props) (propsTarget:Props) =
    propsSource
    |> Map.forall (fun k v ->
        match propsTarget.TryFind(k) with
        | Some(x) ->
            if k = "_label" then
                match x, v with
                | _, List([]) -> true
                | List(list), List(v) -> v |> List.forall (fun v -> list |> List.exists (fun x -> x = v))
                | String(v), List(vs) ->  vs |> List.contains (String(v)) 
                | _ -> x = v
            else x = v
        | _ -> false)

let createNodeActor (state:EntityRegistryState option) name (db:IGraphDb) : EntityActor =
    let rec loop state (self:EntityActor) = async {
        let! msg = self.Receive()
        match msg with
        | Initialize(state) ->
            let state:Props =
                match state with
                | Some(state) ->
                    saveNode state
                | None ->
                    getNodeById name

            return! loop state self
        | Query(Return(ret, res, Some(Single(Node(node))), Some(repl))) ->
            let res =
                if matchProps node.Properties state then
                    if ret |> List.contains (Var(node.Var)) then
                        Pattern(NodePattern({ node with Properties = state }))::res
                    else
                        res
                else
                    []

            repl.Post(Result(res))

            return! loop state self
        | Query(Return(ret, res, Some(SequenceLeaf(Node(node)::plan)), Some(repl))) ->
            if matchProps node.Properties state then
                let res =
                    if ret |> List.contains (Var(node.Var)) then
                        Pattern(NodePattern({ node with Properties = state }))::res
                    else
                        res

                match plan with
                | [] -> repl.Post(Result(res))
                | Node(node)::_ ->
                    let! next = db.GetOrSpawnNode node.Id Map.empty
                    next.Post(Query(Return(ret, res, Some(SequenceLeaf(plan)), Some(repl))))
                | Relation(relation)::_ ->
                    let! next = db.GetOrSpawnRelation relation.Id Map.empty
                    next.Post(Query(Return(ret, res, Some(SequenceLeaf(plan)), Some(repl))))
            else
                repl.Post(Result([]))

            return! loop state self
        | Query(Set(var, property, Literal(exp), Some(Single(Node(node))), None)) ->
            let state = 
                if matchProps node.Properties state then
                     saveNode (state.Remove(property).Add(property, exp))
                else state
            return! loop state self
        | Query(Set(var, property, Literal(exp), Some(Single(Node(node))), Some(Return(Var(ret)::_, _, _, Some(repl))))) when var = ret ->
            let state, res = 
                if matchProps node.Properties state then
                    let newState = saveNode (state.Remove(property).Add(property, exp))
                    newState, [Pattern(NodePattern({ node with Properties = newState }))]
                else state, []
            repl.Post(Result(res))
            return! loop state self
        | _ -> failwith "not implemented"
    }
    EntityActor(name, loop (defaultArg state <| zeroProps()))
    |> AgentRegistry.start
    |> AgentRegistry.post(Initialize(state))


let createRelationActor (state:EntityRegistryState option) name (db:IGraphDb) =
    let rec loop state (self:EntityActor) = async {
        let! msg = self.Receive()
        match msg with
        | Initialize(state) ->
            let state:Props =
                match state with
                | Some state ->
                    saveRelation state
                | None ->
                    getRelationById name

            return! loop state self
        | Query(Return(ret, res, Some(SequenceLeaf(Relation(relation)::Node(source)::Node(target)::plan)), Some(repl))) ->
            if matchProps relation.Properties state then
                let res =
                    if ret |> List.contains (Var(relation.Var)) then
                        Pattern(RelationPattern({ source with Properties = source.Properties.Add("_id", state.["_sourceId"]) }, { relation with Properties = state }, { target with Properties = target.Properties.Add("_id", state.["_targetId"]) }))::res
                    else
                        res
                let sourceId =
                    match state.["_sourceId"] with
                    | String(sourceId) -> sourceId
                    | _ -> failwith "invalid id"

                let targetId =
                    match state.["_targetId"] with
                    | String(targetId) -> targetId
                    | _ -> failwith "invalid id"

                let target = { target with Properties = target.Properties.Add("_id", String(targetId)) }

                let! fromNode = db.GetOrSpawnNode sourceId Map.empty
                fromNode.Post(Query(Return(ret, res, Some(SequenceLeaf(Node(source)::Node(target)::plan)), Some(repl))))
            else
                repl.Post(Result([]))

            return! loop state self
        | _ -> failwith ""
    }
    EntityActor(name, loop (defaultArg state <| zeroProps()))
    |> AgentRegistry.start
    |> AgentRegistry.post(Initialize(state))

let createQueryActor _ name (db:IGraphDb) =
    let resolveNode (ctx:QueryContext) (node:Entity) = async {
        let id =
            match ctx.TryFind(node.Var) with
            | Some(id) -> id
            | _ -> "node-" + System.Guid.NewGuid().ToString()
        let props =
            node.Properties.Add("_id", String(id))
        let! ref = db.GetOrSpawnNode id props
        let context = // Remember `id` in context if node is named
            if node.Var = "" then ctx
            else ctx |> Map.add node.Var id
        return context, ref, props
    }
    let ensureRelation (ctx:QueryContext) (relation:Entity) = async {
        let id =
            match ctx.TryFind("_id") with
            | Some(id) -> id
            | _ -> "relation-" + System.Guid.NewGuid().ToString()
        let props =
            relation.Properties.Add("_id", String(id))
        let! ref = db.GetOrSpawnRelation id props
        let context = // Remember `id` in context if node is named
            if relation.Var = "" then ctx
            else ctx |> Map.add relation.Var id
        return context, ref, { relation with Properties = props }
    }

    let rec loop count res (reply:IAsyncReplyChannel<QueryResult> option) (self:QueryActor) = async {
        let! msg = self.Receive()
        match msg with
        | ProcessOnQueryNode(req, repl) ->
            let plan = mkPlan None req.Query
            let rec runPlan plan patterns ctx = async {
                match plan with
                | Return(ret, res, Some(plan), _) ->
                    match plan with
                    | Single(Node(node)) ->
                        let! count = db.QueryNode (fun r -> GetActors(Query(Return(ret, res, Some(plan), Some(self))), r))
                        return count, res
                    | SequenceLeaf(Relation(relation)::_) ->
                        let! count = db.QueryRelation (fun r -> GetActors(Query(Return(ret, res, Some(plan), Some(self))), r))
                        return count, res
                    | ParallelLeaf(plans) ->
                        let! runs = plans |> List.map (fun plan -> runPlan (Return(ret, res, Some(Single(plan)), Some(self))) patterns ctx) |> Async.Parallel
                        let count = runs |> Array.sumBy (fun (c, _) -> c)
                        return count, res
                    | _ ->
                        return raise (System.Exception("Not implemented"))
                | Create(pat, initPlan, nextPlan) ->
                    let rec ensureCreate ctx pattern =
                        async {
                            match pattern with
                            | NodePattern(node) ->
                                let! ctx', _, props = resolveNode ctx node
                                return ctx', NodePattern({node with Properties = props})
                            | RelationPattern(source, relation, target) ->
                                let! ctx, _, props = resolveNode ctx source
                                let! ctx, _, props' = resolveNode ctx target

                                let relation = { relation with Properties = relation.Properties.Add("_sourceId", props.["_id"]).Add("_targetId", props'.["_id"]) }

                                let! ctx, _, relation = ensureRelation ctx relation

                                return ctx, RelationPattern({ source with Properties = props }, relation, { target with Properties = props' })
                        }
                    let! (ctx', respPattern) =
                        ensureCreate ctx pat

                    let patterns = Pattern(respPattern)::patterns

                    match nextPlan with
                    | Some(nextPlan) ->
                        let! c, r = runPlan nextPlan patterns ctx'
                        return count+c, r
                    | None ->
                        repl.Reply({ Match = patterns ; Context = ctx' })
                        return count, res
                | Set(var, property, exp, Some(plan), Some(Return(Var(ret)::[], _, _, _))) when var = ret ->
                    match plan with
                    | Single(Node(node)) ->
                        let! count = db.QueryNode (fun r -> GetActors(Query(Set(var, property, exp, Some(plan), Some(Return(Var(ret)::[], [], None, Some(self))))), r))
                        return count, res
                    | SequenceLeaf(Relation(relation)::_) ->
                        let! count = db.QueryRelation (fun r -> GetActors(Query(Set(var, property, exp, Some(plan), Some(Return(Var(ret)::[], [], None, Some(self))))), r))
                        return count, res
                    | ParallelLeaf(plans) ->
                        let! runs = plans |> List.map (fun plan -> runPlan (Set(var, property, exp, Some(Single(plan)), Some(Return(Var(ret)::[], [], None, Some(self))))) patterns ctx) |> Async.Parallel
                        let count = runs |> Array.sumBy (fun (c, _) -> c)
                        return count, res
                    | _ ->
                        return raise (System.Exception("Not implemented"))
                | _ ->
                    return raise (System.Exception("Not implemented"))
            }
            let! count, res =
                match plan with
                | Some(plan) -> runPlan plan [] req.Context
                | _ -> failwith "not implemented"

            if count = 0 then
                reply.Value.Reply({ Match = res; Context = EmptyContext})
                return ()            
            else
                return! loop count res (Some(repl)) self
        | Result(list) ->
            let res = list @ res
            let count = count - 1
            if count = 0 then
                reply.Value.Reply({ Match = res; Context = EmptyContext})
                return ()
            else
                return! loop count res reply self
        | MessagesResult(msgCount) ->
            let count = count + msgCount
            return! loop count res reply self
    }
    QueryActor(name, loop 0 [] None) |> AgentRegistry.start

let createRegistryActor serialize createActor state name (db:IGraphDb) =

    let indexName = name

    let rec loop (registry:RegistryState<'a>) (self:RegistryActor<'a, 'b>) = async {

        let tryGetOrSpawn name (state:'b option) (repl:IAsyncReplyChannel<string * Agent<'a>>) =
            match registry.ContainsKey name with
            | true ->
                let actor = registry.[name]
                (repl :> IAsyncReplyChannel<_>).Reply(name, actor)
            | false ->
                let actor = createActor (state) name db
                if serialize then
                    addToIndex indexName name
                (repl :> IAsyncReplyChannel<_>).Reply(name, actor)
                registry.Add(name, actor)

        let! msg = self.Receive()
        match msg with
        | GetOrSpawnActor(id, state, repl) ->
            tryGetOrSpawn id state repl
            return! loop registry self
        | GetActors(m, repl) ->
            (repl :> IAsyncReplyChannel<_>).Reply(registry.Count)
            registry |> Seq.iter (fun kvp -> kvp.Value.Post m)
            return! loop registry self
    }
    let registry : RegistryState<'a> =
        if serialize then
            let index, count = getIndex indexName
            let dic = new RegistryState<'a>(count)
            index
            |> Seq.iter (fun r -> dic.Add(r.Id, createActor None r.Id db))
            dic
        else
            match state with
            | Some(state) -> state
            | None -> new RegistryState<'a>()
    RegistryActor<'a, 'b>(indexName, loop registry) |> AgentRegistry.start