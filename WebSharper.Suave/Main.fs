namespace WebSharper.Suave

open global.Owin
open System.Net
open Suave
open Suave.Types
open Suave.Http.Successful
open Suave.Web
open Suave.Owin
open WebSharper.Owin

type WebSharperAdapter =

    static member ToWebPart(app, ?RootDirectory: string, ?BinDirectory: string) =
        let rootDirectory = defaultArg RootDirectory ""
        let binDirectory =
            match BinDirectory with
            | Some d -> d
            | None -> typeof<WebSharperAdapter>.Assembly.Location
        SiteletMiddleware<_>.AsMidFunc(Options.Create(rootDirectory, binDirectory), app)
        |> Suave.Owin.OwinApp.ofMidFunc
