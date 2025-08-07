using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using static Yuzu.Diagnostics;

namespace Yuzu.Syntax;

public delegate void ScannerErrorCallback(Diagnostic diagnostic, int start, int length, object[] args);

public sealed partial class Scanner(string input, ScannerErrorCallback errorCallback, bool shouldSkipTrivia = true)
{
    private struct ScannerState
    {
        public int pos; // Current position in text (and ending position of current token)
        public int fullStartPos; // Starting position of current token including preceding whitespace
        public int tokenStart; // Starting position of non-whitespace part of current token
        public SyntaxKind token; // Kind of current token
        public string tokenValue; // Parsed value of current token
        public TokenFlags tokenFlags; // Flags for current token
    }

    private static readonly Dictionary<string, SyntaxKind> keywords;
    private static readonly int minKeywordLength;
    private static readonly int maxKeywordLength;

    private readonly string text = input;
    private readonly ScannerErrorCallback onError = errorCallback;
    private ScannerState state = new();

    public bool SkipTrivia { get; } = shouldSkipTrivia;

    public string Text => text;
    public int Position => state.pos;
    public int FullStartPosition => state.fullStartPos;
    public int TokenStart => state.tokenStart;
    public string TokenValue => state.tokenValue;
    public TokenFlags TokenFlags => state.tokenFlags;

    static Scanner()
    {
        keywords = new()
        {
            ["fun"] = SyntaxKind.FunKeyword,
            ["if"] = SyntaxKind.IfKeyword,
            ["else"] = SyntaxKind.ElseKeyword,
            ["for"] = SyntaxKind.ForKeyword,
            ["val"] = SyntaxKind.ValKeyword,
            ["var"] = SyntaxKind.VarKeyword,
            // Types
            ["string"] = SyntaxKind.StringKeyword,
            ["char"] = SyntaxKind.CharKeyword,
            ["bool"] = SyntaxKind.BoolKeyword,
            ["byte"] = SyntaxKind.ByteKeyword,
            ["sbyte"] = SyntaxKind.SByteKeyword,
            ["short"] = SyntaxKind.ShortKeyword,
            ["ushort"] = SyntaxKind.UShortKeyword,
            ["int"] = SyntaxKind.IntKeyword,
            ["uint"] = SyntaxKind.UIntKeyword,
            ["long"] = SyntaxKind.LongKeyword,
            ["ulong"] = SyntaxKind.ULongKeyword,
            // Add Half?
            ["float"] = SyntaxKind.FloatKeyword,
            ["double"] = SyntaxKind.DoubleKeyword,
            // Add an f128 type?
            ["void"] = SyntaxKind.VoidKeyword,
            ["print"] = SyntaxKind.PrintKeyword // temporary!
        };

        minKeywordLength = keywords.Keys.Select(k => k.Length).Min();
        maxKeywordLength = keywords.Keys.Select(k => k.Length).Max();
    }

