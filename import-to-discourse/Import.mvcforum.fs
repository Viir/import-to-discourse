module Import.mvcforum

open System
open System.Linq
open System.Xml

let userXPath = "//SetMembershipUser/MembershipUser"
let categoryXPath = "//SetCategory/Category"
let topicXPath = "//SetTopic/Topic"
let postXPath = "//SetPost/Post"

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

let discourseUser (listPost : Post list) (user : User)
    : Discourse.DbModel.User.User =
    let isFromThisUser post = post.MembershipUser_Id = user.Id
    let fistPostOption = listPost |> List.tryFind isFromThisUser
    let lastPostOption = listPost |> List.tryFindBack isFromThisUser
    let postOptionDateCreated postOption = match postOption with | None -> None | Some post -> Some post.DateCreated
    {
        id = -1;
        name = user.UserName;
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

let discourseCategory discourseCategoryId (category : Category)
    : Discourse.DbModel.Category.Category =
    {
        id = -1;
        name = category.Name;
        created_at = category.DateCreated;
        updated_at = System.DateTime.MinValue;
        user_id = -1;
        slug = category.Slug;
        description = category.Description;
        parent_category_id = discourseCategoryId category.Category_Id;
    }

let discourseTopic discourseUserId discourseCategoryId (listPost : Post list) (topic : Topic)
    : Discourse.DbModel.Topic.Topic =
    let listPostInTopic = listPost |> List.filter (fun post -> post.Topic_Id = topic.Id)
    let lastPost = listPostInTopic |> List.last

    let postCount = listPostInTopic |> List.length

    {
        id = -1;
        userId = discourseUserId topic.MembershipUser_Id;
        slug = topic.Slug;
        title = topic.Name;
        createdAt = topic.CreateDate;
        lastPostedAt = lastPost.DateCreated;
        updatedAt = DateTime.MinValue;
        viewCount = topic.Views;
        postCount = postCount;
        lastPostUserId = discourseUserId lastPost.MembershipUser_Id;
        replyCount = postCount - 1;
        highestPostNumber = postCount;
        categoryId = discourseCategoryId topic.Category_Id;
        isClosed = 0 < topic.IsLocked;
        archetype = Discourse.DbModel.Topic.Regular;
    }

let discoursePost discourseUserId discourseTopicId discoursePostNumber (post : Post)
    : Discourse.DbModel.Post.Post =
    {
        id = -1;
        user_id = Some (discourseUserId post.MembershipUser_Id);
        topic_id = discourseTopicId post.Topic_Id;
        post_number = discoursePostNumber post;
        raw = "Transform from mvcforum content to markdown: Not implemented yet!!!!";
        cooked = post.PostContent;
        created_at = post.DateCreated;
        updated_at = DateTime.MinValue;
        reply_to_post_number = None;
        deleted_at = None;
        reads = 0;
        last_editor_id = None;
        last_version_at = post.DateEdited;
    }


let transformToDiscourse
    (listUser : User list)
    (listCategory : Category list)
    (listTopic : Topic list)
    (listPost : Post list)
    userIdBase
    categoryIdBase
    topicIdBase
    postIdBase
    =
    let discourseUserId userId =
        (listUser |> List.findIndex (fun user -> user.Id = userId)) + userIdBase

    let discourseCategoryIdOption categoryId =
        listCategory |> List.tryFindIndex (fun category -> category.Id = categoryId)
        |> Option.map (fun id -> id + categoryIdBase)

    let discourseTopicId topicId =
        (listTopic |> List.findIndex (fun topic -> topic.Id = topicId)) + topicIdBase

    let discoursePostId postId =
        (listPost |> List.findIndex (fun post -> post.Id = postId)) + postIdBase
    
    let discoursePostNumber post =
        (listPost |> List.filter (fun otherPost -> otherPost.Topic_Id = post.Topic_Id && otherPost.DateCreated < post.DateCreated)
        |> List.length) + 1

    let setUser =
        listUser
        |> List.map (fun user -> {(discourseUser listPost user) with id = (discourseUserId user.Id)})

    let setCategoryDiscourse =
        listCategory
        |> List.map (fun category -> {(discourseCategory discourseCategoryIdOption category) with id = (discourseCategoryIdOption category.Id).Value})

    let setTopicDiscourse =
        listTopic
        |> List.map (fun topic -> {(discourseTopic discourseUserId discourseCategoryIdOption listPost topic) with id = (discourseTopicId topic.Id)})

    let setPostDiscourse =
        listPost
        |> List.map (fun post -> {(discoursePost discourseUserId discourseTopicId discoursePostNumber post) with id = (discoursePostId post.Id)})

    (setUser, setCategoryDiscourse, setTopicDiscourse, setPostDiscourse)

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

    (listUser, listCategory, listTopic, listPost)
