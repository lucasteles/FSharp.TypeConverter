#load "./helpers.fsx"

open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Helpers

initEnviroment ()

help "Run dotnet clean in everty project"

target "clean" (fun _ -> !! "**/bin" ++ "**/obj" |> Shell.cleanDirs)

help "Run dotnet build in every project"

target "build" (fun _ ->
    solutionDir
    |> DotNet.build (fun c ->
        { c with
            Configuration = DotNet.BuildConfiguration.Release }))

help "Run dotnet restore in everty project"

target "restore" (fun _ ->
    DotNet.restore id |> ignore
    DotNet.exec id "tool restore" |> ignore)

help "check code formatting with fantomas"
target "lint" (run fantomasCheck)

help "format code"
target "format" (run fantomasFormat)

"clean" ?=> "restore" ==> "build"
"restore" ==> "lint"
"restore" ==> "format"

runWithDefaultTarget "help"
