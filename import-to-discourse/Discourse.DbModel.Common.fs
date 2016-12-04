module Discourse.DbModel.Common

type columnValue =
    | Null
    | Boolean of bool
    | Integer of int
    | String of string
    | Time of System.DateTime
    | Default

type RecordType =
    | User
    | UserProfile
    | UserOptions
    | UserStats
    | Category
    | Topic
    | Post
    | Tag
    | TopicTag
    | Permalink

let columnValueWithDefaults (defaults : (string * columnValue) list) callbackNoDefault columnName =
    let valueFromDefault =
        defaults
        |> List.tryFind (fun columnNameAndValue -> (fst columnNameAndValue) = columnName)

    if valueFromDefault.IsSome
    then snd valueFromDefault.Value
    else callbackNoDefault columnName


let intOptionAsColumnValue (intOption: int Option) =
    match intOption with
    | Some integer -> Integer integer
    | _ -> Null

let dateTimeOptionAsColumnValue (dateTimeOption: System.DateTime Option) =
    match dateTimeOption with
    | Some dateTime -> Time dateTime
    | _ -> Null

let valueFromColumnName (record : (string * string) list) columnName =
    let column =
        record
        |> List.tryFind (fun (candidateColumnName, value) -> candidateColumnName = columnName)

    match column with
    | Some value -> snd value
    | _ -> null

