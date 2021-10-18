module Common

open System

module Token = 
    type T = Token of string
    //wrap
    let create (s: string) = Token s
    //unwrap
    let value (Token s) = s
    
    let existsIn (input: string) token =
        token |> value |> input.Contains
    let contains (input:string) token =
        let asString = token |> value
        input |> asString.Contains
        
module Path =
    type T = Path of string
    let create (s: string) = Path s
    let value (Path s) = s
    
type Ngram = Ngram of string

type Document = { Path: Path.T; Tokens: Token.T [] }

// An index contains the Ngram map and a Token map.
// Ngram
type Index =
    { Ngrams: Map<Ngram, Token.T []>
      Tokens: Map<Token.T, Path.T []> }

//A query contains the string to find within the Index
type Query = { QueryText: string; Index: Index }

let stringParse delimiter (s: string) = s.Split([| delimiter |])

let stringContact (chars: char []) = String.Concat(chars)

let parseTilde = stringParse '~'

let parseColon = stringParse ':'

let parseComma = stringParse ','

let stringJoin (delimiter: char) (items: string []) = String.Join(delimiter, items)

let stringJoinS (deliminator: string) (strings: string []) = String.Join(deliminator, strings)

let arrayJoinComma = stringJoin ','

let arraySecond (a: 'a []) = a.[1]

module Colours =
    let magenta = ConsoleColor.Magenta
    let green = ConsoleColor.Green
    let red = ConsoleColor.Red
    let yellow = ConsoleColor.Yellow
    let gray = ConsoleColor.Gray
    let darkgreen = ConsoleColor.DarkGreen
    let darkyellow = ConsoleColor.DarkYellow

let apply colour = Console.ForegroundColor <- colour

let printc colour text =
    let current = Console.ForegroundColor
    apply colour
    printf "%s" text
    apply current
