// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

module SuperDumper

open Suave
open Suave.Types
open Suave.Web
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.Successful
open Suave.Http.Writers
open System.IO
open System
open Logary
open Logary.Targets
open Logary.Suave
open Logary.Configuration

let logManager =
    withLogary' "DocumentationServer" (
        withTargets [
            Console.create Console.empty "console"
        ]>>
        withRules [
            Rule.createForTarget "console"
        ])

let suaveConfig =
    { defaultConfig with
        logger = SuaveAdapter(logManager.GetLogger "Suave") }

module DocumentationServer = 
    open Chiron
    open Suave.Http
    open Suave.Http.Applicatives
    open Suave.Http.Successful
    open Suave.Types

    module DTOs = 
        open Chiron.Operators

        type DocumentPath = 
            { path : string
              filename : string }     

            static member ToJson (d : DocumentPath) = 
                Json.write "path" d.path
                *> Json.write "filename" d.filename
                                                   
    open DTOs
    
    module Utils =                     
        let rec getAllFiles dir pattern =
            seq { yield! Directory.EnumerateFiles(dir, pattern)
                  for d in Directory.EnumerateDirectories(dir) do
                      yield! getAllFiles d pattern }

        let convertToDocumentPath rootPath (completePath : string) =
            let noRoot = completePath.Replace(rootPath, "")    
            { path = Path.GetDirectoryName(noRoot); filename = Path.GetFileName(noRoot) }
    
        let allMdFiles ctx = async {            
            let testdataFolder = @"..\..\testdata\"
            let allFiles = getAllFiles testdataFolder "*.md"         
                            |> Seq.map (convertToDocumentPath testdataFolder)
                            |> Seq.toList
            return! OK (allFiles |> Json.serialize |> Json.format) ctx }            
        
    let api =         
        setMimeType "application/json; charset=utf-8" >>= choose [
            path "/api/documentationserver/filelist" >>= Utils.allMdFiles
        ]
            
[<EntryPoint>]
let main argv =     
    printfn "%A" argv
    startWebServer suaveConfig <| 
        choose [
            DocumentationServer.api
        ]

    0 // return an integer exit code

