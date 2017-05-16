namespace WebSharper.Suave.Tests

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI.Next
open WebSharper.UI.Next.Server

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About

module Templating =
    open WebSharper.UI.Next.Html

    [<Literal>]
    let TemplatePath = __SOURCE_DIRECTORY__ + "/Main.html"

    type MainTemplate = Templating.Template<TemplatePath>

    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint : Doc list =
        let ( => ) txt act =
             liAttr [if endpoint = act then yield attr.``class`` "active"] [
                aAttr [attr.href (ctx.Link act)] [text txt]
             ]
        [
            li ["Home" => EndPoint.Home]
            li ["About" => EndPoint.About]
        ]

    let Main ctx action title body =
        Content.Page(
            MainTemplate.Doc(
                title = title,
                menubar = MenuBar ctx action,
                body = body
            )
        )

module Site =
    open WebSharper.UI.Next.Html

    let HomePage ctx =
        Templating.Main ctx EndPoint.Home "Home" [
            h1 [text "Say Hi to the server!"]
            div [client <@ Client.Main() @>]
        ]

    let AboutPage ctx =
        Templating.Main ctx EndPoint.About "About" [
            h1 [text "About"]
            p [text "This is a template WebSharper client-server application."]
        ]

    let Main =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            | EndPoint.Home -> HomePage ctx
            | EndPoint.About -> AboutPage ctx
        )

module Main =

    open System
    open System.IO
    open WebSharper.Owin
    open WebSharper.Suave
    open global.Suave

    let run rootDirectory =
        WebSharperOptions(
            Sitelet = Some Site.Main,
            ServerRootDirectory = rootDirectory
        ).ToWebPart(
            Continuation = request (fun req ->
                RequestErrors.NOT_FOUND ("Not found: " + req.url.PathAndQuery))
        )
        |> startWebServer defaultConfig
        0

    [<EntryPoint>]
    let main = function
        | [||] -> run (Directory.GetCurrentDirectory())
        | [| root |] -> run root
        | _ ->
            eprintfn "Usage: WebSharper.Suave.Tests [ROOT_DIR]"; exit 1
            0
