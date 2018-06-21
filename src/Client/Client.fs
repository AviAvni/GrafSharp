module Client

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch

open Shared

open Fulma
open ForceGraph
open Fulma

type Model = 
    { Query : string
      Graph : Expr list
      Examples : string list
      SelectedExample : string }

type Msg =
| Query
| Load
| QueryChanged of string
| ExampleChanged of string
| FetchExamples of Result<string list, exn>
| FetchGraph of Result<Expr list, exn>
| Init of Result<string, exn>

let init () : Model * Cmd<Msg> =
    let model = { Query = ""; Graph = []; Examples = []; SelectedExample = "" }
    let cmd =
        Cmd.batch 
            [ Cmd.ofPromise
                  (fetchAs<string> "/ping")
                  []
                  (Ok >> Init)
                  (Error >> Init)
              Cmd.ofPromise
                  (fetchAs<string list> "/api/getExamplesName")
                  []
                  (Ok >> FetchExamples)
                  (Error >> FetchExamples) ]                
    model, cmd

let queryGraph query model : Model * Cmd<Msg> =
    let cmd =
        Cmd.ofPromise
            (fetchAs<Expr list> (sprintf "/api/query?query=%s" query))
            []
            (Ok >> FetchGraph)
            (Error >> FetchGraph)
    model, cmd

let loadGraph example model : Model * Cmd<Msg> =
    let cmd =
        Cmd.ofPromise
            (fetchAs<Expr list> (sprintf "/api/loadExample?Example=%s" example))
            []
            (Ok >> FetchGraph)
            (Error >> FetchGraph)
    model, cmd        

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    match model,  msg with
    | _, Query -> queryGraph model.Query model
    | _, Load -> loadGraph model.SelectedExample model
    | _, QueryChanged query -> { model with Query = query }, Cmd.none
    | _, ExampleChanged example -> { model with SelectedExample = example }, Cmd.none
    | _, FetchExamples (Ok examples) -> { model with Examples = examples }, Cmd.none
    | _, FetchGraph (Ok graph) -> { model with Graph = graph }, Cmd.none
    | _, Init (Ok x) -> model, Cmd.none
    | _ -> model, Cmd.none

let safeComponents =
    let intersperse sep ls =
        List.foldBack (fun x -> function
            | [] -> [x]
            | xs -> x::sep::xs) ls []

    let components =
        [
            "Saturn", "https://saturnframework.github.io/docs/"
            "Fable", "http://fable.io"
            "Elmish", "https://elmish.github.io/elmish/"
            "Fulma", "https://mangelmaxime.github.io/Fulma"
        ]
        |> List.map (fun (desc,link) -> a [ Href link ] [ str desc ] )
        |> intersperse (str ", ")
        |> span [ ]

    p [ ]
        [ strong [] [ str "SAFE Template" ]
          str " powered by: "
          components ]

let button txt onClick =
    Button.button
        [ Button.IsFullWidth
          Button.Color IsPrimary
          Button.OnClick onClick ]
        [ str txt ]

let view (model : Model) (dispatch : Msg -> unit) =
    div []
        [ Navbar.navbar [ Navbar.Color IsPrimary ]
            [ Navbar.Item.div [ ]
                [ Heading.h2 [ ]
                    [ str "Graph Database" ] ] ]

          Container.container []
              [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ Input.text [ Input.OnChange (fun x -> dispatch (QueryChanged x.Value)) ]
                      button "Run" (fun _ -> dispatch Query)
                      select [ OnChange (fun x -> dispatch (ExampleChanged x.Value)) ] (model.Examples |> List.map (fun x -> option [Label x; Value x] []))
                      button "Load" (fun _ -> dispatch Load) ]
                Columns.columns []
                    [ Column.column [] [ forceGraph [ ForceGraphProps.SimulationOptions { height = 500; width = 500; animate = true; strength = { collide = 8 }; alpha = 1 }; ForceGraphProps.HighlightDependencies true ] 
                        [ for expr in model.Graph |> List.filter (fun e -> match e with | Pattern(NodePattern _) -> true | _ -> false) do
                            match expr with
                            | Pattern(NodePattern node) ->
                                yield forceGraphNode [ ForceGraphNodeProps.Fill "blue"; ForceGraphNodeProps.Node { id = node.Id }; ForceGraphNodeProps.R 10 ]
                            | _ -> ()
                          for expr in model.Graph |> List.filter (fun e -> match e with | Pattern(RelationPattern _) -> true | _ -> false) do
                            match expr with
                            | Pattern(RelationPattern(source, relation, target)) ->
                                yield forceGraphLink [ ForceGraphLinkProps.Link { source = source.Id; target = target.Id } ]
                            | _ -> () ] ] ] ]

          Footer.footer [ ]
                [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ safeComponents ] ] ]


#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
