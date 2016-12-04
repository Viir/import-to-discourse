module Discourse.Config

open Discourse.DbModel.Common

let recordValueSeparator = "\t"
let columnNameSeparator = ","
let columnValueNull = "\\N"

let idColumnName = "id"

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


let idOffsetFromRecordType recordType =
(*
Additional offsets are applied to IDs which are assigned to imported records as Discourse exhibited problems when restoring backups without:
An attempt to restore from the sql dump failed with the log containing following lines:

[2016-12-03 11:32:48] EXCEPTION: PG::UniqueViolation: ERROR:  duplicate key value violates unique constraint "topics_pkey"
DETAIL:  Key (id)=(11) already exists.
: INSERT INTO "topics" ("slug", "title", "user_id", "last_post_user_id", "category_id", "fancy_title", "created_at", "updated_at", "bumped_at") VALUES ('terms-of-service', 'Terms of Service', -1, -1, 4, 'Terms of Service', '2016-12-03 11:32:48.876358', '2016-12-03 11:32:48.876358', '2016-12-03 11:32:48.877783') RETURNING "id"

The id 11 was already used for another record in the sql dump to be restored.
To work around the restore fail, the topic IDs will be offset to make room for this record with its unfitting ID.

----------------

After introducing an offset for the topic by one, the next restore failed with the same problem for the post for the topic:

[2016-12-03 11:54:42] EXCEPTION: PG::UniqueViolation: ERROR:  duplicate key value violates unique constraint "posts_pkey"
DETAIL:  Key (id)=(13) already exists.
: INSERT INTO "posts" ("raw", "user_id", "topic_id", "last_editor_id", "baked_at", "baked_version", "created_at", "updated_at", "word_count", "post_number", "cooked", "sort_order", "last_version_at") VALUES ('The following terms and conditions govern all use of the company_domain website and all content, services and products available at or through the website, including, but not limited to, company_domain Forum Software, company_domain Support Forums and the company_domain Hosting service ("Hosting"), (taken together, the Website). The Website is owned and operated by company_full_name ("company_short_name"). The Website is offered subject to your acceptance without modification of all of the terms and conditions contained herein and all other operating rules, policies (including, without limitation, company_domain’s [Privacy Policy](/privacy) and [Community Guidelines](/faq)) and procedures that may be published from time to time on this Site by company_short_name (collectively, the "Agreement").

In order to prevent further collisions, now all record IDs are simply offset by a constant.
*)
    match recordType with
    | _ -> 4 * 4
