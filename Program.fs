// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open Suave
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.Successful
open Suave.Web
open System.IO

let app = 
    choose 
        [ GET >>= choose
            [ path "/hello" >>= OK "Hello GET" 
              path "/goodby" >>= OK "Godby GET" ]
          POST >>= choose
            [ path "/hello" >>= OK "Hello POST" 
              path "/goodby" >>= OK "Goodby POST" ] ]

let rec getAllFiles dir pattern =
    seq { yield! Directory.EnumerateFiles(dir, pattern)
          for d in Directory.EnumerateDirectories(dir) do
              yield! getAllFiles d pattern }

type documentPath = { path : string; filename: string }

let convertToDocumentPath rootPath (completePath : string) =
    let noRoot = completePath.Replace(rootPath, "")    
    let s = { path = Path.GetDirectoryName(noRoot); filename = Path.GetFileName(noRoot) }
    s 

[<EntryPoint>]
let main argv = 
    let testdataFolder = @"..\..\testdata\"
    getAllFiles testdataFolder "*.md"         
    |> Seq.map (convertToDocumentPath testdataFolder)
    |> Seq.cast 
    |> printfn "%A"
    printfn "%A" argv
    startWebServer defaultConfig app

    0 // return an integer exit code

