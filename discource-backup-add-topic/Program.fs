module Main

[<EntryPoint>]
let main argv =
    let inputFilePath = argv.[0]
    let outputFilePath = argv.[1]
    printfn "reading from file: %A" inputFilePath
    printfn "%A" argv
    0 // return an integer exit code
