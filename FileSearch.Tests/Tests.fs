module Tests

open System
open Xunit
open Common
let testFile = @"testfile.txt"
let fileToTest = Index.getFiles "txt" testFile

[<Fact>]
let ``My test`` () =
    Assert.True(true)
    
[<Fact>]
let ``Get tokens from file`` () = 
    let tokens = Index.getTokensFromFile testFile
    Assert.Equal(Array.length tokens, 190) //includes stop words
    Assert.True(Array.contains (Common.Token "groupBy") tokens)
    Assert.True(Array.contains (Common.Token "iter") tokens)
    Assert.True(Array.contains (Common.Token "let") tokens)
    
[<Fact>]
let ``Remove stop words from file`` () =
    let tokensLessStopWords =
        Index.getTokensFromFile testFile
        |> Index.filterStopWords
        
    let tokenFrequencies = Index.getTokenFrequencies tokensLessStopWords
    let sortByDescending =
        Array.find (fun (t, c) -> t = Common.Token "sortByDescending") tokenFrequencies
        
    Assert.Equal(Array.length tokensLessStopWords, 176)
    Assert.True(Array.contains (Common.Token "groupBy") tokensLessStopWords)
    //check stop word is filtered.
    Assert.True(not <| Array.contains (Common.Token "let") tokensLessStopWords)
    Assert.Equal(sortByDescending |> snd, 2)
    
[<Fact>]
let ``Duplicate tokens removed`` () =
    let distinctTokensWithStopWordsRemoved =
        Index.getDistinctTokensStopWords testFile

    let sorByDescendingFreq =
        Index.getFrequencyOfToken distinctTokensWithStopWordsRemoved "sortByDescending"
    Assert.Equal(Array.length distinctTokensWithStopWordsRemoved, 74)
    Assert.Equal(sorByDescendingFreq, 1)
    
[<Fact>]
let ``Generate correct ngrams from tokens`` () =
    let tokens =
        [|
            "upAndDown"
            "upAndAcross"
        |] |> Array.map Common.Token
        
    let document = [|{Common.Path = Common.Path "testPath"; Common.Tokens = tokens}|]
    let ngrams = Index.generateNgramsFromDocuments document
    let expectedNgrams =
        [|
            Common.Ngram "Acr", [|Common.Token "upAndAcross"|]
            Common.Ngram "And", [|Common.Token "upAndAcross"; Common.Token "upAndDown"|]
            Common.Ngram "Dow", [|Common.Token "upAndDown"|]
            Common.Ngram "cro", [|Common.Token "upAndAcross"|]
            Common.Ngram "dAc", [|Common.Token "upAndAcross"|]
            Common.Ngram "dDo", [|Common.Token "upAndDown"|]
            Common.Ngram "ndA", [|Common.Token "upAndAcross"|]
            Common.Ngram "ndD", [|Common.Token "upAndDown"|]
            Common.Ngram "pAn", [|Common.Token "upAndAcross"; Common.Token "upAndDown"|]
            Common.Ngram "ros", [|Common.Token "upAndAcross"|]
            Common.Ngram "upA", [|Common.Token "upAndAcross"; Common.Token "upAndDown"|]
        |]
        |> Map.ofArray
    Assert.Equal<Map<Common.Ngram, Common.Token[]>>(expectedNgrams, ngrams)

let mockedIndex =
    let tokensFile1 =
        [|
            "upAndDown"
            "downAndOut"
            "upAndAcross"
        |] |> Array.map Common.Token
    let tokensFile2 =
        [|
            "upAndDown"
            "overAndOut"
            //"upAndAcross"
        |] |> Array.map Common.Token        
    let mockedDocument =
        [|
            {Common.Path = Common.Path "testPath1"; Common.Tokens = tokensFile1}
            {Common.Path = Common.Path "testPath2"; Common.Tokens = tokensFile2}
        |]
    let mockedNgrams = Index.generateNgramsFromDocuments mockedDocument
    let mockedTokensMap = Index.generateTokenToFileMap mockedDocument
    {Ngrams = mockedNgrams; Tokens = mockedTokensMap}
    
[<Fact>]
let ``Search word in mocked index`` =
    let searchResult = Query.searchIndexForTerm mockedIndex "down"
    let expectedTokens = [| Token "downAndOut" |]
    let expectedPaths = Set.empty.Add(Path "testPath")
    let expectedQueryResults = (expectedTokens, expectedPaths)
    Assert.Equal(expectedQueryResults, searchResult)
    
    