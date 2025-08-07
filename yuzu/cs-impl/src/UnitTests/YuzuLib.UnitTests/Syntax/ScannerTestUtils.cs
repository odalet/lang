using System;
using System.Collections.Generic;
using Xunit;

namespace Yuzu.Syntax;

internal sealed record ScannerErrorInfo(Diagnostic Diagnostic, int Start, int Length, object[] Args);

internal sealed record TokenInfo(SyntaxKind Token, Scanner Scanner)
{
    public string Value { get; } = Scanner.TokenValue;
    public TokenFlags Flags { get; } = Scanner.TokenFlags;
    public int FullStart { get; } = Scanner.FullStartPosition;
    public int Start { get; } = Scanner.TokenStart;
    public int End { get; } = Scanner.Position;
    public string FullText { get; } = Scanner.Text[Scanner.FullStartPosition..Scanner.Position];
    public string Text { get; } = Scanner.Text[Scanner.TokenStart..Scanner.Position];
}

[Flags]
internal enum AssertCommonFlags
{
    None = 0,
    NoErrors = 1 << 0,
    HasErrors = 2 << 0,
    OK = NoErrors,
}

internal static class ScannerTestUtils
{
    public static (Scanner scanner, List<ScannerErrorInfo> errors, List<TokenInfo> tokens) Execute(string text, bool shouldSkipTrivia = true, int? maxTokens = 200)
    {
        var (scanner, errors) = NewScanner(text, shouldSkipTrivia);
        var (tokens, reachedMaxTokens) = scanner.Run(maxTokens);
        if (reachedMaxTokens) throw new Exception(
            $"Reached {maxTokens!.Value} scanned tokens: this probably indicates an infinite loop");

        return (scanner, errors, tokens);
    }

    public static void AssertCommon(Scanner scanner, IEnumerable<ScannerErrorInfo> errors, AssertCommonFlags flags = AssertCommonFlags.OK)
    {
        Assert.Equal(scanner.Text.Length, scanner.Position);
        if ((flags & AssertCommonFlags.NoErrors) != 0)
            Assert.Empty(errors);
    }

    private static void TestScannerErrorHandler(List<ScannerErrorInfo> errors, Diagnostic diagnostic, int start, int length, object[] args) =>
        errors.Add(new(diagnostic, start, length, args));

    private static (Scanner scanner, List<ScannerErrorInfo> errors) NewScanner(string sourceCode, bool shouldSkipTrivia = true)
    {
        var errors = new List<ScannerErrorInfo>();
        var scanner = new Scanner(sourceCode, (d, s, l, a) => TestScannerErrorHandler(errors, d, s, l, a), shouldSkipTrivia);
        return (scanner, errors);
    }

    private static (List<TokenInfo> tokens, bool reachedMaxTokens) Run(this Scanner scanner, int? maxTokens = null)
    {
        var tokens = new List<TokenInfo>();
        var count = 0;
        var reachedMaxTokens = false;
        while (!reachedMaxTokens)
        {
            var tok = scanner.Next();
            tokens.Add(new(tok, scanner));
            count++;

            if (tok == SyntaxKind.Eof)
                break;

            if (maxTokens.HasValue && count >= maxTokens.Value)
                reachedMaxTokens = true;
        }

        return (tokens, reachedMaxTokens);
    }
}
