#load "tools/includes.fsx"

open IntelliFactory.Build

let bt =
    BuildTool().PackageId("WebSharper.Suave")
        .VersionFrom("WebSharper")
        .WithFramework(fun x -> x.Net45)

let main =
    bt.WebSharper.Library("WebSharper.Suave")
        .SourcesFromProject()
        .References(fun ref ->
            [
                ref.NuGet("Suave").Latest(allowPreRelease = true).Reference()
                ref.NuGet("WebSharper.Owin").Reference()
            ])

bt.Solution [
    main

    bt.NuGet.CreatePackage()
        .Configure(fun configuration ->
            { configuration with
                Title = Some "WebSharper.Suave"
                LicenseUrl = Some "http://websharper.com/licensing"
                ProjectUrl = Some "https://github.com/intellifactory/websharper.suave"
                Description = "WebSharper adaptor for Suave."
                Authors = [ "IntelliFactory" ]
                RequiresLicenseAcceptance = true })
]
|> bt.Dispatch
