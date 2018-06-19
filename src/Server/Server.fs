open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.Serialization
open Saturn

open Shared
open ActorsTypes
open GraphDriver
open Language
open Microsoft.Extensions.Logging

let publicPath = Path.GetFullPath "../Client/public"
let port = 8085us

let mutable db : IGraphDb = GraphDB() :> IGraphDb
let mutable context = EmptyContext

let webApp = scope {
    get "/ping" (text "pong")

    get "/api/query" (fun next ctx ->
        task {
            let query = ctx.GetQueryStringValue "Query"
            match query with
            | Ok query ->
                let ast = Parser.parse query
                let req = { Query = ast; Context = context }
                let! resp = db.Run(req)
                context <- resp.Context
                return! Successful.OK resp.Match next ctx
            | _ ->
                return! ServerErrors.INTERNAL_ERROR "Error" next ctx
        })

    get "/api/cypher" (fun next ctx ->
        task {
            let cypher = ctx.GetQueryStringValue "Query"
            match cypher with
            | Ok cypher ->
                let ast = Cypher.toGRAFSharpQuery cypher
                let req = { Query = ast; Context = context }
                let! resp = db.Run(req)
                context <- resp.Context
                return! Successful.OK resp.Match next ctx
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
