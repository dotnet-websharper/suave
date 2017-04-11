System.Environment.SetEnvironmentVariable("INTELLIFACTORY", "")

#load "tools/includes.fsx"

open IntelliFactory.Core
open IntelliFactory.Build

let bt =
    BuildTool().PackageId("Zafir.Suave")
        .VersionFrom("Zafir")
        .WithFSharpVersion(FSharpVersion.FSharp40)
        .WithFramework(fun x -> x.Net45)

let suaveVersion = NuGetResolver.Current.Find(bt).FindLatestVersion("Suave").Value.ToString()
let suaveDll = sprintf "%s/packages/Suave.%s/lib/net40/Suave.dll" __SOURCE_DIRECTORY__ suaveVersion

let main =
    bt.Zafir.Library("WebSharper.Suave")
        .SourcesFromProject()
        .References(fun ref ->
            [
                ref.NuGet("Suave").Latest().ForceFoundVersion().BuildTimeOnly().Reference()
                ref.File(suaveDll)
                ref.NuGet("FSharp.Core").Version("[4.0.0.1]").ForceFoundVersion().Reference()
                ref.NuGet("Zafir.Owin").Latest(true).ForceFoundVersion().Reference()
                ref.NuGet("Mono.Cecil").ForceFoundVersion().Reference()
            ])

let tests =
    bt.Zafir.Executable("WebSharper.Suave.Tests")
        .SourcesFromProject()
        .References(fun ref ->
            [
                ref.Project(main)
                ref.File(suaveDll)
                ref.NuGet("FSharp.Core").Version("[4.0.0.1]").ForceFoundVersion().Reference()
                ref.NuGet("Zafir.Owin").Latest(true).ForceFoundVersion().Reference()
                ref.NuGet("Zafir.UI.Next").Latest(true).ForceFoundVersion().Reference()
                ref.NuGet("Zafir.Testing").Latest(true).ForceFoundVersion().Reference()
                ref.NuGet("Mono.Cecil").Reference()
            ])

bt.Solution [
    main
    tests

    bt.NuGet.CreatePackage()
        .Configure(fun configuration ->
            { configuration with
                Title = Some "Zafir.Suave"
                LicenseUrl = Some "http://websharper.com/licensing"
                ProjectUrl = Some "https://github.com/intellifactory/websharper.suave"
                Description = "WebSharper adaptor for Suave."
                Authors = [ "IntelliFactory" ]
                RequiresLicenseAcceptance = true })
        .Add(main)
        .AddDependency("Suave", suaveVersion)
]
|> bt.Dispatch
