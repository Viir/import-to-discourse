module Discourse.Config

open Discourse.DbModel.Common

let recordValueSeparator = "\t"
let columnNameSeparator = ","
let columnValueNull = "\\N"

let idColumnName = "id"

let findSequenceNameRegexGroupName = "sequencename"

let findSequenceIdNameRegexPatternFromTableName tableName =
    Common.regexPatternWithWhitespaceGeneralized
        ("ALTER TABLE[\w\s]*\s+" + tableName + " ALTER COLUMN " + idColumnName + " SET DEFAULT nextval\s*\(\s*'(?<" + findSequenceNameRegexGroupName + ">[\w]*)'")

let statementSetSequenceParamRegexPatternFromSequenceName sequenceName =
    Common.regexPatternWithWhitespaceGeneralized
        ("(?<=^\s*SELECT\s+pg_catalog.setval\s*\(\s*'" + sequenceName + "',\s*)[\d]+\s*(?=,)")

let postActionLikeNameKey = "like"

let tableNameFromRecordType recordType =
    match recordType with
    | User -> "users"
    | UserProfile -> "user_profiles"
    | UserOptions -> "user_options"
    | UserStats -> "user_stats"
    | Category -> "categories"
    | Topic -> "topics"
    | Post -> "posts"
    | Tag -> "tags"
    | TopicTag -> "topic_tags"
    | Permalink -> "permalinks"
    | PostActionType -> "post_action_types"
    | PostAction -> "post_actions"

