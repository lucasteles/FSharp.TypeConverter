#r "nuget: MSBuild.StructuredLogger, 2.1.820"
#r "nuget: Fake.Core.Target, 6.0.0"
#r "nuget: Fake.DotNet.Cli, 6.0.0"
#r "nuget: Fake.IO.FileSystem, 6.0.0"
#r "nuget: Fake.Core.CommandLineParsing, 6.0.0"

open System
open System.Threading.Tasks
open Fake.Core
open Fake.DotNet
open Fake.IO

let (/) path1 path2 = Path.combine path1 path2

let solutionDir = __SOURCE_DIRECTORY__ / ".." |> Path.getFullName
let target name action = Target.create name action

let asyncTarget name (action: _ -> Task) =
    target name (action >> Async.AwaitTask >> Async.RunSynchronously)

let run f _ = f ()
let help = Target.description

let initEnviroment () =
    Environment.SetEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "en-US")

    Environment.GetCommandLineArgs()[2..]
    |> Array.toList
    |> Context.FakeExecutionContext.Create false __SOURCE_FILE__
    |> Context.RuntimeContext.Fake
    |> Context.setExecutionContext

    help "list FAKE actions"
    target "Help" <| fun _ -> Target.listAvailable ()

let runWithDefaultTarget target =
    let ctx = Target.WithContext.runOrDefaultWithArguments target
    Target.updateBuildStatus ctx

let fantomasCheck () =
    let result = DotNet.exec id "fantomas" "-r --check ."

    if result.ExitCode <> 0 then
        failwith "Some files need formatting, check output for more info"

let fantomasFormat () =
    let result = DotNet.exec id "fantomas" "-r ."

    if not result.OK then
        printfn "Errors while formatting all files: %A" result.Messages
