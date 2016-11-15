module Import.mvcforum

open System
open System.Linq
open System.Xml

let userXPath = "//SetMembershipUser/MembershipUser"
let categoryXPath = "//SetCategory/Category"

type User = 
    {
        Id : string;
        UserName : string;
        Email : string;
        CreateDate : System.DateTime;
        LastLoginDate : System.DateTime Option;
        LastActivityDate : System.DateTime Option;
        Slug : string;
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


let import filePath =
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

    let setCategory =
        setRecordFromXPathAndRecordConstructor categoryXPath categoryFromXmlElement

    (listUser, setCategory)