    public SyntaxKind Next()
    {
        state.fullStartPos = state.pos;
        state.tokenFlags = TokenFlags.None;
        state.tokenValue = "";

        while (true)
        {
            state.tokenStart = state.pos;
            var ch = Peek();
            switch (ch)
            {
                case '\t' or '\v' or '\f' or ' ':
                    state.pos++;
                    if (SkipTrivia) continue;
                    while (true)
                    {
                        ch = Peek();
                        if (!Utils.IsStrictlyWhitespace(ch)) break;
                        state.pos++;
                    }
                    state.token = SyntaxKind.WhitespaceTrivia;
                    break;
                case '\r' or '\n':
                    state.tokenFlags |= TokenFlags.PrecedingLineBreak;
                    state.pos++;
                    if (ch == '\r' && Peek() == '\n') state.pos++;
                    if (SkipTrivia) continue;
                    state.token = SyntaxKind.NewLineTrivia;
                    break;
                case '!':
                    if (Peek(1) == '=') { state.pos += 2; state.token = SyntaxKind.ExclamationEqualsToken; }
                    else { state.pos++; state.token = SyntaxKind.ExclamationToken; }
                    break;
                case '%':
                    if (Peek(1) == '=') { state.pos += 2; state.token = SyntaxKind.PercentEqualsToken; }
                    else { state.pos++; state.token = SyntaxKind.PercentToken; }
                    break;
                case '^':
                    if (Peek(1) == '=') { state.pos += 2; state.token = SyntaxKind.CaretEqualsToken; }
                    else { state.pos++; state.token = SyntaxKind.CaretToken; }
                    break;
                case '&':
                    if (Peek(1) == '&')
                    {
                        if (Peek(2) == '=') { state.pos += 3; state.token = SyntaxKind.AmpersandAmpersandEqualsToken; }
                        else { state.pos += 2; state.token = SyntaxKind.AmpersandAmpersandToken; }
                    }
                    else if (Peek(1) == '=') { state.pos += 2; state.token = SyntaxKind.AmpersandEqualsToken; }
                    else { state.pos++; state.token = SyntaxKind.AmpersandToken; }
                    break;
                case '(': state.pos++; state.token = SyntaxKind.LParenToken; break;
                case ')': state.pos++; state.token = SyntaxKind.RParenToken; break;
                case '[': state.pos++; state.token = SyntaxKind.LBracketToken; break;
                case ']': state.pos++; state.token = SyntaxKind.RBracketToken; break;
                case '{': state.pos++; state.token = SyntaxKind.LBraceToken; break;
                case '}': state.pos++; state.token = SyntaxKind.RBraceToken; break;
                case ':': state.pos++; state.token = SyntaxKind.ColonToken; break;
                case ',': state.pos++; state.token = SyntaxKind.CommaToken; break;
                case ';': state.pos++; state.token = SyntaxKind.SemicolonToken; break;
                case '@': state.pos++; state.token = SyntaxKind.AtToken; break;
                case '~': state.pos++; state.token = SyntaxKind.TildeToken; break;
                case '#': state.pos++; state.token = SyntaxKind.SharpToken; break;
                case '.':
                    if (Utils.IsDigit(Peek(1))) state.token = ScanNumber(); // .123 == 0.123
                    else if (Peek(1) == '.') { state.pos += 2; state.token = SyntaxKind.DotDotToken; }
                    else { state.pos++; state.token = SyntaxKind.DotToken; }
                    break;
                case '<':
                    if (IsConflictMarker(text, state.pos))
                    {
                        state.pos = ScanConflictMarker(text, state.pos);
                        if (SkipTrivia) continue;
                        state.token = SyntaxKind.ConflictMarkerTrivia;
                        break;
                    }

                    if (Peek(1) == '<')
                    {
                        if (Peek(2) == '=') { state.pos += 3; state.token = SyntaxKind.LessThanLessThanEqualsToken; }
                        else { state.pos += 2; state.token = SyntaxKind.LessThanLessThanToken; }
                    }
                    else if (Peek(1) == '=') { state.pos += 2; state.token = SyntaxKind.LessThanEqualsToken; }
                    else { state.pos++; state.token = SyntaxKind.LessThanToken; }
                    break;
                case '>':
                    if (IsConflictMarker(text, state.pos))
                    {
                        state.pos = ScanConflictMarker(text, state.pos);
                        if (SkipTrivia) continue;
                        state.token = SyntaxKind.ConflictMarkerTrivia;
                        break;
                    }

                    if (Peek(1) == '>')
                    {
                        if (Peek(2) == '=') { state.pos += 3; state.token = SyntaxKind.GreaterThanGreaterThanEqualsToken; }
                        else { state.pos += 2; state.token = SyntaxKind.GreaterThanGreaterThanToken; }
                    }
                    else if (Peek(1) == '=') { state.pos += 2; state.token = SyntaxKind.GreaterThanEqualsToken; }
                    else { state.pos++; state.token = SyntaxKind.GreaterThanToken; }
                    break;
                case '=':
                    if (IsConflictMarker(text, state.pos))
                    {
                        state.pos = ScanConflictMarker(text, state.pos);
                        if (SkipTrivia) continue;
                        state.token = SyntaxKind.ConflictMarkerTrivia;
                        break;
                    }

                    if (Peek(1) == '=') { state.pos += 2; state.token = SyntaxKind.EqualsEqualsToken; }
                    else if (Peek(1) == '>') { state.pos += 2; state.token = SyntaxKind.EqualsGreaterThanToken; }
                    else { state.pos++; state.token = SyntaxKind.EqualsToken; }
                    break;
                case '|':
                    if (IsConflictMarker(text, state.pos))
                    {
                        state.pos = ScanConflictMarker(text, state.pos);
                        if (SkipTrivia) continue;
                        state.token = SyntaxKind.ConflictMarkerTrivia;
                        break;
                    }

                    if (Peek(1) == '|')
                    {
                        if (Peek(2) == '=') { state.pos += 3; state.token = SyntaxKind.PipePipeEqualsToken; }
                        else { state.pos += 2; state.token = SyntaxKind.PipePipeToken; }
                    }
                    else if (Peek(1) == '=') { state.pos += 2; state.token = SyntaxKind.PipeEqualsToken; }
                    else { state.pos++; state.token = SyntaxKind.PipeToken; }
                    break;
                case '?':
                    if (Peek(1) == '?')
                    {
                        if (Peek(2) == '=') { state.pos += 3; state.token = SyntaxKind.QuestionQuestionEqualsToken; }
                        else { state.pos += 2; state.token = SyntaxKind.QuestionQuestionToken; }
                    }
                    else if (Peek(1) == '.') { state.pos += 2; state.token = SyntaxKind.QuestionDotToken; }
                    else { state.pos++; state.token = SyntaxKind.QuestionToken; }
                    break;
                case '+':
                    if (Peek(1) == '+') { state.pos += 2; state.token = SyntaxKind.PlusPlusToken; }
                    else if (Peek(1) == '=') { state.pos += 2; state.token = SyntaxKind.PlusEqualsToken; }
                    else { state.pos++; state.token = SyntaxKind.PlusToken; }
                    break;
                case '-':
                    if (Peek(1) == '-') { state.pos += 2; state.token = SyntaxKind.MinusMinusToken; }
                    else if (Peek(1) == '=') { state.pos += 2; state.token = SyntaxKind.MinusEqualsToken; }
                    else { state.pos++; state.token = SyntaxKind.MinusToken; }
                    break;
                case '*':
                    if (Peek(1) == '=') { state.pos += 2; state.token = SyntaxKind.StarEqualsToken; }
                    else { state.pos++; state.token = SyntaxKind.StarToken; }
                    break;
                case '/':
                    if (Peek(1) is '/' or '*')
                    {
                        var token = ScanComment();
                        if (SkipTrivia) continue;
                        state.token = token;
                    }
                    else if (Peek(1) == '=') { state.pos += 2; state.token = SyntaxKind.SlashEqualsToken; }
                    else { state.pos++; state.token = SyntaxKind.SlashToken; }
                    break;
                case '\'':
                    state.tokenValue = ScanCharLiteral();
                    state.token = SyntaxKind.CharLiteral;
                    break;
                case '"':
                    // TODO: interpolated strings & multi-line strings
                    state.tokenValue = ScanStringLiteral();
                    state.token = SyntaxKind.StringLiteral;
                    break;
                case '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9':
                    state.token = ScanNumber();
                    break;
                case char.MaxValue:
                    state.token = SyntaxKind.Eof;
                    break;
                case Utils.UnicodeReplacementCharacter:
                    Error(Diagnostics.FileAppearsToBeBinary, 0, 0);
                    state.token = SyntaxKind.NonTextFileMarkerTrivia;
                    state.pos = text.Length;
                    break;
                default:
                    if (ScanIdentifier())
                    {
                        state.token = GetIdentifierToken(state.tokenValue);
                        break;
                    }

                    if (Utils.IsStrictlyWhitespace(ch))
                    {
                        if (ch == Utils.NextLine || SkipTrivia)
                            continue;

                        while (Utils.IsStrictlyWhitespace(ch))
                            state.pos++;

                        state.token = SyntaxKind.WhitespaceTrivia;
                        break;
                    }

                    if (Utils.IsLineBreak(ch))
                    {
                        state.tokenFlags |= TokenFlags.PrecedingLineBreak;
                        state.pos++;
                        if (SkipTrivia) continue;
                        state.token = SyntaxKind.NewLineTrivia;
                        break;
                    }

                    state.token = ScanInvalidCharacter();
                    break;
            }

            return state.token;
        }
    }

