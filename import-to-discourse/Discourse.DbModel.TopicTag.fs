module Discourse.DbModel.TopicTag

open Discourse.DbModel.Common

(*
Schema from backup from v1.7.0.beta7 +27

CREATE TABLE topic_tags (
    id integer NOT NULL,
    topic_id integer NOT NULL,
    tag_id integer NOT NULL,
    created_at timestamp without time zone,
    updated_at timestamp without time zone
);

*)

type TopicTag =
    {
        id: int;
        topic_id: int;
        tag_id: int;
        created_at: System.DateTime Option;
        updated_at: System.DateTime Option;
    }

let columnValueForTopicTag topicTag columnName =
    match columnName with
        | "id" -> Integer topicTag.id
        | "topic_id" -> Integer topicTag.topic_id
        | "tag_id" -> Integer topicTag.tag_id
        | "created_at" -> dateTimeOptionAsColumnValue topicTag.created_at
        | "updated_at" -> dateTimeOptionAsColumnValue topicTag.updated_at
        | _ -> Default
