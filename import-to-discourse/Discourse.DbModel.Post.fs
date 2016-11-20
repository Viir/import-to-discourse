module Discourse.DbModel.Post

open Discourse.DbModel.Common

(*
Schema from backup from v1.7.0.beta7 +27

--
-- TOC entry 418 (class 1259 OID 40109)
-- Name: posts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE posts (
    id integer NOT NULL,
    user_id integer,
    topic_id integer NOT NULL,
    post_number integer NOT NULL,
    raw text NOT NULL,
    cooked text NOT NULL,
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL,
    reply_to_post_number integer,
    reply_count integer DEFAULT 0 NOT NULL,
    quote_count integer DEFAULT 0 NOT NULL,
    deleted_at timestamp without time zone,
    off_topic_count integer DEFAULT 0 NOT NULL,
    like_count integer DEFAULT 0 NOT NULL,
    incoming_link_count integer DEFAULT 0 NOT NULL,
    bookmark_count integer DEFAULT 0 NOT NULL,
    avg_time integer,
    score double precision,
    reads integer DEFAULT 0 NOT NULL,
    post_type integer DEFAULT 1 NOT NULL,
    vote_count integer DEFAULT 0 NOT NULL,
    sort_order integer,
    last_editor_id integer,
    hidden boolean DEFAULT false NOT NULL,
    hidden_reason_id integer,
    notify_moderators_count integer DEFAULT 0 NOT NULL,
    spam_count integer DEFAULT 0 NOT NULL,
    illegal_count integer DEFAULT 0 NOT NULL,
    inappropriate_count integer DEFAULT 0 NOT NULL,
    last_version_at timestamp without time zone NOT NULL,
    user_deleted boolean DEFAULT false NOT NULL,
    reply_to_user_id integer,
    percent_rank double precision DEFAULT 1.0,
    notify_user_count integer DEFAULT 0 NOT NULL,
    like_score integer DEFAULT 0 NOT NULL,
    deleted_by_id integer,
    edit_reason character varying,
    word_count integer,
    version integer DEFAULT 1 NOT NULL,
    cook_method integer DEFAULT 1 NOT NULL,
    wiki boolean DEFAULT false NOT NULL,
    baked_at timestamp without time zone,
    baked_version integer,
    hidden_at timestamp without time zone,
    self_edits integer DEFAULT 0 NOT NULL,
    reply_quoted boolean DEFAULT false NOT NULL,
    via_email boolean DEFAULT false NOT NULL,
    raw_email text,
    public_version integer DEFAULT 1 NOT NULL,
    action_code character varying,
    image_url character varying
);
*)

