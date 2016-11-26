SELECT
    (SELECT * FROM dbo.MembershipUser FOR XML PATH('MembershipUser'), TYPE) AS 'SetMembershipUser',
    (SELECT * FROM dbo.Category FOR XML PATH('Category'), TYPE) AS 'SetCategory',
    (SELECT * FROM dbo.CategoryNotification FOR XML PATH('CategoryNotification'), TYPE) AS 'SetCategoryNotification',
    (SELECT * FROM dbo.Topic FOR XML PATH('Topic'), TYPE) AS 'SetTopic',
    (SELECT * FROM dbo.Post FOR XML PATH('Post'), TYPE) AS 'SetPost',
    (SELECT * FROM dbo.TopicTag FOR XML PATH('TopicTag'), TYPE) AS 'SetTopicTag',
    (SELECT * FROM dbo.Topic_Tag FOR XML PATH('Topic_Tag'), TYPE) AS 'SetTopic_Tag',
    (SELECT * FROM dbo.Vote FOR XML PATH('Vote'), TYPE) AS 'SetVote'
FOR XML PATH(''), ROOT('root')
