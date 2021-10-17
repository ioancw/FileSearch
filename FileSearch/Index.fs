﻿module Index

open System
open System.IO

open Common

let fileTypeToIndex = "fs"

let getFiles fileTypeToIndex folderToIndex =
    Directory.GetFiles(folderToIndex, "*." + fileTypeToIndex, SearchOption.AllDirectories)

let delimiters =
    ".,;<>()-+!@#$%^&*?[]{}:= \t\0'\"\\/".ToCharArray()

let stopWords =
    [ "namespace"; "open"; "let"; "module" ]
    |> List.map Token

let getTokensFromFile fileName =
    File.ReadAllLines(fileName)
    |> Array.collect
        (fun line ->
            line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
            |> Array.map Token)
    
let filterStopWords tokens = 
    tokens
    |> Array.filter (fun token -> not <| List.contains token stopWords)
    
let getDistinctTokensStopWords =
    getTokensFromFile >> filterStopWords >> Array.distinct
    
/// Get counts of a given token
let getTokenFrequencies tokens =
    Array.groupBy id tokens
    |> Array.map (fun (t, ts) -> t, Array.length ts)
    
let getFrequencyOfToken tokens word =
    getTokenFrequencies tokens
    |> Array.find (fun (t, c) -> t = Token word)
    |> snd

let getDocuments (fileNames: string array) =
    fileNames
    |> Array.map
        (fun fileName ->
            { Path = Path(fileName)
              Tokens = getDistinctTokensStopWords fileName })

let tokeniseDocuments = getFiles fileTypeToIndex >> getDocuments

let distinctSnd (f, s) =
    (f, s |> Set.ofArray |> Seq.map snd |> Seq.toArray)

let documentPath =
    fun (k, ds) -> (k, ds |> Array.map (fun d -> d.Path))

let generateTokenToFileMap (documents: Document []) =
    documents
    |> Array.collect
        (fun document ->
            document.Tokens
            |> Array.map (fun token -> (token, document)))
    |> Array.groupBy fst
    |> Array.map (distinctSnd >> documentPath)
    |> Map.ofArray

let ngrams n (Token t) =
    t
    |> Seq.windowed (n + 1)
    |> Seq.map (fun chars -> chars |> Array.take n |> stringContact |> Ngram)
    |> Seq.toArray

let generateNgramsFromDocuments (documents: Document []) =
    let n = 3
    let getNgrams = ngrams n

    documents
    |> Array.collect (fun document -> document.Tokens)
    |> Array.filter (fun (Token token) -> token.Length >= n)
    |> Array.collect
        (fun token ->
            token
            |> getNgrams
            |> Array.map (fun ngram -> (ngram, token)))
    |> Array.groupBy fst
    |> Array.map distinctSnd
    |> Map.ofArray

let saveTokens indexFolder tokens =
    let lines =
        tokens
        |> Map.map
            (fun (Token token) paths ->
                paths
                |> Array.map (fun (Path path) -> sprintf "%s~%s" token path))
        |> Map.toSeq
        |> Seq.collect snd

    do File.WriteAllLines(indexFolder + "/tokens.txt", lines)

let saveNgrams indexFolder (ngrams: Map<Ngram, Token []>) =
    let lines =
        ngrams
        |> Map.map
            (fun (Ngram ngram) tokens ->
                tokens
                |> Array.map (fun (Token t) -> t)
                |> arrayJoinComma
                |> sprintf "%s:%s" ngram)

        |> Map.toSeq
        |> Seq.map snd

    do File.WriteAllLines(indexFolder + "/ngrams.txt", lines)

let loadTokens indexFolder =
    let fileName = indexFolder + "/tokens.txt"

    if File.Exists(fileName) then
        File.ReadAllLines(fileName)
        |> Seq.groupBy (parseTilde >> Array.head >> Token)
        |> Seq.map
            (fun (token, lines) ->
                token,
                lines
                |> Seq.map (parseTilde >> arraySecond >> Path)
                |> Seq.toArray)
        |> Map.ofSeq
        |> Some
    else
        None

let loadNgrams indexFolder =
    let fileName = indexFolder + "/ngrams.txt"

    if File.Exists(fileName) then
        File.ReadAllLines(fileName)
        |> Seq.map
            (fun line ->
                let colonParsed = line |> parseColon

                colonParsed |> Array.head |> Ngram,
                colonParsed
                |> arraySecond
                |> parseComma
                |> Array.map Token)
        |> Map.ofSeq
        |> Some
    else
        None

let getOrCreateIndex indexFolder =
    let ngrams, tokens =
        match loadNgrams indexFolder, loadTokens indexFolder with
        | Some n, Some t -> n, t
        | _ ->
            let tokenisedDocuments = tokeniseDocuments indexFolder
            let ngrams = generateNgramsFromDocuments tokenisedDocuments 
            saveNgrams indexFolder ngrams
            let tokens = generateTokenToFileMap tokenisedDocuments
            saveTokens indexFolder tokens
            ngrams, tokens
            
    { Ngrams = ngrams; Tokens = tokens }
