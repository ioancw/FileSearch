module Tests

open System
open Xunit

let testFile = @"testfile.txt"
let fileToTest = Index.getFiles "txt" testFile

[<Fact>]
let ``My test`` () =
    Assert.True(true)
    
[<Fact>]
let ``Get tokens from file`` () = 
    let tokens = Index.getTokensFromFile testFile
    Assert.Equal(Array.length tokens, 176)
    Assert.True(Array.contains (Common.Token "groupBy") tokens)
    Assert.True(Array.contains (Common.Token "iter") tokens)