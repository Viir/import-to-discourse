module Discourse.DbModel.PostAction

open Discourse.DbModel.Common

(*
Schema from backup from v1.7.0.beta7 +27

CREATE TABLE post_action_types (
    name_key character varying(50) NOT NULL,
    is_flag boolean DEFAULT false NOT NULL,
    icon character varying(20),
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL,
    id integer NOT NULL,
    "position" integer DEFAULT 0 NOT NULL
);

CREATE TABLE post_actions (
    id integer NOT NULL,
    post_id integer NOT NULL,
    user_id integer NOT NULL,
    post_action_type_id integer NOT NULL,
    deleted_at timestamp without time zone,
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL,
    deleted_by_id integer,
    related_post_id integer,
    staff_took_action boolean DEFAULT false NOT NULL,
    deferred_by_id integer,
    targets_topic boolean DEFAULT false NOT NULL,
    agreed_at timestamp without time zone,
    agreed_by_id integer,
    deferred_at timestamp without time zone,
    disagreed_at timestamp without time zone,
    disagreed_by_id integer
);

*)

type PostAction =
    {
        id: int;
        post_id: int;
        user_id: int;
        post_action_type_id: int;
        created_at: System.DateTime;
        updated_at: System.DateTime;
    }

let columnValueForPostAction postAction columnName =
    match columnName with
        | "id" -> Integer postAction.id
        | "post_id" -> Integer postAction.post_id
        | "user_id" -> Integer postAction.user_id
        | "post_action_type_id" -> Integer postAction.post_action_type_id
        | "created_at" -> Time postAction.created_at
        | "updated_at" -> Time postAction.updated_at
        | _ -> Default


