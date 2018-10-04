// $begin{copyright}
//
// This file is part of WebSharper
//
// Copyright (c) 2008-2018 IntelliFactory
//
// Licensed under the Apache License, Version 2.0 (the "License"); you
// may not use this file except in compliance with the License.  You may
// obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
// implied.  See the License for the specific language governing
// permissions and limitations under the License.
//
// $end{copyright}
namespace WebSharper.Suave.Tests

open WebSharper

module Rpc =
    open WebSharper.Web

    [<Rpc>]
    let GetLoggedInUser() =
        Remoting.GetContext().UserSession.GetLoggedInUser()

    [<Rpc>]
    let LoginAs username =
        Remoting.GetContext().UserSession.LoginUser(username)

    [<Rpc>]
    let Logout() =
        Remoting.GetContext().UserSession.Logout()

[<JavaScript>]
module Client =
    open WebSharper.JavaScript
    open WebSharper.UI
    open WebSharper.UI.Html
    open WebSharper.UI.Client

    let Main() =
        // Try RPC through CORS: browse 127.0.0.1:8080 and RPC to localhost:8080
        Remoting.EndPoint <- "http://localhost:8080"
        Rpc.GetLoggedInUser()
        |> View.ConstAsync
        |> Doc.BindView (function
            | Some username ->
                div [] [
                    text ("Logged in as " + username)
                    button [
                        on.click (fun _ _ ->
                            async {
                                do! Rpc.Logout()
                                JS.Window.Location.Reload()
                            }
                            |> Async.Start)
                    ] [text "Log out"]
                ]
            | None ->
                let username = Var.Create "testUser"
                div [] [
                    Doc.Input [] username
                    button [
                        on.clickView username.View (fun _ _ username ->
                            async {
                                do! Rpc.LoginAs username
                                JS.Window.Location.Reload()
                            }
                            |> Async.Start)
                    ] [text "Log in"]
                ]
        )

module Main =

    open System
    open System.IO
    open global.Suave
    open WebSharper.Owin
    open WebSharper.Suave
    open WebSharper.Sitelets
    open WebSharper.UI.Html

    type Endpoint =
        | Tests of WebSharper.Tests.Website.Content.FullAction
        | Home

    let Website =
        WebSharper.Tests.Website.Content.Main true
        // Sitelet.Sum [
        //     Sitelet.EmbedInUnion <@ Tests @> WebSharper.Tests.Website.Content.Main
        //     Sitelet.Content "" Home (fun ctx ->
        //         Content.Page [
        //             div [] [
        //                 a [
        //                     WebSharper.Tests.Website.Actions.Home
        //                     |> WebSharper.Tests.Website.Content.FullAction.Site
        //                     |> Tests
        //                     |> ctx.Link
        //                     |> attr.href
        //                 ] [text "Go to test suites"]
        //             ]
        //             div [] [text "Test RPC with CORS (browse this page at 127.0.0.1:8080):"]
        //             div [] [client <@ Client.Main() @>]
        //         ]
        //     )
        // ]

    let run rootDirectory =
        WebSharper.Web.Remoting.AddAllowedOrigin "http://127.0.0.1:8080"
        WebSharperOptions(
            Sitelet = Some Website,
            ServerRootDirectory = DirectoryInfo(rootDirectory).FullName
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
