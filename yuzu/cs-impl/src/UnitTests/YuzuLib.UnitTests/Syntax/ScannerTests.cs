using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Yuzu.Syntax.ScannerTestUtils;

namespace Yuzu.Syntax;

[ExcludeFromCodeCoverage]
public class ScannerTests
{
    [Fact]
    public void Empty_source_returns_eof()
    {
        var (scanner, errors, tokens) = Execute("");

        AssertCommon(scanner, errors);
        Assert.Single(tokens);
        Assert.Equal(SyntaxKind.Eof, tokens[0].Token);
    }

    // New lines

    [Theory]
    [InlineData("3.14 6.28", false)]
    [InlineData("3.14\n6.28", true)]
    [InlineData("3.14\r\n6.28", true)]
    public void Newlines_are_including_in_the_following_token_info_when_ignoring_trivia(string text, bool hasTokenInfo)
    {
        var (scanner, errors, tokens) = Execute(text, shouldSkipTrivia: true);

        AssertCommon(scanner, errors);
        Assert.Equal(3, tokens.Count);

        // Still, we know there was a line break
        Assert.Equal(
            hasTokenInfo ? TokenFlags.PrecedingLineBreak : TokenFlags.None, tokens[1].Flags);
    }

    [Theory]
    [InlineData("3.14 6.28", 4)]
    [InlineData("3.14\n6.28", 4)]
    [InlineData("3.14\r\n6.28", 4)]
    public void Newlines_are_proper_tokens_when_scanning_trivia(string text, int expectedTokenCount)
    {
        var (scanner, errors, tokens) = Execute(text, shouldSkipTrivia: false);

        AssertCommon(scanner, errors);
        Assert.Equal(expectedTokenCount, tokens.Count);
    }

    // Spaces

    [Fact]
    public void Leading_spaces_are_included_in_the_returned_token_info()
    {
        var (scanner, errors, tokens) = Execute(" \t @ \v ");

        AssertCommon(scanner, errors);
        Assert.Equal(2, tokens.Count);

        // #0: ...@
        Assert.Equal(SyntaxKind.AtToken, tokens[0].Token);
        Assert.Equal("", tokens[0].Value);
        Assert.Equal("@", tokens[0].Text);
        Assert.Equal(" \t @", tokens[0].FullText);
        Assert.Equal(0, tokens[0].FullStart);
        Assert.Equal(3, tokens[0].Start);

        // #1: ...EOF
        Assert.Equal(SyntaxKind.Eof, tokens[1].Token);
        Assert.Equal("", tokens[1].Value);
        Assert.Equal("", tokens[1].Text);
        Assert.Equal(" \v ", tokens[1].FullText);
        Assert.Equal(4, tokens[1].FullStart);
        Assert.Equal(7, tokens[1].Start);
    }

    [Fact]
    public void Spaces_are_proper_tokens_when_scanning_trivia()
    {
        var (scanner, errors, tokens) = Execute(" \t @ \v ", shouldSkipTrivia: false);

        AssertCommon(scanner, errors);
        Assert.Equal(4, tokens.Count);
        // #0: ...
        Assert.Equal(SyntaxKind.WhitespaceTrivia, tokens[0].Token);
        Assert.Equal("", tokens[0].Value);
        Assert.Equal(" \t ", tokens[0].Text);
        Assert.Equal(" \t ", tokens[0].FullText);
        Assert.Equal(0, tokens[0].FullStart);
        Assert.Equal(0, tokens[0].Start);

        // #1: @
        Assert.Equal(SyntaxKind.AtToken, tokens[1].Token);
        Assert.Equal("", tokens[1].Value);
        Assert.Equal("@", tokens[1].Text);
        Assert.Equal("@", tokens[1].FullText);
        Assert.Equal(3, tokens[1].FullStart);
        Assert.Equal(3, tokens[1].Start);

        // #2: ...
        Assert.Equal(SyntaxKind.WhitespaceTrivia, tokens[2].Token);
        Assert.Equal("", tokens[2].Value);
        Assert.Equal(" \v ", tokens[2].Text);
        Assert.Equal(" \v ", tokens[2].FullText);
        Assert.Equal(4, tokens[2].FullStart);
        Assert.Equal(4, tokens[2].Start);

        // #3: ...EOF
        Assert.Equal(SyntaxKind.Eof, tokens[3].Token);
        Assert.Equal("", tokens[3].Value);
        Assert.Equal("", tokens[3].Text);
        Assert.Equal("", tokens[3].FullText);
        Assert.Equal(7, tokens[3].FullStart);
        Assert.Equal(7, tokens[3].Start);
    }

