module Import.mvcforum

open System
open System.Linq
open System.Xml

let userXPath = "//SetMembershipUser/MembershipUser"
let categoryXPath = "//SetCategory/Category"
let topicXPath = "//SetTopic/Topic"
let postXPath = "//SetPost/Post"
let tagXPath = "//SetTopicTag/TopicTag"
let topicTagXPath = "//SetTopic_Tag/Topic_Tag"
let voteXPath = "//SetVote/Vote"

type User = 
    {
        Id : string;
        UserName : string;
        Email : string;
        CreateDate : System.DateTime;
        LastLoginDate : System.DateTime Option;
        LastActivityDate : System.DateTime Option;
        Slug : string;
        Location : string;
        Website : string;
    }

type Category = 
    {
        Id : string;
        Name : string;
        Description : string;
        DateCreated : System.DateTime;
        PageTitle : string;
        Slug : string;
        Category_Id : string;
    }

type Topic = 
    {
        Id : string;
        MembershipUser_Id : string;
        Name : string;
        CreateDate : System.DateTime;
        Solved : int;
        Slug : string;
        Views : int;
        IsSticky : int;
        IsLocked : int;
        Category_Id : string;
        Post_Id : string;
    }

type Post = 
    {
        Id : string;
        MembershipUser_Id : string;
        Topic_Id : string;
        DateCreated : System.DateTime;
        DateEdited : System.DateTime;
        PostContent : string;
        VoteCount : int;
        IsSolution : int;
        IsTopicStarter : int;
        IpAddress : string;
    }

type Tag =
    {
        Id : string;
        Tag : string;
        Slug : string;
    }

type TopicTag =
    {
        Topic_Id : string;
        TopicTag_Id : string;
    }

type Vote =
    {
        Id : string;
        Amount : int;
        DateVoted : DateTime;
        Post_Id : string;
        VotedByMembershipUser_Id : string;
    }

type PermalinkSource =
    | Category of Category
    | Topic of Topic

let permalinkSourceUrlFromCategory (category : Category) =
    "cat/" + category.Slug

let permalinkSourceUrlFromTopic (topic : Topic) =
    "thread/" + topic.Slug

let dictWithIndexAsKey listRecord =
    listRecord |> List.mapi (fun record index -> (record, index)) |> dict

let getValueFromSingleTextNode (xmlElement : XmlElement) =
    if xmlElement <> null && xmlElement.ChildNodes.Count = 1
    then
        match xmlElement.ChildNodes.[0] with
        | :? XmlText as text -> text.Value
        | _ -> null
    else null

let getValueFromSingleChildElementWithName (xmlElement : XmlElement) childElementName =
    let childElement = xmlElement.GetElementsByTagName(childElementName).OfType<XmlElement>().SingleOrDefault()
    getValueFromSingleTextNode childElement

let parseTimeOption timeString =
    match timeString with
    | null -> None
    | _ -> Some (DateTime.Parse(timeString))

let userFromXmlElement (xmlElement : XmlElement) =
    let getColumnValue = getValueFromSingleChildElementWithName xmlElement
    let getColumnTimeValue columnName = DateTime.Parse (getColumnValue columnName)
    let getColumnTimeOptionValue columnName = parseTimeOption (getColumnValue columnName)

    {
        Id = getColumnValue "Id"
        UserName = getColumnValue "UserName"
        Email = getColumnValue "Email"
        CreateDate = getColumnTimeValue "CreateDate"
        LastLoginDate = getColumnTimeOptionValue "LastLoginDate"
        LastActivityDate = getColumnTimeOptionValue "LastActivityDate"
        Slug = getColumnValue "Slug"
        Location = getColumnValue "Location"
        Website = getColumnValue "Website"
    }