    private bool ScanIdentifier()
    {
        var start = state.pos;

        // ASCII: Fast path
        var ch = Peek();
        if (Utils.IsAsciiLetter(ch) || ch == '_')
        {
            while (true)
            {
                state.pos++;
                ch = Peek();
                if (!Utils.IsAsciiLetter(ch) && !Utils.IsDigit(ch) && ch != '_')
                    break;
            }

            if (ch == char.MaxValue || ch < Utils.FirstNonAscii)
            {
                state.tokenValue = text[start..state.pos];
                return true;
            }

            state.pos = start; // reset
        }

        // Normal path
        ch = Peek();
        if (Utils.IsIdentifierStart(ch))
        {
            while (true)
            {
                state.pos++;
                ch = Peek();
                if (!Utils.IsIdentifierPart(ch))
                    break;
            }

            state.tokenValue = text[start..state.pos];
            return true;
        }

        return false;
    }

    private static SyntaxKind GetIdentifierToken(string value)
    {
        var len = value.Length;
        if (len >= minKeywordLength && len <= maxKeywordLength &&
            value[0] >= 'a' && value[0] <= 'z' &&
            keywords.TryGetValue(value, out var keyword)) 
            return keyword;

        return SyntaxKind.Identifier;
    }

    private SyntaxKind ScanInvalidCharacter()
    {
        Error(InvalidCharacter, state.pos, 1);
        state.pos++;
        return SyntaxKind.Unknown;
    }

    private char Peek(int offset = 0) => state.pos + offset < text.Length ? text[state.pos + offset] : char.MaxValue;

    private void Error(Diagnostic diagnostic, params object[] args) => Error(diagnostic, state.pos, 0, args);
    private void Error(Diagnostic diagnostic, int pos, int length, params object[] args) => onError?.Invoke(diagnostic, pos, length, args);
}