(*
Example for cooking:
Two posts from sql dump demonstrating the mapping(similar to markdown) from raw to cooked (Including link, blockquote, preformatted and quote):

COPY posts (id, user_id, topic_id, post_number, raw, cooked, created_at, updated_at, reply_to_post_number, reply_count, quote_count, deleted_at, off_topic_count, like_count, incoming_link_count, bookmark_count, avg_time, score, reads, post_type, vote_count, sort_order, last_editor_id, hidden, hidden_reason_id, notify_moderators_count, spam_count, illegal_count, inappropriate_count, last_version_at, user_deleted, reply_to_user_id, percent_rank, notify_user_count, like_score, deleted_by_id, edit_reason, word_count, version, cook_method, wiki, baked_at, baked_version, hidden_at, self_edits, reply_quoted, via_email, raw_email, public_version, action_code, image_url) FROM stdin;
19	1000	14	1	Using code formatting:\n\n```c#\nusing System;\n\n#pragma warning disable 414, 3021\n\n/// <summary>Main task</summary>\nasync Task<int, int> AccessTheWebAsync()\n{\n    Console.WriteLine("Hello, World!");\n    string urlContents = await getStringTask;\n    return urlContents.Length;\n}\n\npublic class BotStepResult\n{\n   public Exception Exception;\n\n   public MotionRecommendation[] ListMotion;\n\t\n   public IBotTask[][] OutputListTaskPath;\n\n   public int MethodName()\n   {\n       return 4;\n   }\n}\n```\n\nAnd a link: [http://distilledgames.de](http://distilledgames.de)	<p>Using code formatting:</p>\n\n<p></p><pre><code class="lang-auto">using System;\n\n#pragma warning disable 414, 3021\n\n/// &lt;summary&gt;Main task&lt;/summary&gt;\n\n\nasync Task&lt;int, int&gt; AccessTheWebAsync()\n{\n    Console.WriteLine("Hello, World!");\n    string urlContents = await getStringTask;\n    return urlContents.Length;\n}\n\npublic class BotStepResult\n{\n   public Exception Exception;\n\n   public MotionRecommendation[] ListMotion;\n\t\n   public IBotTask[][] OutputListTaskPath;\n\n   public int MethodName()\n   {\n       return 4;\n   }\n}</code></pre>\n\n<p>And a link: <a href="http://distilledgames.de" rel="nofollow">http://distilledgames.de</a></p>	2016-11-10 20:41:39.697865	2016-11-10 20:47:20.015402	\N	1	0	\N	0	0	0	0	5	0.650000000000000022	2	1	0	1	1000	f	\N	0	0	0	0	2016-11-10 20:47:19.351903	f	\N	0	0	0	\N	\N	57	2	1	f	2016-11-10 20:47:20.012464	1	\N	4	f	f	\N	2	\N	\N
20	1	14	2	> This is a Blockquote\n> Another line in Blockquote\n\n    Preformatted Text\n    More preformatted Text\n\n* List item\n\nFull quote below:\n\n[quote="username-added-by-import, post:1, topic:14, full:true"]\nUsing code formatting:\n\n```c#\nusing System;\n\n#pragma warning disable 414, 3021\n\n/// <summary>Main task</summary>\nasync Task<int, int> AccessTheWebAsync()\n{\n    Console.WriteLine("Hello, World!");\n    string urlContents = await getStringTask;\n    return urlContents.Length;\n}\n\npublic class BotStepResult\n{\n   public Exception Exception;\n\n   public MotionRecommendation[] ListMotion;\n\t\n   public IBotTask[][] OutputListTaskPath;\n\n   public int MethodName()\n   {\n       return 4;\n   }\n}\n```\n\nAnd a link: [http://distilledgames.de](http://distilledgames.de)\n[/quote]	<blockquote><p>This is a Blockquote<br>Another line in Blockquote</p></blockquote>\n\n<pre><code>Preformatted Text\nMore preformatted Text</code></pre>\n\n<ul><li>List item</li></ul>\n\n<p>Full quote below:</p>\n\n<aside class="quote" data-post="1" data-topic="14" data-full="true"><div class="title">\n<div class="quote-controls"></div>\n<img alt="" width="20" height="20" src="//discourse.demo.botengine.de/letter_avatar_proxy/v2/letter/u/e56c9b/40.png" class="avatar">username-added-by-import:</div>\n<blockquote>\n<p>Using code formatting:</p>\n<p><code></code>`c#<br>using System;</p>\n<p><span class="hashtag">#pragma</span> warning disable 414, 3021</p>\n<p>/// <summary>Main task</summary></p>\n<p>async Task AccessTheWebAsync()<br>{<br>    Console.WriteLine("Hello, World!");<br>    string urlContents = await getStringTask;<br>    return urlContents.Length;<br>}</p>\n<p>public class BotStepResult<br>{<br>   public Exception Exception;</p>\n<p>   public MotionRecommendation[] ListMotion;</p>\n<p>   public IBotTask[][] OutputListTaskPath;</p>\n<p>   public int MethodName()<br>   {<br>       return 4;<br>   }<br>}<br><code></code>`</p>\n<p>And a link: <a href="http://distilledgames.de">http://distilledgames.de</a></p>\n</blockquote></aside>	2016-11-13 09:38:32.478365	2016-11-13 09:38:32.478365	\N	0	1	\N	0	0	0	0	\N	\N	1	1	0	2	1	f	\N	0	0	0	0	2016-11-13 09:38:32.534202	f	\N	1	0	0	\N	\N	87	1	1	f	2016-11-13 09:38:32.476529	1	\N	0	f	f	\N	1	\N	\N

*)

type Post =
    {
        id : int;
        user_id : int Option;
        topic_id : int;
        post_number : int;
        raw : string;
        cooked : string;
        created_at : System.DateTime;
        updated_at : System.DateTime;
        reply_to_post_number : int Option;
        deleted_at : System.DateTime Option;
        reads : int;
        last_editor_id : int Option;
        last_version_at : System.DateTime;
        sort_order : int Option;
    }

let columnValueForPost post columnName =
    match columnName with
        | "id" -> Integer post.id
        | "user_id" -> intOptionAsColumnValue post.user_id
        | "topic_id" -> Integer post.topic_id
        | "post_number" -> Integer post.post_number
        | "raw" -> String post.raw
        | "cooked" -> String post.cooked
        | "created_at" -> Time post.created_at
        | "updated_at" -> Time post.updated_at
        | "reply_to_post_number" -> intOptionAsColumnValue post.reply_to_post_number
        | "deleted_at" -> dateTimeOptionAsColumnValue post.deleted_at
        | "reads" -> Integer post.reads
        | "last_editor_id" -> intOptionAsColumnValue post.last_editor_id
        | "last_version_at" -> Time post.last_version_at
        | "sort_order" -> intOptionAsColumnValue post.sort_order
        | _ -> Default
