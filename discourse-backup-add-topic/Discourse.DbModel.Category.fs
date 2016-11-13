module Discourse.DbModel.Category

open Discourse.DbModel.Common

(*
Schema from backup from v1.7.0.beta7 +27

--
-- TOC entry 438 (class 1259 OID 40583)
-- Name: categories; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE categories (
    id integer NOT NULL,
    name character varying(50) NOT NULL,
    color character varying(6) DEFAULT 'AB9364'::character varying NOT NULL,
    topic_id integer,
    topic_count integer DEFAULT 0 NOT NULL,
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL,
    user_id integer NOT NULL,
    topics_year integer DEFAULT 0,
    topics_month integer DEFAULT 0,
    topics_week integer DEFAULT 0,
    slug character varying NOT NULL,
    description text,
    text_color character varying(6) DEFAULT 'FFFFFF'::character varying NOT NULL,
    read_restricted boolean DEFAULT false NOT NULL,
    auto_close_hours double precision,
    post_count integer DEFAULT 0 NOT NULL,
    latest_post_id integer,
    latest_topic_id integer,
    "position" integer,
    parent_category_id integer,
    posts_year integer DEFAULT 0,
    posts_month integer DEFAULT 0,
    posts_week integer DEFAULT 0,
    email_in character varying,
    email_in_allow_strangers boolean DEFAULT false,
    topics_day integer DEFAULT 0,
    posts_day integer DEFAULT 0,
    logo_url character varying,
    background_url character varying,
    allow_badges boolean DEFAULT true NOT NULL,
    name_lower character varying(50) NOT NULL,
    auto_close_based_on_last_post boolean DEFAULT false,
    topic_template text,
    suppress_from_homepage boolean DEFAULT false,
    contains_messages boolean,
    sort_order character varying,
    sort_ascending boolean
);
*)

type Category =
    {
        id: int;
        name: string;
        created_at: System.DateTime;
        updated_at: System.DateTime;
        user_id: int;
        slug: string;
        description: string;
        parent_category_id: int Option;
    }

let columnValueForCategory category columnName =
    match columnName with
        | "id" -> Integer category.id
        | "name" -> String category.name
        | "created_at" -> Time category.created_at
        | "updated_at" -> Time category.updated_at
        | "user_id" -> Integer category.user_id
        | "slug" -> String category.slug
        | "description" -> String category.description
        | "parent_category_id" -> intOptionAsColumnValue category.parent_category_id
        | "name_lower" -> String (category.name.ToLowerInvariant())
        | _ -> Default
