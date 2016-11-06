module AddTopic

open System.Text.RegularExpressions

let recordValueSeparator = "\t"
let columnNameSeparator = ","

let topicsTableName = "topics"

type CopySection = { tableName: string; startLineIndex : int; listLine : List<string>; listColumnName : List<string> }

let indexOfElementMatchingRegexPattern regexPattern list =
    list |> List.findIndex (fun elem -> Regex.IsMatch(elem, regexPattern, RegexOptions.IgnoreCase))

let indexOfElementStartingWithRegexPattern regexPattern list =
    indexOfElementMatchingRegexPattern ("^" + regexPattern) list

let copySectionFromTableName tableName listLine =
    let startLinePattern = "COPY\s+" + tableName + "\s*\(([^\)]*)\)"
    let startLineIndex = listLine |> indexOfElementStartingWithRegexPattern startLinePattern
    let copySectionListLine =
        listLine
        |> List.skip startLineIndex
        |> List.takeWhile (fun line -> not (Regex.IsMatch(line, "^\\\.")))

    let listColumnNameText = Regex.Match(copySectionListLine.Head, startLinePattern).Groups.[1].Value
    let listColumnName =
        listColumnNameText.Split(List.toArray [columnNameSeparator], System.StringSplitOptions.RemoveEmptyEntries)
        |> Array.toList
        |> List.map (fun columnName -> columnName.Trim())

    { tableName = tableName; startLineIndex = startLineIndex; listLine = copySectionListLine; listColumnName = List.empty }

let addTopic postgresqlDump =
    let listLine = postgresqlDump |> Array.toList

    let topicsCopySection = listLine |> copySectionFromTableName topicsTableName

    listLine

