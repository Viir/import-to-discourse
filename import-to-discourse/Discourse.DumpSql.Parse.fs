module Discourse.DumpSql.Parse

open Discourse.Config
open Discourse.DbModel.Common
open System.Text.RegularExpressions
open System.Linq


let copySectionEndLine = "\\."

let copySectionEndLinePattern = "^" + Regex.Escape(copySectionEndLine)

type CopySection =
    {
        tableName: string;
        startLineIndex : int;
        listLine : List<string>;
        listColumnName : List<string>;
        listRecord : List<List<string * string>>
    }

let listColumnValueFromRecordLine (recordLine : string) =
    recordLine.Split([recordValueSeparator] |> List.toArray, System.StringSplitOptions.None)
    |> Array.toList

let copySectionStartLinePattern tableName =
    "^COPY\s+" + tableName + "\s*\(([^\)]*)\)"

let indexOfFirstElementMatchingRegexPattern regexPattern list =
    list |> List.findIndex (fun elem -> Regex.IsMatch(elem, regexPattern, RegexOptions.IgnoreCase))

let indexOfLastElementMatchingRegexPattern regexPattern list =
    list |> List.findIndexBack (fun elem -> Regex.IsMatch(elem, regexPattern, RegexOptions.IgnoreCase))

let copySectionFromTableName tableName listLine =
    let startLinePattern = copySectionStartLinePattern tableName
    let startLineIndex = listLine |> indexOfFirstElementMatchingRegexPattern startLinePattern
    let copySectionListLine =
        listLine
        |> List.skip startLineIndex
        |> List.takeWhile (fun line -> not (Regex.IsMatch(line, copySectionEndLinePattern)))

    let listColumnNameText = Regex.Match(copySectionListLine.Head, startLinePattern).Groups.[1].Value
    let listColumnName =
        listColumnNameText.Split(List.toArray [columnNameSeparator], System.StringSplitOptions.RemoveEmptyEntries)
        |> Array.toList
        |> List.map (fun columnName -> columnName.Trim())

    let (_, listRecordLine) = copySectionListLine |> List.splitAt 1

    let listRecordListColumnValue =
        listRecordLine
        |> List.map listColumnValueFromRecordLine

    let listRecord =
        listRecordListColumnValue
        |> List.map (fun recordListColumnValue ->
            recordListColumnValue
            |> List.mapi (fun index columnValue ->
                (listColumnName.ElementAt(index), columnValue)))

    {
        tableName = tableName;
        startLineIndex = startLineIndex;
        listLine = copySectionListLine;
        listColumnName = listColumnName;
        listRecord = listRecord
    }

let copySectionFromRecordType recordType listLine =
    copySectionFromTableName (tableNameFromRecordType recordType) listLine

let idFromRecord record =
    record
    |> List.pick (fun (columnName, value) -> if columnName = idColumnName then Some value else None)

let idMaxOrZero copySection =
    let setId =
        copySection.listRecord
        |> List.map idFromRecord
        |> List.map System.Int32.Parse
    if 0 < (setId |> List.length) then setId |> List.max else 0
