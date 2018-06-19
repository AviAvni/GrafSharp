module gGRAFSharpParseAndExecuteTests

open System
open System.IO
open Expecto
open GraphActor.Core.gGRAFSharp
open GraphDriver
open Types

let printContext ctx =
    for (var,id) in Map.toSeq ctx do
        printfn "\t'%s'->'%s'" var id

let [<Tests>] torNetwork =
    let root = "../GraphActor.Desktop/db-setup-cypher"
    [
        Path.Combine(root, "Tor network.cypher")
    ]
    |> List.map (fun path ->
        test (Path.GetFileName(path)) {

            let lines =
                File.ReadAllLines (path)
                |> Array.filter (fun s -> not (s.StartsWith("//")))

            let graph = GraphDB() :> IGraphDb

            String.Join("\n", lines)
                  .Split([|"\n\n"|], StringSplitOptions.RemoveEmptyEntries)
            |> Array.fold (fun vars x ->
                //printfn "========"
                //printfn "%s" (x.Trim())
                //printfn "--------"
                //printfn "Context:"
                //printContext vars
                //printfn "--------"
                let query = Parser.parse x
                //printfn "Query:\n %A" query
                //printfn "--------"
                let response =
                    // We pass variables from prev query to the next one to save the context
                    let task = graph.Run({Query = query; Context=vars})
                    task.Wait()
                    task.Result
                //printfn "Response:\n %A" response

                response.Context
              ) Map.empty
            |> printContext
        }
    )
    |> testList "gGRAF# Parse & Execute"
