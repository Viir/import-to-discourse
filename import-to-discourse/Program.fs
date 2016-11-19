module Main

open System.IO

let promptConsolePressKey () =
    printfn "Press key to continue."
    System.Console.ReadKey()

let onArgumentsChecked
    toBeImportedInputFilePath
    discourseInputFilePath
    discourseOutputFilePath
    =
    printfn "Reading data to be imported from file: %A" toBeImportedInputFilePath
    printfn "Reading discourse database dump from file: %A" discourseInputFilePath

    let (listUser, listCategory, listTopic, listPost) =
        Import.mvcforum.importFromFileAtPath toBeImportedInputFilePath

    let (setUser, setCategory, setTopic, setPost)   =
        Import.mvcforum.transformToDiscourse
            listUser
            listCategory
            listTopic
            listPost
            1000
            1000
            1000
            1000

    promptConsolePressKey() |> ignore

    let sqlDumpListLine = File.ReadAllLines(discourseInputFilePath)

    let modifiedDump =
        AddRecord.postgresqlDumpWithRecordsAdded
            sqlDumpListLine
            setUser
            setCategory
            setTopic
            setPost

    printfn "Writing to file: %A" discourseOutputFilePath
    File.WriteAllLines(discourseOutputFilePath, modifiedDump, System.Text.Encoding.UTF8)
    0

[<EntryPoint>]
let main argv =
    printfn "Warning: diagnostics not implemented. For example, importing a user with an email address that was already present in the database was observed to result in a reset of discourse."
    promptConsolePressKey() |> ignore

    if argv.Length < 3
    then
        printfn "Missing arguments."
        promptConsolePressKey() |> ignore
        2
    else
        let toBeImportedInputFilePath = argv.[0]
        let discourseInputFilePath = argv.[1]
        let discourseOutputFilePath = argv.[2]

        onArgumentsChecked toBeImportedInputFilePath discourseInputFilePath discourseOutputFilePath

