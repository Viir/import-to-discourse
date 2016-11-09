module ModelUser

open Model

(*
Copied from: https://github.com/discourse/discourse/blob/9e69798285078e017501973872f1bf1c2beddd9b/app/models/user.rb#L1041-L1083

# == Schema Information
#
# Table name: users
#
#  id                      :integer          not null, primary key
#  username                :string(60)       not null
#  created_at              :datetime         not null
#  updated_at              :datetime         not null
#  name                    :string
#  seen_notification_id    :integer          default(0), not null
#  last_posted_at          :datetime
#  email                   :string(513)      not null
#  password_hash           :string(64)
#  salt                    :string(32)
#  active                  :boolean          default(FALSE), not null
#  username_lower          :string(60)       not null
#  auth_token              :string(32)
#  last_seen_at            :datetime
#  admin                   :boolean          default(FALSE), not null
#  last_emailed_at         :datetime
#  trust_level             :integer          not null
#  approved                :boolean          default(FALSE), not null
#  approved_by_id          :integer
#  approved_at             :datetime
#  previous_visit_at       :datetime
#  suspended_at            :datetime
#  suspended_till          :datetime
#  date_of_birth           :date
#  views                   :integer          default(0), not null
#  flag_level              :integer          default(0), not null
#  ip_address              :inet
#  moderator               :boolean          default(FALSE)
#  blocked                 :boolean          default(FALSE)
#  title                   :string
#  uploaded_avatar_id      :integer
#  locale                  :string(10)
#  primary_group_id        :integer
#  registration_ip_address :inet
#  trust_level_locked      :boolean          default(FALSE), not null
#  staged                  :boolean          default(FALSE), not null
#  first_seen_at           :datetime
#  auth_token_updated_at   :datetime
#
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
Copied from: https://github.com/discourse/discourse/blob/5dbd6a304bed5400be481d71061d3e3ebb4d6785/app/models/user_stat.rb#L102-L121

# == Schema Information
#
# Table name: user_stats
#
#  user_id                  :integer          not null, primary key
#  topics_entered           :integer          default(0), not null
#  time_read                :integer          default(0), not null
#  days_visited             :integer          default(0), not null
#  posts_read_count         :integer          default(0), not null
#  likes_given              :integer          default(0), not null
#  likes_received           :integer          default(0), not null
#  topic_reply_count        :integer          default(0), not null
#  new_since                :datetime         not null
#  read_faq                 :datetime
#  first_post_created_at    :datetime
#  post_count               :integer          default(0), not null
#  topic_count              :integer          default(0), not null
#  bounce_score             :integer          default(0), not null
#  reset_bounce_score_after :datetime
#
*)

type UserStats =
    {
        user_id: int;
        new_since: System.DateTime;
    }

let userStatsSetColumnValueStatic =
    [
        ("", Default);
    ]

let columnValueForUserStats userStats columnName =
    match columnName with
        | "user_id" -> Integer userStats.user_id
        | "new_since" -> Time userStats.new_since
        | _ -> Default

let columnValueForUserStatsWithDefaults userStats columnName =
    columnValueWithDefaults userStatsSetColumnValueStatic (columnValueForUserStats userStats) columnName

