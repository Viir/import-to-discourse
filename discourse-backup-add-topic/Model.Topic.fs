module ModelTopic

open Model

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
        categoryId: int;
        isClosed: bool;
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
        ("bumped_at", Time System.DateTime.MinValue);
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
        | "category_id" -> Integer topic.categoryId
        | "closed" -> Boolean topic.isClosed
        | "slug" -> String topic.slug
        | _ -> Default

let columnValueForTopicWithDefaults topic columnName =
    columnValueWithDefaults setColumnValueStatic (columnValueForTopic topic) columnName

