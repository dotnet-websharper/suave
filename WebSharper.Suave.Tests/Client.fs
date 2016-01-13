namespace WebSharper.Suave.Tests

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI.Next
open WebSharper.UI.Next.Client
open WebSharper.UI.Next.Html

[<JavaScript>]
module Client =

    let Main () =
        let rvInput = Var.Create ""
        let submit = Submitter.Create (rvInput.View.Map Some) None
        let sendResult =
            submit.View |> View.MapAsync (function
                | None -> async { return "" }
                | Some x -> Server.DoSomething x
            )
        div [
            inputAttr [Attr.Value rvInput] []
            buttonAttr [on.click (fun _ _ -> submit.Trigger())] [text "Send"]
            hr []
            h4Attr [attr.``class`` "text-muted"] [text "The server responded:"]
            divAttr [attr.``class`` "jumbotron"] [h1 [textView sendResult]]
        ]
