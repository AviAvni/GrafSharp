module CypherParseTests

open System.IO
open Expecto

open GraphActor.Core.gGRAFSharp.CypherQueryTransform
open GraphDriver
open Types


let [<Tests>] cypherParseAndTransform =
    Directory.GetFiles("../GraphActor.Desktop/db-setup-cypher")
    |> List.ofArray
    |> List.filter (fun x -> not <| Path.GetFileName(x).StartsWith("_"))
    //|> List.filter (fun x -> x.IndexOf("James Bond")>0)  // TODO: for debug
    |> List.map (fun filePath ->
        let testName = Path.GetFileName(filePath)
        testAsync testName {
            let cypherText = File.ReadAllText(filePath)
            let graph = GraphDB() :> IGraphDb

            let initialCtx = async { return Map.empty}
            let! ctx =
                toGRAFSharpQueries cypherText
                |> Array.fold (fun ctxAsync query -> async {
                    //printfn "%A" query
                    let! ctx = ctxAsync
                    let! resp =
                        graph.Run({Query = query; Context=ctx})
                        |> Async.AwaitTask
                    //printfn "Response:\n %A" response
                    return resp.Context
                  }) initialCtx
            Expect.isGreaterThan (ctx.Count) 0 ">0"
        }
      )
    |> testList "ANTLR4 Cypher Parse"