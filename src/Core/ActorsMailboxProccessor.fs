module ActorsMailboxProccessor

open ActorsCore

type Agent<'a>(id:string, comp:#NodeRef<'a> -> Async<unit>, ?token) as this =
    inherit NodeRef<'a>(id)
    let agent = new MailboxProcessor<'a>((fun _ -> comp this), ?cancellationToken = token)

    //let agent = new MailboxProcessor<'a>((fun _ -> comp (this :> NodeRef<'a>)), ?cancellationToken = token)

    override x.Post(msg:'a) = agent.Post(msg)
    override x.PostAndTryAsyncReply(builder) = agent.PostAndTryAsyncReply(fun rc -> builder(new ReplyChannel<_>(rc)))
    override x.PostAndAsyncReply(builder) = agent.PostAndAsyncReply(fun rc -> builder(new ReplyChannel<_>(rc)))
    override x.Receive() = agent.Receive()
    override x.Start() =
        agent.Start()
