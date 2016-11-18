module Main

open System.IO

let onArgumentsChecked
    toBeImportedInputFilePath
    discourseInputFilePath
    discourseOutputFilePath
    =
    printfn "reading data to be imported from file: %A" toBeImportedInputFilePath
    printfn "reading discourse dump from file: %A" discourseInputFilePath

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

    printfn "press key to continue"
    System.Console.ReadKey() |> ignore

    let sqlDumpListLine = File.ReadAllLines(discourseInputFilePath)

    let modifiedDump =
        AddRecord.postgresqlDumpWithRecordsAdded
            sqlDumpListLine
            setUser
            setCategory
            setTopic
            setPost

    printfn "writing to file: %A" discourseOutputFilePath
    File.WriteAllLines(discourseOutputFilePath, modifiedDump, System.Text.Encoding.UTF8)
    0

[<EntryPoint>]
let main argv =
    printfn "warning: diagnostics not implemented. For example, importing a user with an email address that was already present in the dump was observed to result in a reset of discourse."
    System.Console.ReadKey() |> ignore

    if argv.Length < 3
    then
        printfn "missing arguments"
        System.Console.ReadKey() |> ignore
        2
    else
        let toBeImportedInputFilePath = argv.[0]
        let discourseInputFilePath = argv.[1]
        let discourseOutputFilePath = argv.[2]

        onArgumentsChecked toBeImportedInputFilePath discourseInputFilePath discourseOutputFilePath