let categoryFromXmlElement (xmlElement : XmlElement) =
    let getColumnValue = getValueFromSingleChildElementWithName xmlElement
    let getColumnTimeValue columnName = DateTime.Parse (getColumnValue columnName)
    let getColumnTimeOptionValue columnName = parseTimeOption (getColumnValue columnName)

    {
        Id = getColumnValue "Id"
        Name = getColumnValue "Name"
        Description = getColumnValue "Description"
        DateCreated = getColumnTimeValue "DateCreated"
        PageTitle = getColumnValue "PageTitle"
        Slug = getColumnValue "Slug"
        Category_Id = getColumnValue "Category_Id"
    }

let topicFromXmlElement (xmlElement : XmlElement) =
    let getColumnValue = getValueFromSingleChildElementWithName xmlElement
    let getColumnIntValue columnName = Int32.Parse (getColumnValue columnName)
    let getColumnTimeValue columnName = DateTime.Parse (getColumnValue columnName)

    {
        Id = getColumnValue "Id"
        MembershipUser_Id = getColumnValue "MembershipUser_Id"
        Name = getColumnValue "Name"
        CreateDate = getColumnTimeValue "CreateDate"
        Solved = getColumnIntValue "Solved"
        Views = getColumnIntValue "Views"
        IsSticky = getColumnIntValue "IsSticky"
        IsLocked = getColumnIntValue "IsLocked"
        Slug = getColumnValue "Slug"
        Category_Id = getColumnValue "Category_Id"
        Post_Id = getColumnValue "Post_Id"
    }

let postFromXmlElement (xmlElement : XmlElement) =
    let getColumnValue = getValueFromSingleChildElementWithName xmlElement
    let getColumnIntValue columnName = Int32.Parse (getColumnValue columnName)
    let getColumnTimeValue columnName = DateTime.Parse (getColumnValue columnName)

    {
        Id = getColumnValue "Id"
        MembershipUser_Id = getColumnValue "MembershipUser_Id"
        Topic_Id = getColumnValue "Topic_Id"
        PostContent = getColumnValue "PostContent"
        DateCreated = getColumnTimeValue "DateCreated"
        DateEdited = getColumnTimeValue "DateEdited"
        IsTopicStarter = getColumnIntValue "IsTopicStarter"
        IsSolution = getColumnIntValue "IsSolution"
        VoteCount = getColumnIntValue "VoteCount"
        IpAddress = getColumnValue "IpAddress"
    }

let tagFromXmlElement (xmlElement : XmlElement) =
    let getColumnValue = getValueFromSingleChildElementWithName xmlElement
    {
        Id = getColumnValue "Id"
        Tag = getColumnValue "Tag"
        Slug = getColumnValue "Slug"
    }

let topicTagFromXmlElement (xmlElement : XmlElement) =
    let getColumnValue = getValueFromSingleChildElementWithName xmlElement
    {
        Topic_Id = getColumnValue "Topic_Id"
        TopicTag_Id = getColumnValue "TopicTag_Id"
    }

let voteFromXmlElement (xmlElement : XmlElement) =
    let getColumnValue = getValueFromSingleChildElementWithName xmlElement
    let getColumnIntValue columnName = Int32.Parse (getColumnValue columnName)
    let getColumnTimeValue columnName = DateTime.Parse (getColumnValue columnName)
    {
        Id = getColumnValue "Id"
        Amount = getColumnIntValue "Amount"
        DateVoted = getColumnTimeValue "DateVoted"
        Post_Id = getColumnValue "Post_Id"
        VotedByMembershipUser_Id = getColumnValue "VotedByMembershipUser_Id"
    }

let categoryDefinitionTopicId categoryId =
    categoryId + "-category-definition-topic"

let discourseUser (listPost : Post list) (user : User)
    : Discourse.DbModel.User.User =
    let isFromThisUser post = post.MembershipUser_Id = user.Id
    let fistPostOption = listPost |> List.tryFind isFromThisUser
    let lastPostOption = listPost |> List.tryFindBack isFromThisUser
    let postOptionDateCreated postOption = match postOption with | None -> None | Some post -> Some post.DateCreated
    {
        id = -1;
        name = null;
        username = user.UserName;
        createdAt = user.CreateDate;
        updatedAt = System.DateTime.MinValue;
        email = user.Email;
        last_posted_at = postOptionDateCreated lastPostOption;
        last_seen_at = user.LastActivityDate;
        trust_level = 0;
        registration_ip_address = null;
        first_seen_at = postOptionDateCreated fistPostOption;
        profile_location = user.Location;
        profile_website = user.Website;
    }

