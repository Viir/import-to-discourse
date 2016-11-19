module Discourse.DbModel.User

open Discourse.DbModel.Common

(*
Schema from backup from v1.7.0.beta7 +27

--
-- TOC entry 622 (class 1259 OID 58839)
-- Name: users; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE users (
    id integer NOT NULL,
    username character varying(60) NOT NULL,
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL,
    name character varying,
    seen_notification_id integer DEFAULT 0 NOT NULL,
    last_posted_at timestamp without time zone,
    email character varying(513) NOT NULL,
    password_hash character varying(64),
    salt character varying(32),
    active boolean DEFAULT false NOT NULL,
    username_lower character varying(60) NOT NULL,
    auth_token character varying(32),
    last_seen_at timestamp without time zone,
    admin boolean DEFAULT false NOT NULL,
    last_emailed_at timestamp without time zone,
    trust_level integer NOT NULL,
    approved boolean DEFAULT false NOT NULL,
    approved_by_id integer,
    approved_at timestamp without time zone,
    previous_visit_at timestamp without time zone,
    suspended_at timestamp without time zone,
    suspended_till timestamp without time zone,
    date_of_birth date,
    views integer DEFAULT 0 NOT NULL,
    flag_level integer DEFAULT 0 NOT NULL,
    ip_address inet,
    moderator boolean DEFAULT false,
    blocked boolean DEFAULT false,
    title character varying,
    uploaded_avatar_id integer,
    locale character varying(10),
    primary_group_id integer,
    registration_ip_address inet,
    trust_level_locked boolean DEFAULT false NOT NULL,
    staged boolean DEFAULT false NOT NULL,
    first_seen_at timestamp without time zone,
    auth_token_updated_at timestamp without time zone
);
*)

type User =
    {
        id: int;
        username: string;
        createdAt: System.DateTime;
        updatedAt: System.DateTime;
        name: string;
        email: string;
        last_posted_at: System.DateTime Option;
        last_seen_at: System.DateTime Option;
        trust_level: int;
        registration_ip_address: string;
        first_seen_at: System.DateTime Option;

        profile_location: string;
        profile_website: string;
    }

let userSetColumnValueStatic =
    [
        ("", Default);
    ]

let columnValueForUser user columnName =
    match columnName with
        | "id" -> Integer user.id
        | "username" -> String user.username
        | "created_at" -> Time user.createdAt
        | "updated_at" -> Time user.updatedAt
        | "name" -> String user.name
        | "email" -> String user.email
        | "username_lower" -> String (user.username.ToLowerInvariant())
        | "last_posted_at" -> dateTimeOptionAsColumnValue user.last_posted_at
        | "last_seen_at" -> dateTimeOptionAsColumnValue user.last_seen_at
        | "trust_level" -> Integer user.trust_level
        | "registration_ip_address" -> String user.registration_ip_address
        | "first_seen_at" -> dateTimeOptionAsColumnValue user.first_seen_at
        | _ -> Default

let columnValueForUserWithDefaults user columnName =
    columnValueWithDefaults userSetColumnValueStatic (columnValueForUser user) columnName

(*
Schema from backup from v1.7.0.beta7 +27

--
-- TOC entry 614 (class 1259 OID 58780)
-- Name: user_options; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE user_options (
    user_id integer NOT NULL,
    email_always boolean DEFAULT false NOT NULL,
    mailing_list_mode boolean DEFAULT false NOT NULL,
    email_digests boolean,
    email_direct boolean DEFAULT true NOT NULL,
    email_private_messages boolean DEFAULT true NOT NULL,
    external_links_in_new_tab boolean DEFAULT false NOT NULL,
    enable_quoting boolean DEFAULT true NOT NULL,
    dynamic_favicon boolean DEFAULT false NOT NULL,
    disable_jump_reply boolean DEFAULT false NOT NULL,
    automatically_unpin_topics boolean DEFAULT true NOT NULL,
    digest_after_minutes integer,
    auto_track_topics_after_msecs integer,
    new_topic_duration_minutes integer,
    last_redirected_to_top_at timestamp without time zone,
    email_previous_replies integer DEFAULT 2 NOT NULL,
    email_in_reply_to boolean DEFAULT true NOT NULL,
    like_notification_frequency integer DEFAULT 1 NOT NULL,
    mailing_list_mode_frequency integer DEFAULT 0 NOT NULL,
    include_tl0_in_digests boolean DEFAULT false,
    notification_level_when_replying integer
);
*)

let userOptionsSetColumnValueStatic =
    [
        ("email_digests", Boolean true);
        ("digest_after_minutes", Integer 10080);
        ("auto_track_topics_after_msecs", Integer 240000);
        ("new_topic_duration_minutes", Integer 2880);

        ("", Default);
    ]

let columnValueForUserOptions (user:User) columnName =
    match columnName with
        | "user_id" -> Integer user.id
        | _ -> Default

let columnValueForUserOptionsWithDefaults user columnName =
    columnValueWithDefaults userOptionsSetColumnValueStatic (columnValueForUserOptions user) columnName


(*
Schema from backup from v1.7.0.beta7 +27

--
-- TOC entry 617 (class 1259 OID 58805)
-- Name: user_profiles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE user_profiles (
    user_id integer NOT NULL,
    location character varying,
    website character varying,
    bio_raw text,
    bio_cooked text,
    profile_background character varying(255),
    dismissed_banner_key integer,
    bio_cooked_version integer,
    badge_granted_title boolean DEFAULT false,
    card_background character varying(255),
    card_image_badge_id integer,
    views integer DEFAULT 0 NOT NULL
);

*)

type UserProfile =
    {
        user_id: int;
        location: string;
    }

let columnValueForUserProfile user columnName =
    match columnName with
        | "user_id" -> Integer user.id
        | "location" -> String user.profile_location
        | "website" -> String user.profile_website
        | _ -> Default


(*
Schema from backup from v1.7.0.beta7 +27

--
-- TOC entry 505 (class 1259 OID 41942)
-- Name: user_stats; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE user_stats (
    user_id integer NOT NULL,
    topics_entered integer DEFAULT 0 NOT NULL,
    time_read integer DEFAULT 0 NOT NULL,
    days_visited integer DEFAULT 0 NOT NULL,
    posts_read_count integer DEFAULT 0 NOT NULL,
    likes_given integer DEFAULT 0 NOT NULL,
    likes_received integer DEFAULT 0 NOT NULL,
    topic_reply_count integer DEFAULT 0 NOT NULL,
    new_since timestamp without time zone NOT NULL,
    read_faq timestamp without time zone,
    first_post_created_at timestamp without time zone,
    post_count integer DEFAULT 0 NOT NULL,
    topic_count integer DEFAULT 0 NOT NULL,
    bounce_score integer DEFAULT 0 NOT NULL,
    reset_bounce_score_after timestamp without time zone
);
*)

let userStatsSetColumnValueStatic =
    [
        ("", Default);
    ]

let columnValueForUserStats (user : User) columnName =
    match columnName with
        | "user_id" -> Integer user.id
        | "new_since" -> Time System.DateTime.MinValue
        | _ -> Default

let columnValueForUserStatsWithDefaults userStats columnName =
    columnValueWithDefaults userStatsSetColumnValueStatic (columnValueForUserStats userStats) columnName

