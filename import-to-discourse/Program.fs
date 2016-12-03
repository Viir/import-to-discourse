module Main

open Discourse.DbModel.Common
open System.IO

let promptConsolePressKey () =
    printfn "Press key to continue."
    System.Console.ReadKey()

let onArgumentsChecked
    discourseInputFilePath
    toBeImportedInputFilePath
    discourseOutputFilePath
    =
    printfn "Reading discourse database dump from file: %A" discourseInputFilePath
    printfn "Reading data to be imported from file: %A" toBeImportedInputFilePath

    let newStopwatch() = System.Diagnostics.Stopwatch.StartNew()

    let stopwatchWithFile = newStopwatch()

    let sqlDumpListLine = File.ReadAllLines(discourseInputFilePath) |> Array.toList

    let stopwatchMerge = newStopwatch()

    let idBaseFromRecordType recordType =
        ((Discourse.DumpSql.Parse.copySectionFromRecordType recordType sqlDumpListLine)
        |> Discourse.DumpSql.Parse.idMaxOrZero) + 1 + (Discourse.Config.idOffsetFromRecordType recordType)

    let (listUser, listCategory, listTopic, listPost, listTag, listTopicTag) =
        Import.mvcforum.importFromFileAtPath toBeImportedInputFilePath

    let (setUser, setCategory, setTopic, setPost, setTag, setTopicTag)   =
        Import.mvcforum.transformToDiscourse
            (listUser, (idBaseFromRecordType User))
            (listCategory, (idBaseFromRecordType Category))
            (listTopic, (idBaseFromRecordType Topic))
            (listPost, (idBaseFromRecordType Post))
            (listTag, (idBaseFromRecordType Tag))
            (listTopicTag, (idBaseFromRecordType TopicTag))

    let modifiedDump =
        AddRecord.postgresqlDumpWithRecordsAdded
            sqlDumpListLine
            setUser
            setCategory
            setTopic
            setPost
            setTag
            setTopicTag

    stopwatchMerge.Stop()

    printfn "Imported %i user accounts, %i categories, %i topics and %i posts in %i ms."
        setUser.Length
        setCategory.Length
        setTopic.Length
        setPost.Length
        stopwatchMerge.ElapsedMilliseconds

    printfn "Starting to write sql script to file %s" discourseOutputFilePath

    File.WriteAllLines(discourseOutputFilePath, modifiedDump, System.Text.Encoding.UTF8)

    printfn "Completed in %i ms." stopwatchWithFile.ElapsedMilliseconds
    0

let listParameterDescription =
    [
        "Path to the file containing the sql dump from discourse."
        "Path to the file containing the data exported from mvcforum using the sql script from the file 'MvcForum.Export.To.Xml.sql'."
    ]

[<EntryPoint>]
let main argv =
    printfn "Tool from https://github.com/Viir/import-to-discourse to import from mvcforum to discourse."
    printfn "Warning: diagnostics are not implemented. For example, importing a user with an email address that was already present in the database was observed to result in a reset of discourse."
    promptConsolePressKey() |> ignore

    let expectedArgumentCount = listParameterDescription |> List.length
    let missingArgumentCount = expectedArgumentCount - argv.Length

    if 0 < missingArgumentCount
    then
        printfn "Missing %i arguments. The following %i arguments are expected:\n%s"
            missingArgumentCount
            expectedArgumentCount
            (System.String.Join(
                System.Environment.NewLine,
                (listParameterDescription |> List.map (fun pd -> "+ " + pd) |> List.toArray)))

        promptConsolePressKey() |> ignore
        2
    else
        let timeString = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHHmmss")

        let discourseInputFilePath = argv.[0]
        let toBeImportedInputFilePath = argv.[1]
        let discourseOutputFilePath = discourseInputFilePath + ".with.imported." + timeString + ".sql"

        onArgumentsChecked discourseInputFilePath toBeImportedInputFilePath discourseOutputFilePath

