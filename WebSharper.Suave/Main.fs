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
open System.Threading.Tasks

[<AutoOpen>]
module internal Utils =

    let prependWith (c: string) (s: string) =
        if s.StartsWith c then s else c + s

type WebSharperAdapter =

    /// <summary>
    /// Run a WebSharper sitelet as a Suave WebPart.
    /// </summary>
    /// <param name="app">The sitelet.</param>
    /// <param name="RootDirectory">The root of the application. (default: working directory)</param>
    /// <param name="BinDirectory">The directory containing the application's binaries. (default: WebSharper.Suave.dll's location)</param>
    /// <param name="RequestPathBase">The prefix under which to serve the application. (default: "")</param>
    /// <param name="Continuation">The WebPart to run on unmatched URLs. (default: RequestErrors.NOT_FOUND "File not found")</param>
    /// <param name="ServeResources">Whether to also serve Script and Content resources. (default: true)</param>
    static member ToWebPart
        (
            app,
            ?RootDirectory: string,
            ?BinDirectory: string,
            ?RequestPathBase: string,
            ?Continuation: WebPart,
            ?ServeResources: bool
        ) =
        let rootDirectory =
            match RootDirectory with
            | Some d -> d
            | None -> System.IO.Directory.GetCurrentDirectory()
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
        let midFunc = SiteletMiddleware<_>.AsMidFunc(Options.Create(rootDirectory, binDirectory), app)
        let app (env: OwinEnvironment) =
            midFunc
                .Invoke(fun env -> Task.Run(fun () -> env.["owin.ResponseStatusCode"] <- 404))
                .Invoke(env)
            |> Async.AwaitTask
        let siteletWebPart =
            match Continuation with
            | Some cont -> Suave.Owin.OwinApp.ofAppWithContinuation pathBase app cont
            | None -> Suave.Owin.OwinApp.ofApp pathBase app
        match defaultArg ServeResources true with
        | true ->
            choose [
                pathScan (fmt "Scripts") (Files.browseFile (Path.Combine(rootDirectory, "Scripts", "WebSharper")))
                pathScan (fmt "Content") (Files.browseFile (Path.Combine(rootDirectory, "Content", "WebSharper")))
                siteletWebPart
            ]
        | false -> siteletWebPart
