module Common

let listFindTupleSndWhereFstEquals fstValue list =
    snd (list |> List.find (fun tuple -> (fst tuple) = fstValue))