    // Comments

    [Theory]
    [InlineData("3.14 // comment")]
    [InlineData("3.14 /// doc-comment")]
    [InlineData("3.14 // comment\n")]
    [InlineData("3.14 /// doc-comment\n")]
    [InlineData("3.14 // comment\r\n")]
    [InlineData("3.14 /// doc-comment\r\n")]
    [InlineData("3.14 /* multi-line \r\n comment\n */")]
    [InlineData("3.14 /* /*/* nested multi-line comment */  \r\n\n\n      */*/")]
    public void Comments_are_ignored_when_ignoring_trivia(string text)
    {
        var (scanner, errors, tokens) = Execute(text, shouldSkipTrivia: true);

        AssertCommon(scanner, errors);
        Assert.Equal(2, tokens.Count);
    }

    [Theory]
    [InlineData("3.14 // comment", SyntaxKind.SingleLineCommentTrivia, 4)]
    [InlineData("3.14 /// doc-comment", SyntaxKind.DocCommentTrivia, 4)]
    [InlineData("3.14 // comment\n", SyntaxKind.SingleLineCommentTrivia, 5)]
    [InlineData("3.14 /// doc-comment\n", SyntaxKind.DocCommentTrivia, 5)]
    [InlineData("3.14 // comment\r\n", SyntaxKind.SingleLineCommentTrivia, 5)]
    [InlineData("3.14 /// doc-comment\r\n", SyntaxKind.DocCommentTrivia, 5)]
    [InlineData("3.14 /* multi-line \r\n comment\n */", SyntaxKind.MultiLineCommentTrivia, 4)]
    [InlineData("3.14 /* /*/* nested multi-line comment */  \r\n\n\n      */*/", SyntaxKind.MultiLineCommentTrivia, 4)]
    public void Comments_are_correctly_categorized(string text, SyntaxKind expectedToken, int expectedTokenCount)
    {
        var (scanner, errors, tokens) = Execute(text, shouldSkipTrivia: false);

        AssertCommon(scanner, errors);
        Assert.Equal(expectedTokenCount, tokens.Count);
        Assert.Equal(expectedToken, tokens[2].Token); // #0 is 3.14, #1 is space
    }

    [Theory]
    [InlineData("/*")]
    [InlineData("/* multi-line")]
    [InlineData("/* multi-line \r\n comment\n")]
    [InlineData("/* /*/* nested \r\n multi-line comment */  \r\n\n\n      */")]
    [InlineData("/* /*/* nested \r\n multi-line comment")]
    public void Unterminated_comments_are_detected(string text)
    {
        void executeUnterminatedCommentsTests(bool shouldSkipTrivia)
        {
            var (scanner, errors, tokens) = Execute(text, shouldSkipTrivia);

            AssertCommon(scanner, errors, AssertCommonFlags.HasErrors);
            Assert.Equal(shouldSkipTrivia ? 1 : 2, tokens.Count);
            Assert.Single(errors);
            Assert.Equal(Diagnostics.UnterminatedComment, errors[0].Diagnostic);
        }

        executeUnterminatedCommentsTests(true);
        executeUnterminatedCommentsTests(false);
    }
}
