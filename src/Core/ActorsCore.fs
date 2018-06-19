module ActorsCore

open System.Threading

type IAsyncReplyChannel<'a> =
    abstract Reply : 'a -> unit

type ReplyChannel<'a>(channel:AsyncReplyChannel<'a>) =
    interface IAsyncReplyChannel<'a> with
        member __.Reply(msg) = channel.Reply(msg)

[<AbstractClass>]
type NodeRef(id:string) =
    member val Id = id with get, set
    abstract Start : unit -> unit //IDisposable

[<AbstractClass>]
type NodeRef<'a>(id:string) =
    inherit NodeRef(id)
    abstract Receive : unit -> Async<'a>
    abstract Post : 'a -> unit
    abstract PostAndTryAsyncReply : (IAsyncReplyChannel<'b> -> 'a) -> Async<'b option>
    abstract PostAndAsyncReply : (IAsyncReplyChannel<'b> -> 'a) -> Async<'b>

let inline eq a b = obj.ReferenceEquals(a,b)
let inline neq a b = eq a b |> not

type Atom<'T when 'T : not struct>(value : 'T) =
    let cell = ref value
    let spinner = lazy (new SpinWait())

    let rec swap f =
        let tempValue = !cell
        if Interlocked.CompareExchange<'T>(cell, f tempValue, tempValue) |> neq tempValue then
            spinner.Value.SpinOnce()
            swap f

    member __.Value with get() = !cell
    member __.Swap (f : 'T -> 'T) = swap f

[<RequireQualifiedAccess>]
module Atom =
    let atom value = new Atom<_>(value)
    let swap (atom : Atom<_>) (f : _ -> _) = atom.Swap f

[<RequireQualifiedAccess>]
module AgentRegistry =

    let private nodes = Map.empty<string, #NodeRef list> |> Atom.atom

    let register (ref:#NodeRef<'a>) =
        match Map.tryFind ref.Id nodes.Value with
        | Some(refs) -> nodes.Swap(fun a -> Map.add ref.Id ((ref :> NodeRef) :: refs) a)
        | None -> nodes.Swap(fun a -> Map.add ref.Id [(ref :> NodeRef)] a)
        ref

    let resolve id =
        Map.tryFind id nodes.Value
        |> Option.map(List.map(fun n -> n :?> NodeRef<_>))

    let resolveSingle id =
        match Map.tryFind id nodes.Value with
        | Some(r) when r.Length = 1 -> r.Head :?> NodeRef<_> |> Some
        | _ -> None

    let spawn (ref:#NodeRef<_>) = (register ref).Start()

    let start (ref:#NodeRef<'a>) =
        spawn ref
        ref

    let postAll (refs:#seq<#NodeRef<'a>>) msg =
        refs |> Seq.iter (fun r -> r.Post(msg))

    let post msg (ref:#NodeRef<'a>) =
        ref.Post(msg)
        ref

    let postAndTryAsyncReply (refs:#seq<NodeRef<'a>>) msg =
        refs
        |> Seq.map (fun ag -> ag.PostAndTryAsyncReply(msg))
        |> Async.Parallel

    let postAndAsyncReply (refs:#seq<NodeRef<'a>>) msg = async {
            let! responses = postAndTryAsyncReply refs msg
            return responses |> Seq.choose id
        }
