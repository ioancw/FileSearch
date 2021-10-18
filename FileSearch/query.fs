module Query

open Common
open Index
open Search

/// searched the supplied index for the supplied string.
/// the string is first broken down into an ngram, which is then used to
/// 'query' the Ngrams within the index.
/// we check whether the token exists within the original search term.
let searchIndexForTerm index (searchTerm: string) =
    let searchNgram = searchTerm.Substring(0, 3) |> Ngram
    let tokensMatchingNgram = index.Ngrams.TryFind(searchNgram)
    match tokensMatchingNgram with
    | Some tokens ->
        let matchedTokens =
            tokens
            |> Array.filter (Token.contains searchTerm)
        //this ensures that the word being searched for exists somewhere within the returned token.
        //i.e. searching for 'let' returns 'completely' and 'deedletest'
        let matchedDocs =
            matchedTokens
            |> Array.collect (fun token ->
                match index.Tokens.TryFind(token) with
                | Some docs -> docs
                | None -> Array.empty)
            |> Set.ofArray
        matchedTokens, matchedDocs
    | None -> Array.empty, Set.empty

type QueryOperator =
    | And
    | Or

type Query =
    | QueryResult of Token.T [] * Set<Path.T>
    | CombineOperator of QueryOperator

// executes the queries either side of the operator for later processing
let executeQuery query =
    let executeQuery = searchIndexForTerm query.Index

    query.QueryText
    |> stringParse ' '
    |> Array.map
        (fun t ->
            match t with
            | "&" -> CombineOperator And
            | "|" -> CombineOperator Or
            | _ -> executeQuery t |> QueryResult)
    |> List.ofArray

let eval l o r =
    match o with
    | And -> Set.intersect l r
    | Or -> Set.union l r

// combine the query results based on the query operator.
// currently the only two operators are AND and OR
let rec combineQueryResults qs =
    match qs with
    | [ _ ] -> qs
    | QueryResult (ql, l) :: CombineOperator o :: QueryResult (qr, r) :: t ->
        let evalResult = eval l o r
        QueryResult(Array.append ql qr, evalResult) :: t
        |> combineQueryResults
    | _ when (qs.Length % 2) = 0 -> failwith "Not a balanced list"
    | _ -> failwith (sprintf "Error: %A" qs)

let runQuery =
    executeQuery >> combineQueryResults >> List.head

let searchFilesForTokens queryResult =
    match queryResult with
    | QueryResult (queryTokens, matchingFiles) ->
        matchingFiles
        |> Seq.collect
            (searchFile
                queryTokens
                id
                (fun (lineNumber, line) -> queryTokens |> Array.exists (Token.existsIn line))
            )
        |> Seq.groupBy (fun result -> result.File)
        |> printResults
    | _ -> failwith "Query didn't execute correctly"
    
let searchFor queryFolder query =
    let query =
        { QueryText = query
          Index = getOrCreateIndex queryFolder }

    // run query to get tokens and files that match the search query
    // then find and highlight the token in the set of files
    query
    |> runQuery
    |> searchFilesForTokens



