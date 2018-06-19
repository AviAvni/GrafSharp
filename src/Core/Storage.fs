module Storage

open Shared
open ActorsTypes

let enableStorage = true

let private mapper = LiteDB.FSharp.FSharpBsonMapper()

let private nodesDb = new LiteDB.LiteDatabase("Nodes.db", mapper)
let private relationsDb = new LiteDB.LiteDatabase("Relations.db", mapper)
let private indexeDb = new LiteDB.LiteDatabase("Index.db", mapper)

let private nodesCollection = nodesDb.GetCollection<Entity>()
let private relationsCollection = relationsDb.GetCollection<Entity>()

let saveNode (node:Props) =
    if enableStorage then
        nodesCollection.Upsert { Var = ""; Properties = node } |> ignore
    node

let getNodeById (id:string) =
    if enableStorage then
        let id = LiteDB.BsonValue(string id)
        let res = nodesCollection.FindById(id)
        res.Properties
    else
        Map.empty

let saveRelation (relation:Props) =
    if enableStorage then
        relationsCollection.Insert({ Var = ""; Properties = relation }) |> ignore
    relation

let getRelationById (id:string) =
    if enableStorage then
        let id = LiteDB.BsonValue(string id)
        let res = relationsCollection.FindById(id)
        res.Properties
    else
        Map.empty

let addToIndex (indexName:string) (item:string) =
    if enableStorage then
        let indexCollection = indexeDb.GetCollection<Record>(indexName)
        indexCollection.Insert({ Record.Id = item }) |> ignore
    ()

let getIndex (indexName:string) =
    if enableStorage then
        let indexCollection = indexeDb.GetCollection<Record>(indexName)
        indexCollection.FindAll(), indexCollection.Count()
    else
        Seq.empty, 0