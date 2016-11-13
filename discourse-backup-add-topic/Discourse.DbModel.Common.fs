module Discourse.DbModel.Common

type columnValue =
    | Null
    | Boolean of bool
    | Integer of int
    | String of string
    | Time of System.DateTime
    | Default


let columnValueWithDefaults (defaults : (string * columnValue) list) callbackNoDefault columnName =
    let valueFromDefault =
        defaults
        |> List.tryFind (fun columnNameAndValue -> (fst columnNameAndValue) = columnName)

    if valueFromDefault.IsSome
    then snd valueFromDefault.Value
    else callbackNoDefault columnName


let dateTimeOptionAsColumnValue (dateTime: System.DateTime Option) =
    if dateTime.IsSome then Time dateTime.Value else Null


let valueFromColumnName (record : (string * string) list) columnName =
    let column =
        record
        |> List.tryFind (fun (candidateColumnName, value) -> candidateColumnName = columnName)

    match column with
    | Some value -> snd value
    | _ -> null

