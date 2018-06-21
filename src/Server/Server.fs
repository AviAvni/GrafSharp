open System.IO

open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.Serialization
open Saturn

open Shared
open ActorsTypes
open GraphDriver
open Language
open System

let publicPath = Path.GetFullPath "../Client/public"
let port = 8085us

let mutable db : IGraphDb = GraphDB() :> IGraphDb
let mutable context = EmptyContext

let runQuery ast saveContext =
    task {
        let req = { Query = ast; Context = context }
        let! resp = db.Run(req)
        if saveContext then
            context <- resp.Context
        return resp        
    }

let webApp = scope {
    get "/ping" (text "pong")

    get "/api/query" (fun next ctx ->
        task {
            let query = ctx.GetQueryStringValue "Query"
            let saveContext = ctx.GetQueryStringValue "SaveContext"
            match query, saveContext with
            | Ok query, Ok saveContext ->
                let ast = Parser.parse query
                let! resp = runQuery ast (saveContext = "true")
                return! Successful.OK resp.Match next ctx
            | _ ->
                return! ServerErrors.INTERNAL_ERROR "Error" next ctx
        })

    get "/api/cypher" (fun next ctx ->
        task {
            let cypher = ctx.GetQueryStringValue "Query"
            let saveContext = ctx.GetQueryStringValue "SaveContext"
            match cypher, saveContext with
            | Ok cypher, Ok saveContext ->
                let ast = Cypher.toGRAFSharpQuery cypher
                let! resp = runQuery ast (saveContext = "true")
                return! Successful.OK resp.Match next ctx
            | _ ->
                return! ServerErrors.INTERNAL_ERROR "Error" next ctx
        })

    get "/api/getExamplesName" (fun next ctx ->
        task {
            let files = Directory.GetFiles(Path.Combine(publicPath, "db-setup-cypher")) |> Array.map (Path.GetFileName) |> Array.toList
            return! Successful.OK files next ctx
        })

    get "/api/loadExample" (fun next ctx ->
        task {
            let example = ctx.GetQueryStringValue "Example"
            match example with
            | Ok example when File.Exists(Path.Combine(publicPath, "db-setup-cypher", example)) ->
                let queries =
                    File.ReadAllText(Path.Combine(publicPath, "db-setup-cypher", example))
                        .Split([| ";" |], StringSplitOptions.RemoveEmptyEntries)

                let! results =
                    queries
                    |> Array.map (fun cypher -> 
                        let ast = Cypher.toGRAFSharpQuery cypher
                        runQuery ast true |> Async.AwaitTask)
                    |> Async.Parallel
                    |> Async.StartAsTask

                let resp =
                    results
                    |> Array.map (fun result -> result.Match)
                    |> Seq.collect id
                    |> Seq.toList

                return! Successful.OK resp next ctx
            | _ ->
                return! ServerErrors.INTERNAL_ERROR "Error" next ctx
        })    
}

let configureSerialization (services:IServiceCollection) =
    let fableJsonSettings = Newtonsoft.Json.JsonSerializerSettings()
    fableJsonSettings.Converters.Add(Fable.JsonConverter())
    services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer fableJsonSettings)

let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    router webApp
    memory_cache
    use_static publicPath
    service_config configureSerialization
    use_gzip
}

run app
