namespace WebSharper.Suave

open global.Owin
open System.Net
open System.IO
open Suave
open Suave.Types
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.Successful
open Suave.Web
open Suave.Owin
open WebSharper.Owin

type WebSharperAdapter =

    static member ToWebPart(app, ?RootDirectory: string, ?BinDirectory: string) =
        let rootDirectory =
            match RootDirectory with
            | Some d -> d
            | None -> "../.."
        let rootDirectory = DirectoryInfo(rootDirectory).FullName
        let binDirectory =
            match BinDirectory with
            | Some d -> d
            | None ->
                typeof<WebSharperAdapter>.Assembly.Location
                |> Path.GetDirectoryName
        choose [
            pathScan "/Scripts/WebSharper/%s" (Files.browseFile (Path.Combine(rootDirectory, "Scripts", "WebSharper")))
            pathScan "/Content/WebSharper/%s" (Files.browseFile (Path.Combine(rootDirectory, "Content", "WebSharper")))
            SiteletMiddleware<_>.AsMidFunc(Options.Create(rootDirectory, binDirectory), app)
            |> Suave.Owin.OwinApp.ofMidFunc
        ]
