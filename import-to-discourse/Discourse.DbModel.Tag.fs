module Discourse.DbModel.Tag

open Discourse.DbModel.Common

(*
Schema from backup from v1.7.0.beta7 +27

CREATE TABLE tags (
    id integer NOT NULL,
    name character varying NOT NULL,
    topic_count integer DEFAULT 0 NOT NULL,
    created_at timestamp without time zone,
    updated_at timestamp without time zone
);

*)

type Tag =
    {
        id: int;
        name: string;
        created_at: System.DateTime Option;
        updated_at: System.DateTime Option;
    }

let columnValueForTag tag columnName =
    match columnName with
        | "id" -> Integer tag.id
        | "name" -> String tag.name
        | "created_at" -> dateTimeOptionAsColumnValue tag.created_at
        | "updated_at" -> dateTimeOptionAsColumnValue tag.updated_at
        | _ -> Default
