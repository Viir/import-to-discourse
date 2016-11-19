module Discourse.DbModel.Topic

open Discourse.DbModel.Common

(*
Schema from backup from v1.7.0.beta7 +27

--
-- TOC entry 422 (class 1259 OID 57981)
-- Name: topics; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE topics (
    id integer NOT NULL,
    title character varying NOT NULL,
    last_posted_at timestamp without time zone,
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL,
    views integer DEFAULT 0 NOT NULL,
    posts_count integer DEFAULT 0 NOT NULL,
    user_id integer,
    last_post_user_id integer NOT NULL,
    reply_count integer DEFAULT 0 NOT NULL,
    featured_user1_id integer,
    featured_user2_id integer,
    featured_user3_id integer,
    avg_time integer,
    deleted_at timestamp without time zone,
    highest_post_number integer DEFAULT 0 NOT NULL,
    image_url character varying,
    off_topic_count integer DEFAULT 0 NOT NULL,
    like_count integer DEFAULT 0 NOT NULL,
    incoming_link_count integer DEFAULT 0 NOT NULL,
    bookmark_count integer DEFAULT 0 NOT NULL,
    category_id integer,
    visible boolean DEFAULT true NOT NULL,
    moderator_posts_count integer DEFAULT 0 NOT NULL,
    closed boolean DEFAULT false NOT NULL,
    archived boolean DEFAULT false NOT NULL,
    bumped_at timestamp without time zone NOT NULL,
    has_summary boolean DEFAULT false NOT NULL,
    vote_count integer DEFAULT 0 NOT NULL,
    archetype character varying DEFAULT 'regular'::character varying NOT NULL,
    featured_user4_id integer,
    notify_moderators_count integer DEFAULT 0 NOT NULL,
    spam_count integer DEFAULT 0 NOT NULL,
    illegal_count integer DEFAULT 0 NOT NULL,
    inappropriate_count integer DEFAULT 0 NOT NULL,
    pinned_at timestamp without time zone,
    score double precision,
    percent_rank double precision DEFAULT 1.0 NOT NULL,
    notify_user_count integer DEFAULT 0 NOT NULL,
    subtype character varying,
    slug character varying,
    auto_close_at timestamp without time zone,
    auto_close_user_id integer,
    auto_close_started_at timestamp without time zone,
    deleted_by_id integer,
    participant_count integer DEFAULT 1,
    word_count integer,
    excerpt character varying(1000),
    pinned_globally boolean DEFAULT false NOT NULL,
    auto_close_based_on_last_post boolean DEFAULT false,
    auto_close_hours double precision,
    pinned_until timestamp without time zone,
    fancy_title character varying(400),
    CONSTRAINT has_category_id CHECK (((category_id IS NOT NULL) OR ((archetype)::text <> 'regular'::text))),
    CONSTRAINT pm_has_no_category CHECK (((category_id IS NULL) OR ((archetype)::text <> 'private_message'::text)))
);

*)

type TopicArchetype =
    | Regular
    | PrivateMessage

type Topic =
    {
        id: int;
        userId: int;
        title: string;
        createdAt: System.DateTime;
        lastPostedAt: System.DateTime;
        updatedAt: System.DateTime;
        viewCount: int;
        postCount: int;
        lastPostUserId: int;
        replyCount: int;
        highestPostNumber: int;
        categoryId: int Option;
        isClosed: bool;
        archetype: TopicArchetype;
        slug: string;
    }

let setColumnValueStatic =
    [
        ("featured_user1_id", Null);
        ("featured_user2_id", Null);
        ("featured_user3_id", Null);
        ("avg_time", Null);
        ("deleted_at", Null);
        ("image_url", Null);
        ("off_topic_count", Default);
        ("like_count", Default);
        ("incoming_link_count", Default);
        ("bookmark_count", Default);
        ("archived", Boolean false);
        ("has_summary", Boolean false);
        ("vote_count", Default);
        ("archetype", Default);
        ("featured_user4_id", Null);
        ("notify_moderators_count", Default);
        ("spam_count", Default);
        ("illegal_count", Default);
        ("inappropriate_count", Default);
        ("pinned_at", Null);
        ("score", Null);
        ("percent_rank", Default);
        ("notify_user_count", Default);
        ("subtype", Null);
        ("auto_close_at", Null);
        ("auto_close_user_id", Null);
        ("auto_close_started_at", Null);
        ("deleted_by_id", Null);
        ("participant_count", Default);
        ("word_count", Null);
        ("excerpt", Null);
        ("pinned_globally", Default);
        ("auto_close_based_on_last_post", Default);
        ("auto_close_hours", Null);
        ("pinned_until", Null);
        ("fancy_title", Null);

        ("", Default);
    ]

let stringFromTopicArchetype archetype =
    match archetype with
    | Regular -> String "regular"
    | PrivateMessage -> String "private_message"

let columnValueForTopic topic columnName =
    match columnName with
        | "id" -> Integer topic.id
        | "user_id" -> Integer topic.userId
        | "title" -> String topic.title
        | "created_at" -> Time topic.createdAt
        | "last_posted_at" -> Time topic.lastPostedAt
        | "updated_at" -> Time topic.updatedAt
        | "views" -> Integer topic.viewCount
        | "posts_count" -> Integer topic.postCount
        | "last_post_user_id" -> Integer topic.lastPostUserId
        | "reply_count" -> Integer topic.replyCount
        | "highest_post_number" -> Integer topic.highestPostNumber
        | "category_id" -> intOptionAsColumnValue topic.categoryId
        | "closed" -> Boolean topic.isClosed
        | "bumped_at" -> Time topic.lastPostedAt
        | "archetype" -> stringFromTopicArchetype topic.archetype
        | "slug" -> String topic.slug
        | _ -> Default

let columnValueForTopicWithDefaults topic columnName =
    columnValueWithDefaults setColumnValueStatic (columnValueForTopic topic) columnName

