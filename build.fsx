System.Environment.SetEnvironmentVariable("INTELLIFACTORY", "")

#load "tools/includes.fsx"

open IntelliFactory.Core
open IntelliFactory.Build

let bt =
    BuildTool().PackageId("WebSharper.Suave")
        .VersionFrom("WebSharper")
        .WithFSharpVersion(FSharpVersion.FSharp40)
        .WithFramework(fun x -> x.Net45)

let suaveVersion = NuGetResolver.Current.Find(bt).FindLatestVersion("Suave").Value.ToString()
let suaveDll = sprintf "%s/packages/Suave.%s/lib/net40/Suave.dll" __SOURCE_DIRECTORY__ suaveVersion

let main =
    bt.WebSharper.Library("WebSharper.Suave")
        .SourcesFromProject()
        .References(fun ref ->
            [
                ref.File(suaveDll)
                ref.NuGet("FSharp.Core").Version("[4.0.0.1]").ForceFoundVersion().Reference()
                ref.NuGet("WebSharper.Owin").ForceFoundVersion().Reference()
                ref.NuGet("Mono.Cecil").ForceFoundVersion().Reference()
            ])

let tests =
    bt.WebSharper.Executable("WebSharper.Suave.Tests")
        .SourcesFromProject()
        .References(fun ref ->
            [
                ref.Project(main)
                ref.File(suaveDll)
                ref.NuGet("FSharp.Core").Version("[4.0.0.1]").ForceFoundVersion().Reference()
                ref.NuGet("WebSharper.Owin").Reference()
                ref.NuGet("WebSharper.UI.Next").Reference()
                ref.NuGet("WebSharper.Testing").Reference()
                ref.NuGet("WebSharper.Tests").Reference()
                ref.NuGet("Mono.Cecil").Reference()
            ])

bt.Solution [
    main
    tests

    bt.NuGet.CreatePackage()
        .Configure(fun configuration ->
            { configuration with
                Title = Some "WebSharper.Suave"
                LicenseUrl = Some "http://websharper.com/licensing"
                ProjectUrl = Some "https://github.com/intellifactory/websharper.suave"
                Description = "WebSharper adaptor for Suave."
                Authors = [ "IntelliFactory" ]
                RequiresLicenseAcceptance = true })
        .Add(main)
        .AddDependency("Suave", suaveVersion)
]
|> bt.Dispatch
