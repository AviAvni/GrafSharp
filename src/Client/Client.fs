module Client

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch

open Shared

open Fulma
open Fable.Core
open Fable.Import.React
open Fable.Core.JsInterop


type Model = { Query : string; Graph : Expr list }

type Msg =
| Query
| QueryChanged of string
| FetchGraph of Result<Expr list, exn>
| Init of Result<string, exn>

type Strength = { collide : int }

type SimulationOptions = { height : int; width : int; animate : bool; strength : Strength; alpha : int }

type Node = { id : string }

type Link = { source : string; target : string }

[<RequireQualifiedAccess>]
type ForceGraphProps =
    | SimulationOptions of SimulationOptions
    | HighlightDependencies of bool
    static member Custom(key: string, value: obj): ForceGraphProps = unbox(key, value)

[<RequireQualifiedAccess>]
type ForceGraphNodeProps =
    | Fill of string
    | Node of Node
    | R of int
    | Cx of int
    | Cy of int
    static member Custom(key: string, value: obj): ForceGraphNodeProps = unbox(key, value)

[<RequireQualifiedAccess>]
type ForceGraphLinkProps =
    | Link of Link
    static member Custom(key: string, value: obj): ForceGraphLinkProps = unbox(key, value)

let forceGraph (props: ForceGraphProps list) (children : ReactElement list) : ReactElement =
    ofImport "InteractiveForceGraph" "react-vis-force" (keyValueList CaseRules.LowerFirst props) children
let forceGraphNode (props: ForceGraphNodeProps list): ReactElement = 
    ofImport "ForceGraphNode" "react-vis-force" (keyValueList CaseRules.LowerFirst props) []
    
let forceGraphLink (props: ForceGraphLinkProps list): ReactElement = 
    ofImport "ForceGraphLink" "react-vis-force" (keyValueList CaseRules.LowerFirst props) []

let init () : Model * Cmd<Msg> =
    let model = { Query = ""; Graph = [] }
    let cmd =
        Cmd.ofPromise
            (fetchAs<string> "/api/ping")
            []
            (Ok >> Init)
            (Error >> Init)
    model, cmd

let queryGraph query model : Model * Cmd<Msg> =
    let cmd =
        Cmd.ofPromise
            (fetchAs<Expr list> (sprintf "/api/query?query=%s" query))
            []
            (Ok >> FetchGraph)
            (Error >> FetchGraph)
    model, cmd

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    match model,  msg with
    | _, Query -> queryGraph model.Query model
    | _, QueryChanged query -> { model with Query = query }, Cmd.none
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
                    [ Input.text [ Input.Option.OnChange (fun x -> dispatch (QueryChanged x.Value)) ]
                      button "Run" (fun _ -> dispatch Query) ]
                Columns.columns []
                    [ Column.column [] [ forceGraph [ ForceGraphProps.SimulationOptions { height = 500; width = 500; animate = true; strength = { collide = 8 }; alpha = 1 }; ForceGraphProps.HighlightDependencies true ] 
                        [ for expr in model.Graph do
                            match expr with
                            | Pattern(NodePattern node) ->
                                yield forceGraphNode [ ForceGraphNodeProps.Fill "blue"; ForceGraphNodeProps.Node { id = node.Id }; ForceGraphNodeProps.R 10 ]
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
