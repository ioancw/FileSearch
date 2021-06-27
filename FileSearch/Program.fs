module FileSearch

open System

open Query
open Common
open Argu

type Arguments =
    | Folder of path: string
    | Find of query: string
    interface IArgParserTemplate with
        member t.Usage =
            match t with
            | Folder _ -> "Specify folder to search."
            | Find _ -> "What to search for."

[<EntryPoint>]
let main args =

    let parser =
        ArgumentParser.Create<Arguments>(programName = "FileSearch")

    let parsedArgs = parser.Parse()
    let folder = parsedArgs.GetResult Folder
    let find = parsedArgs.GetResult Find

    let currentColour = Console.ForegroundColor

    do apply Colours.gray

    do searchFor folder find

    do apply currentColour
    0 // return an integer exit code
