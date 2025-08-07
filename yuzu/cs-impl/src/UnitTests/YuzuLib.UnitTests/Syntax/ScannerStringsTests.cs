using System.Diagnostics.CodeAnalysis;
using Xunit;
using static Yuzu.Syntax.ScannerTestUtils;

namespace Yuzu.Syntax;

[ExcludeFromCodeCoverage]
public class ScannerStringsTests
{
    // NB: \x in C# can be followed by 1 to 4 digits. In Yuzu, we only allow strictly 2 digits
    [Theory]
    [InlineData("\"\"", "")]
    [InlineData("\"Abcd Efgh\"", "Abcd Efgh")]
    [InlineData("\"Abcd \\\\ Efgh\"", "Abcd \\ Efgh")]
    [InlineData("\"<-- \\' \\\" \\\\ \\0 \\a \\b \\e \\f \\n \\r \\t \\v -->\"", "<-- ' \" \\ \0 \a \b \u001b \f \n \r \t \v -->")]
    [InlineData("\"\\x56\"", "V", TokenFlags.UnicodeEscape)]
    [InlineData("\"\\u0056\"", "V", TokenFlags.UnicodeEscape)]
    [InlineData("\"\\U00000056\"", "V", TokenFlags.UnicodeEscape)]
    [InlineData("\"\\x56ff\"", "Vff", TokenFlags.UnicodeEscape)] // NB: only the 2 first digits are part of the escape sequence!
    [InlineData("\"\\u56ff\"", "囿", TokenFlags.UnicodeEscape)]
    [InlineData("\"\\U000056ff\"", "囿", TokenFlags.UnicodeEscape)]
    [InlineData("\"\\U0001f47d\"", "👽", TokenFlags.UnicodeEscape)]
    public void Strings_are_parsed(string text, string expectedValue, TokenFlags expectedFlags = TokenFlags.None)
    {
        var (scanner, errors, tokens) = Execute(text);

        AssertCommon(scanner, errors);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.StringLiteral, tokens[0].Token);
        Assert.Equal(expectedFlags, tokens[0].Flags);
        Assert.Equal(text, tokens[0].Text);
        Assert.Equal(text, tokens[0].FullText);
        Assert.Equal(expectedValue, tokens[0].Value);
    }
}
