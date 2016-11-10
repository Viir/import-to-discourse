module AddRecord

open System
open System.Linq
open System.Text.RegularExpressions

open Model
open ModelUser
open ModelTopic

let recordValueSeparator = "\t"
let columnNameSeparator = ","

let idColumnName = "id"

let userTableName = "users"
let userOptionsTableName = "user_options"
let userProfilesTableName = "user_profiles"
let userStatsTableName = "user_stats"
let topicTableName = "topics"

let copySectionEndLine = "\\."

let copySectionEndLinePattern = "^" + Regex.Escape(copySectionEndLine)

let userToBeAdded =
    {
        id = 1000;
        username = "username-added-by-import";
        createdAt = DateTime.UtcNow;
        updatedAt = DateTime.MinValue;
        name = "name-added-by-import";
        email = "user-added-by-import@viir.de";
        last_posted_at = None;
        last_seen_at = None;
        trust_level = 0;
        registration_ip_address = null;
        first_seen_at = None;

        profile_location = "profile location";
        profile_website = "http://distilledgames.de";
    }

let topicToBeAdded =
    {
        id = 1000;
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

let copySectionWithUsers (listUser: List<User>) copySectionOriginal =
    copySectionWithRecords
        copySectionOriginal
        (fun user -> columnValueForUserWithDefaults user)
        listUser

let copySectionWithUserOptions (listUser: List<User>) copySectionOriginal =
    copySectionWithRecords
        copySectionOriginal
        (fun user -> columnValueForUserOptionsWithDefaults user)
        listUser

let copySectionWithUserProfiles (listUser: List<User>) copySectionOriginal =
    copySectionWithRecords
        copySectionOriginal
        (fun user -> columnValueForUserProfile user)
        listUser

let copySectionWithUserStats (listUser: List<User>) copySectionOriginal =
    copySectionWithRecords
        copySectionOriginal
        (fun user -> columnValueForUserStatsWithDefaults user)
        listUser

let copySectionWithTopics (listTopic: List<Topic>) copySectionOriginal =
    copySectionWithRecords
        copySectionOriginal
        (fun topic -> columnValueForTopicWithDefaults topic)
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

let listTransform =
    [
        (userTableName, (copySectionWithUsers [ userToBeAdded ]));
        (userOptionsTableName, (copySectionWithUserOptions [ userToBeAdded ]));
        (userProfilesTableName, (copySectionWithUserProfiles [ userToBeAdded ]));
        (userStatsTableName, (copySectionWithUserStats [ userToBeAdded ]));
    ]

let addRecord postgresqlDump =
    let listLine = postgresqlDump |> Array.toList

    let listLineWithRecordsAppended =
        List.fold (fun state (tableName, t) ->
            let copySection = state |> copySectionFromTableName tableName
            let copySectionAdd = copySection |> t
            state |> withCopySectionAppended copySectionAdd)
            listLine
            listTransform

    listLineWithRecordsAppended |> List.toArray

