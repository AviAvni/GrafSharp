module PerformanceTests

open Expecto
open GraphDriver
open Types
open System.Threading.Tasks
open GraphActor.Core.gGRAFSharp
open System.Diagnostics
open Expecto.Logging

let [<Tests>] performance =
    testCase "Create 1M nodes" (fun _ ->
        let logger = Logging.Log.create "perf"

        let sw = Stopwatch.StartNew()

        let graph = GraphDB() :> IGraphDb
        
        Message.event Warn "[{logger}] Init Graph {time}"
        |> Message.setField "logger" "perf"
        |> Message.setField "time" sw.Elapsed
        |> logger.logSimple

        sw.Restart()

        let queries =  
            seq { 1..1000000 }
            |> Seq.map (fun i -> {Query = Parser.parse (sprintf "CREATE (:Node { id: %d})" i); Context=EmptyContext})

        Message.event Warn "[{logger}] Create Queries {time}"
        |> Message.setField "logger" "perf"
        |> Message.setField "time" sw.Elapsed
        |> logger.logSimple

        sw.Restart()

        let task =
            queries
            |> Seq.map (fun i -> graph.Run(i))
            |> Task.WhenAll

        task.Wait()

        Message.event Warn "[{logger}] Run Queries {time}"
        |> Message.setField "logger" "perf"
        |> Message.setField "time" sw.Elapsed
        |> logger.logSimple
    )