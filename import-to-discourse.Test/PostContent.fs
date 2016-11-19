module import_to_discourse.Test.PostContent

open NUnit.Framework
open System.Text.RegularExpressions

let AssertMatchesRegex (input, pattern) =
    let regexMatch = Regex.Match(input, pattern)
    Assert.That(regexMatch.Success, (fun () -> "input " + input + " did not match pattern " + pattern))

[<Test>]
let ``test helper AssertMatchesRegex works`` () =
    Assert.That(
        (fun () -> AssertMatchesRegex("input", "pattern")|> ignore),
        Throws.Exception)
    |> ignore

[<Test>]
let ``code tag to backtick``() = 
    Assert.AreEqual(
        "`code`",
        PostContent.postContentRawFromHtml "<code>code</code>")

[<Test>]
let ``pre tag content to preformatted content``() =
    AssertMatchesRegex(
        (PostContent.postContentRawFromHtml "<pre>preformatted content\nnewline</pre>"),
        @"```\s*preformatted content\nnewline\s*```")

[<Test>]
let ``does not fail at form tag``() =
    AssertMatchesRegex(
        (PostContent.postContentRawFromHtml "<form>content</form>"),
        @"content")
