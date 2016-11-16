module Main

open System.IO

[<EntryPoint>]
let main argv =
    let inputFilePath = argv.[0]
    let outputFilePath = argv.[1]
   
    printfn "reading from file: %A" inputFilePath

    printfn "press key to continue"
    System.Console.ReadKey() |> ignore
    
    let sqlDumpListLine = File.ReadAllLines(inputFilePath)

    let modifiedDump = AddRecord.addRecord sqlDumpListLine

    printfn "writing to file: %A" outputFilePath
    
    File.WriteAllLines(outputFilePath, modifiedDump, System.Text.Encoding.UTF8)

    0
