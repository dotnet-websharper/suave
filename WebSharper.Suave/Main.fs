namespace WebSharper.Suave

open global.Owin
open System.Net
open System.IO
open Suave
open Suave.Filters
open Suave.Http
open Suave.Web
open Suave.Owin
open WebSharper.Owin

[<AutoOpen>]
module internal Utils =

    let prependWith (c: string) (s: string) =
        if s.StartsWith c then s else c + s

type WebSharperAdapter =

    static member ToWebPart(app, ?RootDirectory: string, ?BinDirectory: string, ?RequestPathBase: string) =
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
        let pathBase =
            match RequestPathBase with
            | None -> ""
            | Some p -> p.TrimEnd('/')
        let fmt p = PrintfFormat<_,_,_,_,_>(pathBase + "/" + p + "/WebSharper/%s")
        choose [
            pathScan (fmt "Scripts") (Files.browseFile (Path.Combine(rootDirectory, "Scripts", "WebSharper")))
            pathScan (fmt "Content") (Files.browseFile (Path.Combine(rootDirectory, "Content", "WebSharper")))
            SiteletMiddleware<_>.AsMidFunc(Options.Create(rootDirectory, binDirectory), app)
            |> Suave.Owin.OwinApp.ofMidFunc pathBase
        ]
