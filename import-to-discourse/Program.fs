module Main

open System.IO

[<EntryPoint>]
let main argv =
    let toBeImportedInputFilePath = argv.[0]
    let discourseInputFilePath = argv.[1]
    let discourseOutputFilePath = argv.[2]
   
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
