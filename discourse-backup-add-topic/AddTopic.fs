﻿module AddTopic

open System
open System.Linq
open System.Text.RegularExpressions

let recordValueSeparator = "\t"
let columnNameSeparator = ","

let idColumnName = "id"

let topicsTableName = "topics"

let copySectionEndLine = "\\."

let copySectionEndLinePattern = "^" + Regex.Escape(copySectionEndLine)

type Topic =
    {
        userId: int;
        title: string;
        createdAt: DateTime;
        lastPostedAt: DateTime;
        updatedAt: DateTime;
        viewCount: int;
        postCount: int;
        lastPostUserId: int;
        replyCount: int;
        highestPostNumber: int;
        categoryId: int;
        isClosed: bool;
        slug: string;
    }

type columnValue =
    | Null
    | Boolean of bool
    | Integer of int
    | String of string
    | Time of DateTime
    | Default

let setColumnValueStatic =
    [
        ("featured_user1_id", Null);
        ("featured_user2_id", Null);
        ("featured_user3_id", Null);
        ("avg_time", Null);
        ("deleted_at", Null);
        ("image_url", Null);
        ("off_topic_count", Default);
        ("like_count", Default);
        ("incoming_link_count", Default);
        ("bookmark_count", Default);
        ("archived", Boolean false);
        ("bumped_at", Time DateTime.MinValue);
        ("has_summary", Boolean false);
        ("vote_count", Default);
        ("archetype", Default);
        ("featured_user4_id", Null);
        ("notify_moderators_count", Default);
        ("spam_count", Default);
        ("illegal_count", Default);
        ("inappropriate_count", Default);
        ("pinned_at", Null);
        ("score", Null);
        ("percent_rank", Default);
        ("notify_user_count", Default);
        ("subtype", Null);
        ("auto_close_at", Null);
        ("auto_close_user_id", Null);
        ("auto_close_started_at", Null);
        ("deleted_by_id", Null);
        ("participant_count", Default);
        ("word_count", Null);
        ("excerpt", Null);
        ("pinned_globally", Default);
        ("auto_close_based_on_last_post", Default);
        ("auto_close_hours", Null);
        ("pinned_until", Null);
        ("fancy_title", Null);

        ("", Default);
    ]

let topicToBeAdded =
    {
        userId = -1;
        title = "topic which has been added by import tool";
        createdAt = DateTime.UtcNow;
        lastPostedAt = DateTime.UtcNow;
        updatedAt = DateTime.MinValue;
        viewCount = 4;
        postCount = 0;
        lastPostUserId = -1;
        replyCount = 0;
        highestPostNumber = 0;
        categoryId = 0;
        isClosed = false;
        slug = "this-is-the-imported-topics-slug";
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
    columnValue.Replace("\t", "\\t")

let timeString (dateTime:DateTime) =
    //  example taken from discourse backup dump.sql: 2016-09-17 20:31:35.679472
    dateTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff")

let columnValueStringFromUnion columnValueUnion =
    match columnValueUnion with
        | Null -> "\\N"
        | Boolean b -> if b then "t" else "f"
        | Integer i -> i.ToString()
        | String s -> escapedForColumnValue s
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

let copySectionWithRecordsIdIncremented copySectionOriginal funColumnValueFromName listRecordId =
    let id =
        copySectionOriginal.listRecord
        |> List.map (fun record -> record |> List.find (fun (columnName, _) -> columnName = idColumnName))
        |> List.map (fun (_, idString) -> System.Int32.Parse(idString))
        |> List.max

    copySectionWithRecords
        copySectionOriginal
        (fun recordId columnName ->
            if columnName = idColumnName
            then Integer (id + 1)
            else (funColumnValueFromName recordId columnName))
        listRecordId

let columnValueForTopic topic columnName =
    let valueStatic =
        setColumnValueStatic
        |> List.tryFind (fun columnNameAndValue -> (fst columnNameAndValue) = columnName)

    match columnName with
        | "user_id" -> Integer topic.userId
        | "title" -> String topic.title
        | "created_at" -> Time topic.createdAt
        | "last_posted_at" -> Time topic.lastPostedAt
        | "updated_at" -> Time topic.updatedAt
        | "views" -> Integer topic.viewCount
        | "posts_count" -> Integer topic.postCount
        | "last_post_user_id" -> Integer topic.lastPostUserId
        | "reply_count" -> Integer topic.replyCount
        | "highest_post_number" -> Integer topic.highestPostNumber
        | "category_id" -> Integer topic.categoryId
        | "closed" -> Boolean topic.isClosed
        | "slug" -> String topic.slug
        | _ when valueStatic.IsSome -> snd valueStatic.Value
        | _ -> Default

let copySectionWithTopics (listTopic: List<Topic>) copySectionOriginal =
    copySectionWithRecordsIdIncremented
        copySectionOriginal
        (fun topic -> columnValueForTopic topic)
        listTopic

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
        [ [ ""; sectionStartLine ]; sectionToBeAdded.listLine; [ copySectionEndLine ]]
        |> List.concat

    [ beforeListLine; sectionToBeAddedListLine; afterListLine ]
    |> List.concat

let addTopic postgresqlDump =
    let listLine = postgresqlDump |> Array.toList

    let topicsCopySection = listLine |> copySectionFromTableName topicsTableName

    let topicsCopySectionAdd = topicsCopySection |> copySectionWithTopics [ topicToBeAdded ]

    let listLineWithTopicAdded = listLine |> withCopySectionAppended topicsCopySectionAdd

    listLineWithTopicAdded |> List.toArray

