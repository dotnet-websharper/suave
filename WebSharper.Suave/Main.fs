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

    static member ToWebPart(app, ?RootDirectory: string) =
        let rootDirectory = defaultArg RootDirectory ""
        SiteletMiddleware<_>.AsMidFunc(Options.Create(rootDirectory), app)
        |> Suave.Owin.OwinApp.ofMidFunc
