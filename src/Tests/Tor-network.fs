module Tor_network

open System
open System.IO
open Expecto
open GraphActor.Core.gGRAFSharp
open GraphDriver
open GraphActor.Core.gGRAFSharp.Parser
open Types

let printContext ctx =
    for (var,id) in Map.toSeq ctx do
        printfn "\t'%s'->'%s'" var id

let [<Tests>] torNetwork =
    testCase "Parse `Tor-network.cypher`" (fun _ ->
        // https://neo4j.com/graphgist/2dcbbe7f-a2e4-4a2b-ad1e-6c0a26efd6fd#listing_category=web-amp-social
        let lines =
            "../GraphActor.Desktop/db-setup-cypher/Tor-network.cypher"
            |> File.ReadAllLines
            |> Array.filter (fun s -> not (s.StartsWith("//")))

        let graph = GraphDB() :> IGraphDb

        String.Join("\n", lines)
              .Split([|"\n\n"|], StringSplitOptions.RemoveEmptyEntries)
        |> Array.fold (fun vars x ->
            printfn "========"
            printfn "%s" (x.Trim())
            printfn "--------"
            printfn "Context:"
            printContext vars
            printfn "--------"
            let query = Parser.parse x
            printfn "Query:\n %A" query
            printfn "--------"
            let response =
                // We pass variables from prev query to the next one to save the context
                let task = graph.Run({Query = query; Context=vars})
                task.Wait()
                task.Result
            printfn "Response:\n %A" response

            response.Context
          ) Map.empty
        |> printContext

        // TODO: verify graph
    )
