module AddRecord

open System
open System.Linq
open System.Text.RegularExpressions

open Discourse.DbModel.Common
open Discourse.DbModel.User
open Discourse.DbModel.Category
open Discourse.DbModel.Topic
open Discourse.DbModel.Post

let recordValueSeparator = "\t"
let columnNameSeparator = ","

let idColumnName = "id"

let userTableName = "users"
let userOptionsTableName = "user_options"
let userProfilesTableName = "user_profiles"
let userStatsTableName = "user_stats"
let categoryTableName = "categories"
let topicTableName = "topics"
let postTableName = "posts"

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

let indexOfFirstElementMatchingRegexPattern regexPattern list =
    list |> List.findIndex (fun elem -> Regex.IsMatch(elem, regexPattern, RegexOptions.IgnoreCase))

let indexOfLastElementMatchingRegexPattern regexPattern list =
    list |> List.findIndexBack (fun elem -> Regex.IsMatch(elem, regexPattern, RegexOptions.IgnoreCase))

let copySectionStartLinePattern tableName =
    "^COPY\s+" + tableName + "\s*\(([^\)]*)\)"

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

let record listColumnName funColumnValueFromName recordId =
    let recordListColumnNameAndValue =
        listColumnName
        |> List.map (fun columnName -> (columnName, funColumnValueFromName columnName))

    let recordLine =
        String.Join(recordValueSeparator,
            recordListColumnNameAndValue
            |> List.map snd)
    (recordListColumnNameAndValue, recordLine)

let escapedForColumnValue (columnValue:string) =
    columnValue.Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "\\r")

let timeString (dateTime:DateTime) =
    //  example taken from discourse backup dump.sql: 2016-09-17 20:31:35.679472
    dateTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff")

let columnValueStringFromUnion columnValueUnion =
    match columnValueUnion with
        | Null -> "\\N"
        | Boolean b -> if b then "t" else "f"
        | Integer i -> i.ToString()
        | String s ->
            match s with
            | null -> "\\N"
            | _ -> escapedForColumnValue s
        | Time t -> timeString t
        | Default -> "DEFAULT"

let copySectionWithRecords copySectionOriginal funColumnValueFromName listRecordId =
    let listRecordListColumnNameAndValue =
        listRecordId
        |> List.map (fun recordId ->
            copySectionOriginal.listColumnName
            |> List.choose (fun columnName ->
                let columnValue = funColumnValueFromName recordId columnName
                if columnValue = Default
                then None
                else Some (columnName, columnValueStringFromUnion columnValue)))

    let listRecordLine =
        listRecordListColumnNameAndValue
        |> List.map (fun record -> String.Join(recordValueSeparator, record |> List.map snd))
    {
        copySectionOriginal
        with
            listRecord = listRecordListColumnNameAndValue;
            listLine = listRecordLine
    }

let copySectionFromListRecord columnValueFromRecord listRecord copySectionOriginal =
    copySectionWithRecords
        copySectionOriginal
        (fun record -> columnValueFromRecord record)
        listRecord

let withCopySectionAppended sectionToBeAdded listLine =
    let tableName = sectionToBeAdded.tableName
    let copySectionPattern = copySectionStartLinePattern tableName
    let lastSectionStartLineIndex = listLine |> indexOfLastElementMatchingRegexPattern copySectionPattern
    let lastSectionLineCount = (listLine |> List.skip lastSectionStartLineIndex |> indexOfFirstElementMatchingRegexPattern copySectionEndLinePattern) + 1
    let (beforeListLine, afterListLine) = listLine |> List.splitAt (lastSectionStartLineIndex + lastSectionLineCount)

    let listColumnUsedName =
        sectionToBeAdded.listRecord
        |> List.collect (fun record -> record |> List.map fst)
        |> List.distinct

    //  !not implemented: check order of columns!

    let sectionStartLine = "COPY " + tableName + " (" + String.Join(columnNameSeparator, listColumnUsedName) + ") FROM stdin;"

    let sectionToBeAddedListLine =
        if 0 < (listColumnUsedName |> List.length)
        then [ [ ""; sectionStartLine ]; sectionToBeAdded.listLine; [ copySectionEndLine ]] |> List.concat
        else []

    [ beforeListLine; sectionToBeAddedListLine; afterListLine ]
    |> List.concat

let postgresqlDumpWithRecordsAdded
    postgresqlDump
    setUserToBeAdded
    setCategoryToBeAdded
    setTopicToBeAdded
    setPostToBeAdded
    =
    let listLine = postgresqlDump |> Array.toList

    let listTransform =
        [
            (userTableName, (copySectionFromListRecord columnValueForUserWithDefaults setUserToBeAdded));
            (userOptionsTableName, (copySectionFromListRecord columnValueForUserOptionsWithDefaults setUserToBeAdded));
            (userProfilesTableName, (copySectionFromListRecord columnValueForUserProfile setUserToBeAdded));
            (userStatsTableName, (copySectionFromListRecord columnValueForUserStatsWithDefaults setUserToBeAdded));

            (categoryTableName, (copySectionFromListRecord columnValueForCategory setCategoryToBeAdded));

            (topicTableName, (copySectionFromListRecord columnValueForTopicWithDefaults setTopicToBeAdded));

            (postTableName, (copySectionFromListRecord columnValueForPost setPostToBeAdded));
        ]

    let listLineWithRecordsAppended =
        List.fold (fun state (tableName, transform) ->
            let copySection = state |> copySectionFromTableName tableName
            let copySectionAdd = copySection |> transform
            state |> withCopySectionAppended copySectionAdd)
            listLine
            listTransform

    listLineWithRecordsAppended |> List.toArray

