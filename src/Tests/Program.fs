module Program

open Expecto
open System

[<EntryPoint>]
let main args =
    let config =
        { defaultConfig with
            ``parallel`` = false
            verbosity = Logging.LogLevel.Verbose }
    runTestsInAssembly config args
    //runTests config PerformanceTests.performance