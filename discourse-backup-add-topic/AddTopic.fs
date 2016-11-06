module AddTopic

open System
open System.Linq
open System.Text.RegularExpressions

let recordValueSeparator = "\t"
let columnNameSeparator = ","

let idColumnName = "id"

let topicsTableName = "topics"

type Topic =
    {
        userId: int;
        title: string;
        createdAt: DateTime;
        lastPostedAt: DateTime;
    }

let topicToBeAdded =
    {
        userId = -1;
        title = "topic which has been added by import tool";
        createdAt = DateTime.UtcNow;
        lastPostedAt = DateTime.UtcNow;
    }

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

let withRecordAdded copySectionOriginal funColumnValueFromName =
    let recordListColumnNameAndValue =
        copySectionOriginal.listColumnName
        |> List.map (fun columnName -> (columnName, funColumnValueFromName columnName))
    let recordLine =
        String.Join(recordValueSeparator,
            recordListColumnNameAndValue
            |> List.map snd)
    {
        copySectionOriginal
        with
            listRecord = List.append copySectionOriginal.listRecord [ recordListColumnNameAndValue ];
            listLine = List.append copySectionOriginal.listLine [ recordLine ]
    }

let withRecordAddedIdIncrement copySectionOriginal funColumnValueFromName =
    let id =
        copySectionOriginal.listRecord
        |> List.map (fun record -> record |> List.find (fun (columnName, _) -> columnName = idColumnName))
        |> List.map (fun (_, idString) -> System.Int32.Parse(idString))
        |> List.max

    withRecordAdded
        copySectionOriginal
        (fun columnName -> if columnName = idColumnName then (id + 1).ToString() else (funColumnValueFromName columnName))

let timeString (dateTime:DateTime) =
    //  example taken from discourse backup dump.sql: 2016-09-17 20:31:35.679472
    dateTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff")

let columnValueForTopic topic columnName =
    match columnName with
        | "user_id" -> topic.userId.ToString()
        | "title" -> topic.title
        | "created_at" -> timeString topic.createdAt
        | "last_posted_at" -> timeString topic.lastPostedAt
        | _ -> null

let withRecordTopicAdded topic copySectionOriginal =
    withRecordAddedIdIncrement
        copySectionOriginal (columnValueForTopic topic)

let withCopySectionReplaced sectionOriginalTableName sectionReplacement listLine =
    let copySectionOriginal = listLine |> copySectionFromTableName sectionOriginalTableName
    let (beforeListLine, afterBefore) = listLine |> List.splitAt copySectionOriginal.startLineIndex
    let (_, afterListLine) = afterBefore |> List.splitAt copySectionOriginal.listLine.Length
    [ beforeListLine; sectionReplacement.listLine; afterListLine ]
    |> List.concat

let addTopic postgresqlDump =
    let listLine = postgresqlDump |> Array.toList

    let topicsCopySection = listLine |> copySectionFromTableName topicsTableName

    let topicsCopySectionUpdated = topicsCopySection |> withRecordTopicAdded topicToBeAdded

    let listLineWithTopicAdded = listLine |> withCopySectionReplaced topicsTableName topicsCopySectionUpdated

    listLineWithTopicAdded |> List.toArray

