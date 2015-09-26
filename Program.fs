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

type sökväg = { sökväg : string; filnamn: string }

let convertToSökväg grund (helSökväg : string) =
    let utanGrund = helSökväg.Replace(grund, "")
    let s = { sökväg = utanGrund; filnamn = grund }
    s 

[<EntryPoint>]
let main argv = 
    getAllFiles "c:\\temp\\doc" "*.txt"     
    |> Seq.map (convertToSökväg "c:\\temp\\doc\\")
    |> Seq.cast 
    |> printfn "%A"
    printfn "%A" argv
    startWebServer defaultConfig app

    0 // return an integer exit code

