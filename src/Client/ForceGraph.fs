module ForceGraph

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import.React
open Fable.Helpers.React

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