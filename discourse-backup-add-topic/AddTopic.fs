module AddTopic

open System.Text.RegularExpressions

let indexOfElementMatchingRegexPattern regexPattern list =
    list |> List.findIndex (fun elem -> Regex.IsMatch(elem, regexPattern, RegexOptions.IgnoreCase))

let indexOfElementStartingWithRegexPattern regexPattern list =
    indexOfElementMatchingRegexPattern ("^" + regexPattern) list

let addTopic postgresqlDump =
    let listLine = postgresqlDump |> Array.toList
    let copyTopicsLineIndex = listLine |> indexOfElementStartingWithRegexPattern "COPY topics \("
    let copyPostsLineIndex = listLine |> indexOfElementStartingWithRegexPattern "COPY posts \("
    postgresqlDump

