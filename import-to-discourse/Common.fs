module Common

let listFindTupleSndWhereFstEquals fstValue list =
    snd (list |> List.find (fun tuple -> (fst tuple) = fstValue))

let regexPatternWithWhitespaceGeneralized originalPattern =
    System.Text.RegularExpressions.Regex.Replace(originalPattern, "\s+", "\s+")