let discourseCategory discourseCategoryId discourseTopicId (category : Category)
    : Discourse.DbModel.Category.Category =
    let definitionTopicId = discourseTopicId (categoryDefinitionTopicId category.Id)
    {
        id = -1;
        name = category.Name;
        created_at = category.DateCreated;
        updated_at = System.DateTime.MinValue;
        user_id = -1;
        slug = category.Slug;
        description = category.Description;
        parent_category_id = discourseCategoryId category.Category_Id;
        topic_id = definitionTopicId;
    }

let discourseTopic discourseUserId discourseCategoryId (listPost : Post list) (topic : Topic)
    : Discourse.DbModel.Topic.Topic =
    let listPostInTopic = listPost |> List.filter (fun post -> post.Topic_Id = topic.Id)
    let lastPost = listPostInTopic |> List.last

    let postCount = listPostInTopic |> List.length

    {
        id = -1;
        user_id = discourseUserId topic.MembershipUser_Id;
        slug = topic.Slug;
        title = topic.Name;
        created_at = topic.CreateDate;
        last_posted_at = lastPost.DateCreated;
        updated_at = DateTime.MinValue;
        views = topic.Views;
        posts_count = postCount;
        last_post_user_id = discourseUserId lastPost.MembershipUser_Id;
        reply_count = postCount - 1;
        highest_post_number = postCount;
        category_id = discourseCategoryId topic.Category_Id;
        closed = 0 < topic.IsLocked;
        archetype = Discourse.DbModel.Topic.Regular;
    }

let discoursePost discourseUserId discourseTopicId discoursePostNumber (post : Post)
    : Discourse.DbModel.Post.Post =
    let post_number = discoursePostNumber post
    {
        id = -1;
        user_id = Some (discourseUserId post.MembershipUser_Id);
        topic_id = discourseTopicId post.Topic_Id;
        post_number = post_number;
        raw = (PostContent.postContentRawFromHtml post.PostContent) + "";
        cooked = post.PostContent + "";
        created_at = post.DateCreated;
        updated_at = DateTime.MinValue;
        reply_to_post_number = None;
        deleted_at = None;
        reads = 0;
        last_editor_id = None;
        last_version_at = post.DateEdited;
        sort_order = Some post_number;
        like_count = 0;
    }

let discourseTag (tag : Tag)
    : Discourse.DbModel.Tag.Tag =
    {
        id = -1
        name = tag.Tag
        created_at = None
        updated_at = None
    }

let discourseTopicTag discourseTopicId discourseTagId (topicFromId : string -> Topic) (topicTag : TopicTag)
    : Discourse.DbModel.TopicTag.TopicTag =
    let discourseTopicId = discourseTopicId topicTag.Topic_Id
    let topic = topicFromId topicTag.Topic_Id
    {
        id = -1
        topic_id = discourseTopicId
        tag_id = discourseTagId topicTag.TopicTag_Id
        created_at = Some topic.CreateDate
        updated_at = None
    }

let discoursePermalinkFromCategoryAndTopic url categoryId topicId
    : Discourse.DbModel.Permalink.Permalink =
    {
        id = -1
        url = url
        category_id = categoryId
        topic_id = topicId
        post_id = None
        external_url = null
        created_at = Some DateTime.UtcNow
        updated_at = None
    }

let discoursePermalinkFromPermalinkSource discourseCategoryId discourseTopicId permalinkSource
    : Discourse.DbModel.Permalink.Permalink =
    match permalinkSource with
    | Category category ->
        discoursePermalinkFromCategoryAndTopic
            (permalinkSourceUrlFromCategory category)
            (Some (discourseCategoryId category.Id))
            None
    | Topic topic ->
        discoursePermalinkFromCategoryAndTopic
            (permalinkSourceUrlFromTopic topic)
            None
            (Some (discourseTopicId topic.Id))


