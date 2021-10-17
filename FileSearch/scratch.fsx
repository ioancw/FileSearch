
//this will probably differ on mac, or something other than windows.
#r @"bin\Debug\net5.0\FileSearch.dll"
let testFileLocation = System.IO.Path.Combine(__SOURCE_DIRECTORY__, "..", @"FileSearch.Tests\testfile.txt")


let documents = Index.getDocuments [| testFileLocation |]
//74 tokens expands to 185 ngrams
//get the ngrams
let ngrams = Index.generateNgramsFromDocuments documents
ngrams |> Map.count

ngrams |> Map.iter (fun k v -> printfn "%A:%A" k v)

let tokens = Index.generateTokenToFileMap documents

let tokens2 =
    [|
        "upAndDown"
        "downAndOut"
        "upAndAcross"
    |] |> Array.map Common.Token
    
let document = [|{Common.Path = Common.Path "testPath"; Common.Tokens = tokens2}|]
let ngrams2 = Index.generateNgramsFromDocuments document
ngrams |> Map.iter (fun k v -> printfn "%A:%A" k v)

open Common

let mockedIndex =
    let tokens =
        [|
            "upAndDown"
            "downAndOut"
            "upAndAcross"
        |] |> Array.map Common.Token
    let mockedDocument = [|{Common.Path = Common.Path "testPath"; Common.Tokens = tokens}|]
    let mockedNgrams = Index.generateNgramsFromDocuments mockedDocument
    let mockedTokensMap = Index.generateTokenToFileMap mockedDocument
    {Ngrams = mockedNgrams; Tokens = mockedTokensMap}


//runs the whole query and combines searches for multi tokens
//the query contains the index to be searched.
let mockedQuery = {QueryText = "Down"; Index = mockedIndex}
let queryResult = Query.runQuery mockedQuery

//executes the query but doesn't combine
let queryTokens = Query.executeQuery mockedQuery

//seach the index for an individual word
let search = Query.searchIndexForTerm mockedQuery.Index "down"
let search2 = Query.searchIndexForTerm mockedQuery.Index "Across"

//a two part query with a token to combine the results
let mockedQuery2 = {QueryText = "Down | Across"; Index = mockedIndex}
let executedQuery2 = Query.executeQuery mockedQuery2