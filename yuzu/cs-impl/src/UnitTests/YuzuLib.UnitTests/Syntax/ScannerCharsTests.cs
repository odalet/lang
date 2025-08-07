using System.Diagnostics.CodeAnalysis;
using Xunit;
using static Yuzu.Syntax.ScannerTestUtils;

namespace Yuzu.Syntax;

[ExcludeFromCodeCoverage]
public class ScannerCharsTests
{
    // NB: \x in C# can be followed by 1 to 4 digits. In Yuzu, we only allow strictly 2 digits
    [Theory]
    //[InlineData("\"\"", "")]
    [InlineData("'A'", "A")]
    [InlineData("'\\''", "'")]
    [InlineData("'\\\"'", "\"")]
    [InlineData("'\\0'", "\0")]
    [InlineData("'\\a'", "\a")]
    [InlineData("'\\b'", "\b")]
    [InlineData("'\\e'", "\e")]
    [InlineData("'\\f'", "\f")]
    [InlineData("'\\n'", "\n")]
    [InlineData("'\\r'", "\r")]
    [InlineData("'\\t'", "\t")]
    [InlineData("'\\v'", "\v")]
    [InlineData("'\\x56'", "V", TokenFlags.UnicodeEscape)]
    [InlineData("'\\u0056'", "V", TokenFlags.UnicodeEscape)]
    [InlineData("'\\u56ff'", "囿", TokenFlags.UnicodeEscape)]
    public void Characters_are_parsed(string text, string expectedValue, TokenFlags expectedFlags = TokenFlags.None)
    {
        var (scanner, errors, tokens) = Execute(text);

        AssertCommon(scanner, errors);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.CharLiteral, tokens[0].Token);
        Assert.Equal(expectedFlags, tokens[0].Flags);
        Assert.Equal(text, tokens[0].Text);
        Assert.Equal(text, tokens[0].FullText);
        Assert.Equal(expectedValue, tokens[0].Value);
    }
}
