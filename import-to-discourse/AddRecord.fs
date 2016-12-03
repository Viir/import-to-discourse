﻿module AddRecord

open System

open Discourse.Config
open Discourse.DbModel.Common
open Discourse.DbModel.User
open Discourse.DbModel.Category
open Discourse.DbModel.Topic
open Discourse.DbModel.Post
open Discourse.DumpSql.Parse


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
    postgresqlDumpListLine
    setUserToBeAdded
    setCategoryToBeAdded
    setTopicToBeAdded
    setPostToBeAdded
    =
    let listTransform =
        [
            (User, (copySectionFromListRecord columnValueForUserWithDefaults setUserToBeAdded));
            (UserOptions, (copySectionFromListRecord columnValueForUserOptionsWithDefaults setUserToBeAdded));
            (UserProfile, (copySectionFromListRecord columnValueForUserProfile setUserToBeAdded));
            (UserStats, (copySectionFromListRecord columnValueForUserStatsWithDefaults setUserToBeAdded));

            (Category, (copySectionFromListRecord columnValueForCategory setCategoryToBeAdded));

            (Topic, (copySectionFromListRecord columnValueForTopicWithDefaults setTopicToBeAdded));

            (Post, (copySectionFromListRecord columnValueForPost setPostToBeAdded));
        ]

    let listLineWithRecordsAppended =
        listTransform
        |> List.fold (fun state (recordType, transform) ->
            let copySection = state |> copySectionFromRecordType recordType
            let copySectionAdd = copySection |> transform
            state |> withCopySectionAppended copySectionAdd)
            postgresqlDumpListLine

    listLineWithRecordsAppended |> List.toArray

