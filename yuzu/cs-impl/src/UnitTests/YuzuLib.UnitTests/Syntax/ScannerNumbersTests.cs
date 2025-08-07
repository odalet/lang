using System.Diagnostics.CodeAnalysis;
using Xunit;
using static Yuzu.Syntax.ScannerTestUtils;

namespace Yuzu.Syntax;

[ExcludeFromCodeCoverage]
public class ScannerNumbersTests
{
    [Theory]
    [InlineData("0b0", "0")]
    [InlineData("0b1010", "1010")]
    [InlineData("0b_10__10", "_10__10", TokenFlags.ContainsSeparator)]
    [InlineData("0b1010u", "1010", TokenFlags.UIntSuffix)]
    [InlineData("0b1010l", "1010", TokenFlags.LongSuffix)]
    [InlineData("0b1010ul", "1010", TokenFlags.ULongSuffix)]
    public void Binary_number_is_parsed(string text, string expectedValue, TokenFlags additionalExpectedFlags = TokenFlags.None)
    {
        var (scanner, errors, tokens) = Execute(text);

        AssertCommon(scanner, errors);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.NumberLiteral, tokens[0].Token);
        Assert.Equal(TokenFlags.BinarySpecifier | additionalExpectedFlags, tokens[0].Flags);
        Assert.Equal(text, tokens[0].Text);
        Assert.Equal(text, tokens[0].FullText);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("0b", "", "0b", 2)]
    [InlineData("0b234567890", "", "0b", 3)]
    [InlineData("0b101234567890", "101", "0b101", 3)]
    public void Invalid_binary_number_is_detected(string text, string expectedValue, string expectedText, int expectedTokenCount)
    {
        var (scanner, errors, tokens) = Execute(text);

        AssertCommon(scanner, errors, AssertCommonFlags.HasErrors);

        Assert.Single(errors);
        Assert.Equal(Diagnostics.InvalidNumberLiteral, errors[0].Diagnostic);

        Assert.Equal(expectedTokenCount, tokens.Count);
        Assert.Equal(SyntaxKind.NumberLiteral, tokens[0].Token);
        Assert.Equal(TokenFlags.BinarySpecifier, tokens[0].Flags);
        Assert.Equal(expectedText, tokens[0].Text);
        Assert.Equal(expectedText, tokens[0].FullText);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("0o0", "0")]
    [InlineData("0o4567", "4567")]
    [InlineData("0o_45__67", "_45__67", TokenFlags.ContainsSeparator)]
    [InlineData("0o4567u", "4567", TokenFlags.UIntSuffix)]
    [InlineData("0o4567l", "4567", TokenFlags.LongSuffix)]
    [InlineData("0o4567ul", "4567", TokenFlags.ULongSuffix)]
    public void Octal_number_is_parsed(string text, string expectedValue, TokenFlags additionalExpectedFlags = TokenFlags.None)
    {
        var (scanner, errors, tokens) = Execute(text);

        AssertCommon(scanner, errors);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.NumberLiteral, tokens[0].Token);
        Assert.Equal(TokenFlags.OctalSpecifier | additionalExpectedFlags, tokens[0].Flags);
        Assert.Equal(text, tokens[0].Text);
        Assert.Equal(text, tokens[0].FullText);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("0o", "", "0o", 2)]
    [InlineData("0o890", "", "0o", 3)]
    [InlineData("0o777890", "777", "0o777", 3)]
    public void Invalid_octal_number_is_detected(string text, string expectedValue, string expectedText, int expectedTokenCount)
    {
        var (scanner, errors, tokens) = Execute(text);

        AssertCommon(scanner, errors, AssertCommonFlags.HasErrors);

        Assert.Single(errors);
        Assert.Equal(Diagnostics.InvalidNumberLiteral, errors[0].Diagnostic);

        Assert.Equal(expectedTokenCount, tokens.Count);
        Assert.Equal(SyntaxKind.NumberLiteral, tokens[0].Token);
        Assert.Equal(TokenFlags.OctalSpecifier, tokens[0].Flags);
        Assert.Equal(expectedText, tokens[0].Text);
        Assert.Equal(expectedText, tokens[0].FullText);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("0x0", "0")]
    [InlineData("0xABCD", "abcd")]
    [InlineData("0xabcd", "abcd")]
    [InlineData("0x_a__b___c____d", "_a__b___c____d", TokenFlags.ContainsSeparator)]
    [InlineData("0xABCDu", "abcd", TokenFlags.UIntSuffix)]
    [InlineData("0xABCDl", "abcd", TokenFlags.LongSuffix)]
    [InlineData("0xABCDul", "abcd", TokenFlags.ULongSuffix)]
    public void Hexadecimal_number_is_parsed(string text, string expectedValue, TokenFlags additionalExpectedFlags = TokenFlags.None)
    {
        var (scanner, errors, tokens) = Execute(text);

        AssertCommon(scanner, errors);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.NumberLiteral, tokens[0].Token);
        Assert.Equal(TokenFlags.HexSpecifier | additionalExpectedFlags, tokens[0].Flags);
        Assert.Equal(text, tokens[0].Text);
        Assert.Equal(text, tokens[0].FullText);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("0x", "")]
    public void Invalid_hexadecimal_number_is_detected(string text, string expectedValue)
    {
        var (scanner, errors, tokens) = Execute(text);

        AssertCommon(scanner, errors, AssertCommonFlags.HasErrors);
        
        Assert.Single(errors);
        Assert.Equal(Diagnostics.InvalidNumberLiteral, errors[0].Diagnostic);
        
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.NumberLiteral, tokens[0].Token);
        Assert.Equal(TokenFlags.HexSpecifier, tokens[0].Flags);
        Assert.Equal(text, tokens[0].Text);
        Assert.Equal(text, tokens[0].FullText);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("0", "0")]
    [InlineData("1", "1")]
    [InlineData("9", "9")]
    [InlineData("42", "42")]
    [InlineData("42u", "42", TokenFlags.UIntSuffix)]
    [InlineData("42l", "42", TokenFlags.LongSuffix)]
    [InlineData("42ul", "42", TokenFlags.ULongSuffix)]
    [InlineData("0000000000001", "0000000000001")]
    [InlineData("00_00__0000___00001", "00_00__0000___00001", TokenFlags.ContainsSeparator)]
    [InlineData("0.0", "0.0")]
    [InlineData("1.0", "1.0")]
    [InlineData("3.14", "3.14")]
    [InlineData("3.14f", "3.14", TokenFlags.FloatSuffix)]
    [InlineData("3.14d", "3.14", TokenFlags.DoubleSuffix)]
    [InlineData("00.00", "00.00")]
    [InlineData("0____0.0____0", "0____0.0____0", TokenFlags.ContainsSeparator)]
    [InlineData("0.1e2", "0.1E+2")]
    [InlineData("0.1e-2", "0.1E-2")]
    [InlineData("1_0.0_1e2_0", "1_0.0_1E+2_0", TokenFlags.ContainsSeparator)]
    [InlineData("1_0.0_1e-2_0", "1_0.0_1E-2_0", TokenFlags.ContainsSeparator)]
    [InlineData(".0", "0.0")]
    [InlineData(".1", "0.1")]
    [InlineData(".0__0", "0.0__0", TokenFlags.ContainsSeparator)]
    [InlineData(".1e2", "0.1E+2")]
    [InlineData(".1e-2", "0.1E-2")]
    [InlineData(".0_1e2_0", "0.0_1E+2_0", TokenFlags.ContainsSeparator)]
    [InlineData(".0_1e-2_0", "0.0_1E-2_0", TokenFlags.ContainsSeparator)]
    public void Decimal_number_is_parsed(string text, string expectedValue, TokenFlags expectedFlags = TokenFlags.None)
    {
        var (scanner, errors, tokens) = Execute(text);

        AssertCommon(scanner, errors);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.NumberLiteral, tokens[0].Token);
        Assert.Equal(expectedFlags, tokens[0].Flags);
        Assert.Equal(text, tokens[0].Text);
        Assert.Equal(text, tokens[0].FullText);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("1e", "1")]
    [InlineData("1.", "1")]
    [InlineData("1.1e", "1.1")]
    [InlineData("1.e", "1")]
    public void Invalid_decimal_number_is_detected(string text, string expectedValue, TokenFlags additionalExpectedFlags = TokenFlags.None)
    {
        var (scanner, errors, tokens) = Execute(text);

        AssertCommon(scanner, errors, AssertCommonFlags.HasErrors);

        Assert.Single(errors);
        Assert.Equal(Diagnostics.InvalidNumberLiteral, errors[0].Diagnostic);

        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.NumberLiteral, tokens[0].Token);
        Assert.Equal(additionalExpectedFlags, tokens[0].Flags);
        Assert.Equal(text, tokens[0].Text);
        Assert.Equal(text, tokens[0].FullText);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("0b101hello", "101", "0b101")]
    [InlineData("0b101ulhello", "101", "0b101ul")]
    [InlineData("0o777hello", "777", "0o777")]
    [InlineData("0o777ulhello", "777", "0o777ul")]
    [InlineData("0xabcdefhello", "abcdef", "0xabcdef")]
    [InlineData("0xabcdefulhello", "abcdef", "0xabcdeful")]
    [InlineData("1234567890hello", "1234567890", "1234567890")]
    [InlineData("1234567890ulhello", "1234567890", "1234567890ul")]
    public void Identifiers_next_to_number_literal_are_detected(string text, string expectedValue, string expectedText)
    {
        var (scanner, errors, tokens) = Execute(text);

        AssertCommon(scanner, errors, AssertCommonFlags.HasErrors);

        Assert.Single(errors);
        Assert.Equal(Diagnostics.AnIdentifierOrKeywordCannotImmediatelyFollowANumberLiteral, errors[0].Diagnostic);

        Assert.Equal(3, tokens.Count);
        Assert.Equal(SyntaxKind.NumberLiteral, tokens[0].Token);
        Assert.Equal(SyntaxKind.Identifier, tokens[1].Token);
        Assert.Equal(SyntaxKind.Eof, tokens[2].Token);

        Assert.Equal(expectedText, tokens[0].Text);
        Assert.Equal(expectedText, tokens[0].FullText);
        Assert.Equal(expectedValue, tokens[0].Value);
    }
}
