module PostContent

let postContentRawFromHtml html =
    match html with
    | null -> null
    | _ ->
        let converter = new ReverseMarkdown.Converter(new ReverseMarkdown.Config("pass_through", true))
        converter.Convert(html)