let categoryDefinitionTopicAndPostFromCategory
    discourseCategoryId
    (category : Category)
    id
    : (Topic * Post) =
    let topicId = categoryDefinitionTopicId category.Id
    ({
        Id = topicId;
        Category_Id = category.Id;
        MembershipUser_Id = null;
        Name = category.Name;
        CreateDate = category.DateCreated;
        Solved = 0;
        Slug = category.Slug;
        Views = 0;
        IsSticky = 0;
        IsLocked = 0;
        Post_Id = null;
    },
    {
        Id = category.Id + "-category-definition-post";
        MembershipUser_Id = null;
        Topic_Id = topicId;
        DateCreated = category.DateCreated;
        DateEdited = category.DateCreated;
        PostContent = category.Description;
        VoteCount = 0;
        IsSolution = 0;
        IsTopicStarter = 0;
        IpAddress = null;
    })

let transformToDiscourse
    ((listUser : User list), userIdBase)
    ((listCategory : Category list), categoryIdBase)
    ((listTopicLessCategoryDefinition : Topic list), topicIdBase)
    ((listPostLessCategoryDefinition : Post list), postIdBase)
    ((listTag : Tag list), tagIdBase)
    ((listTopicTag : TopicTag list), topicTagIdBase)
    permalinkIdBase
    (listVote : Vote list)
    postActionIdBase
    postActionTypeLikeIdOption
    =
    let discourseUserId userId =
        if userId = null
        then -1
        else (listUser |> List.findIndex (fun user -> user.Id = userId)) + userIdBase

    let discourseCategoryIdOption categoryId =
        listCategory |> List.tryFindIndex (fun category -> category.Id = categoryId)
        |> Option.map (fun id -> id + categoryIdBase)

    let listCategoryDefinitionTopicAndPost =
        listCategory
        |> List.mapi (fun index category -> categoryDefinitionTopicAndPostFromCategory discourseCategoryIdOption category (index.ToString()))

    let listCategoryDefinitionTopicId =
        listCategoryDefinitionTopicAndPost |> List.map fst;

    let listTopic =
        [listCategoryDefinitionTopicId; listTopicLessCategoryDefinition] |> List.concat

    let topicFromId topicId =
        listTopic
        |> List.pick (fun c -> if c.Id = topicId then Some c else None)

    let listPost =
        [(listCategoryDefinitionTopicAndPost |> List.map snd); listPostLessCategoryDefinition] |> List.concat

    let discourseTopicId topicId =
        (listTopic |> List.findIndex (fun topic -> topic.Id = topicId)) + topicIdBase

    let discoursePostId postId =
        (listPost |> List.findIndex (fun post -> post.Id = postId)) + postIdBase

    let discourseTagId tagId =
        (listTag |> List.findIndex (fun tag -> tag.Id = tagId)) + tagIdBase

    let discoursePostNumber (post : Post) =
        (listPost |> List.filter (fun otherPost -> otherPost.Topic_Id = post.Topic_Id && otherPost.DateCreated < post.DateCreated)
        |> List.length) + 1

    let setUserDiscourse =
        listUser
        |> List.map (fun user -> {(discourseUser listPost user) with id = (discourseUserId user.Id)})

    let setCategoryDiscourse =
        listCategory
        |> List.map (fun category ->
            {(discourseCategory discourseCategoryIdOption discourseTopicId category) with id = (discourseCategoryIdOption category.Id).Value})

    let setTopicDiscourse =
        listTopic
        |> List.map (fun topic -> {(discourseTopic discourseUserId discourseCategoryIdOption listPost topic) with id = (discourseTopicId topic.Id)})

    let setPostAction =
        match postActionTypeLikeIdOption with
        | None -> []
        | Some postActionTypeLikeId ->
            listVote
            |> List.filter (fun vote -> 1 <= vote.Amount)
            |> List.mapi (fun index vote ->
                {
                    id = index + postActionIdBase
                    post_id = discoursePostId vote.Post_Id
                    user_id = discourseUserId vote.VotedByMembershipUser_Id
                    post_action_type_id = postActionTypeLikeId
                    created_at = vote.DateVoted
                    updated_at = vote.DateVoted
                } : Discourse.DbModel.PostAction.PostAction)
    
    let likeCountFromDiscoursePostId discoursePostId =
        setPostAction
        |> List.filter (fun action -> action.post_id = discoursePostId && (Some action.post_action_type_id) = postActionTypeLikeIdOption)
        |> List.length

    let setPostDiscourse =
        listPost
        |> List.map (fun post ->
            {(discoursePost discourseUserId discourseTopicId discoursePostNumber post) with id = (discoursePostId post.Id)})
        |> List.map (fun post -> { post with like_count = (likeCountFromDiscoursePostId post.id)})

    let setTagDiscourse =
        listTag
        |> List.map (fun tag -> {(discourseTag tag) with id = (discourseTagId tag.Id)})

    let setTopicTagDiscourse =
        listTopicTag
        |> List.mapi (fun index topicTag ->
            {(discourseTopicTag discourseTopicId discourseTagId topicFromId topicTag) with id = (index + topicTagIdBase)})

    let setPermalinkCategory =
        listCategory
        |> List.map (fun category ->
            discoursePermalinkFromCategoryAndTopic
                (permalinkSourceUrlFromCategory category)
                (discourseCategoryIdOption category.Id)
                None)

    let setPermalinkTopic =
        listTopicLessCategoryDefinition
        |> List.map (fun topic ->
            discoursePermalinkFromCategoryAndTopic
                (permalinkSourceUrlFromTopic topic)
                None
                (Some (discourseTopicId topic.Id)))

    let setPermalinkDiscourseWithId =
        [ setPermalinkCategory; setPermalinkTopic ]
        |> List.concat
        |> List.mapi (fun index permalink ->
            {permalink with id = (index + permalinkIdBase)})

    (
        setUserDiscourse,
        setCategoryDiscourse,
        setTopicDiscourse,
        setPostDiscourse,
        setTagDiscourse,
        setTopicTagDiscourse,
        setPermalinkDiscourseWithId,
        setPostAction
    )

