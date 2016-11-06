module Main

open System.IO

[<EntryPoint>]
let main argv =
    let inputFilePath = argv.[0]
    let outputFilePath = argv.[1]
    
    printfn "reading from file: %A" inputFilePath
    
    let sqlDumpListLine = File.ReadAllLines(inputFilePath)

    let modifiedDump = AddTopic.addTopic sqlDumpListLine

    printfn "writing to file: %A" outputFilePath
    
    File.WriteAllLines(outputFilePath, modifiedDump, System.Text.Encoding.UTF8)

    0
