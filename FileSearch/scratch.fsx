
//this will probably differ on mac, or something other than windows.
#r @"bin\Debug\net5.0\FileSearch.dll"
open Common

let testFileLocation = System.IO.Path.Combine(__SOURCE_DIRECTORY__, "..", @"FileSearch.Tests\testfile.txt")


let documents = Index.getDocuments [| testFileLocation |]
//74 tokens expands to 185 ngrams
//get the ngrams
let ngrams = Index.generateNgramsFromDocuments documents
ngrams |> Map.count

ngrams |> Map.iter (fun k v -> printfn $"%A{k}:%A{v}")

let tokens = Index.generateTokenToFileMap documents

let tokens2 =
    [|
        "upAndDown"
        "downAndOut"
        "upAndAcross"
    |] |> Array.map Common.Token.create
    
let document = [|{Common.Path = Common.Path.create "testPath"; Common.Tokens = tokens2}|]
let ngrams2 = Index.generateNgramsFromDocuments document
ngrams |> Map.iter (fun k v -> printfn "%A:%A" k v)

let mockedIndex =
    let tokensFile1 =
        [|
            "upAndDown"
            "downAndOut"
            "upAndAcross"
        |] |> Array.map Common.Token.create
    let tokensFile2 =
        [|
            "upAndDown"
            "overAndOut"
        |] |> Array.map Common.Token.create        
    let mockedDocument =
        [|
            {Common.Path = Common.Path.create "testPath1"; Common.Tokens = tokensFile1}
            {Common.Path = Common.Path.create "testPath2"; Common.Tokens = tokensFile2}
        |]
    let mockedNgrams = Index.generateNgramsFromDocuments mockedDocument
    let mockedTokensMap = Index.generateTokenToFileMap mockedDocument
    {Ngrams = mockedNgrams; Tokens = mockedTokensMap}


//runs the whole query and combines searches for multi tokens
//the query contains the index to be searched.
let mockedQuery = {QueryText = "Down"; Index = mockedIndex}
let queryResult = Query.runQuery mockedQuery

//executes the query but doesn't combine
let queryTokens = Query.executeQuery mockedQuery

//search the index for an individual word
let search = Query.searchIndexForTerm mockedQuery.Index "down"
let search2 = Query.searchIndexForTerm mockedQuery.Index "Across"

//a two part query with a token to combine the results

let mockedQuery3 = {QueryText = "Down & Across"; Index = mockedIndex}
let executedQuery3 = Query.executeQuery mockedQuery3
let queryRan = Query.runQuery mockedQuery3

let search4 = Query.searchIndexForTerm mockedIndex "down"
let searchTerm = "down"
let searchNgram = searchTerm.Substring(0, 3) |> Ngram
let tokensMatchingNgram = mockedIndex.Ngrams.TryFind(searchNgram)
tokensMatchingNgram.Value |> Array.filter (Token.contains searchTerm)