let importFromFileAtPath
    filePath
    =
    let fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read)

    let xmlDocument = new XmlDocument();

    xmlDocument.Load(fileStream)

    let setRecordFromXPathAndRecordConstructor xPath recordConstructor =
        xmlDocument.SelectNodes(xPath).OfType<XmlElement>()
        |> List.ofSeq
        |> List.map recordConstructor

    let listUser =
        setRecordFromXPathAndRecordConstructor userXPath userFromXmlElement
        |> List.sortBy (fun user -> user.CreateDate)

    let listCategory =
        setRecordFromXPathAndRecordConstructor categoryXPath categoryFromXmlElement
        |> List.sortBy (fun category -> category.DateCreated)

    let listTopic =
        setRecordFromXPathAndRecordConstructor topicXPath topicFromXmlElement
        |> List.sortBy (fun topic -> topic.CreateDate)

    let listPost =
        setRecordFromXPathAndRecordConstructor postXPath postFromXmlElement
        |> List.sortBy (fun post -> post.DateCreated)

    let listTag =
        setRecordFromXPathAndRecordConstructor tagXPath tagFromXmlElement

    let listTopicTag =
        setRecordFromXPathAndRecordConstructor topicTagXPath topicTagFromXmlElement

    let listVote =
        setRecordFromXPathAndRecordConstructor voteXPath voteFromXmlElement
        |> List.sortBy (fun vote -> vote.DateVoted)

    (listUser, listCategory, listTopic, listPost, listTag, listTopicTag, listVote)
