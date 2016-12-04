module Discourse.DbModel.Permalink

open Discourse.DbModel.Common

(*
Schema from backup from v1.7.0.beta7 +27

CREATE TABLE permalinks (
    id integer NOT NULL,
    url character varying(1000) NOT NULL,
    topic_id integer,
    post_id integer,
    category_id integer,
    created_at timestamp without time zone,
    updated_at timestamp without time zone,
    external_url character varying(1000)
);

*)

type Permalink =
    {
        id: int;
        url: string;
        topic_id: int Option;
        post_id: int Option;
        category_id: int Option;
        external_url: string;
        created_at: System.DateTime Option;
        updated_at: System.DateTime Option;
    }

let columnValueForPermalink permalink columnName =
    match columnName with
        | "id" -> Integer permalink.id
        | "url" -> String permalink.url
        | "topic_id" -> intOptionAsColumnValue permalink.topic_id
        | "post_id" -> intOptionAsColumnValue permalink.post_id
        | "category_id" -> intOptionAsColumnValue permalink.category_id
        | "external_url" -> String permalink.external_url
        | "created_at" -> dateTimeOptionAsColumnValue permalink.created_at
        | "updated_at" -> dateTimeOptionAsColumnValue permalink.updated_at
        | _ -> Default
