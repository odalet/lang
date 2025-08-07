namespace Yuzu;

public enum DiagnosticCategory
{
    Error,
    Warning,
    Suggestion,
    Message
}

public record Diagnostic(int Code, DiagnosticCategory Category, string Key, string Text)
{
    public string Format(params object[] args) => string.Format(Text, args);
}

public static class Diagnostics
{
    private static Diagnostic E(int code, string prop, string text) => new(code, DiagnosticCategory.Error, $"{prop}_{code}", text);

    // Scanning Errors
    public static readonly Diagnostic UnterminatedComment = E(1000, nameof(UnterminatedComment), "Unterminated comment.");
    public static readonly Diagnostic UnterminatedCharLiteral = E(1001, nameof(UnterminatedCharLiteral), "Unterminated character literal.");
    public static readonly Diagnostic EmptyCharLiteral = E(1002, nameof(EmptyCharLiteral), "Empty character literal.");
    public static readonly Diagnostic TooManyCharactersInCharLiteral = E(1003, nameof(TooManyCharactersInCharLiteral), "Too many characters in character literal.");
    public static readonly Diagnostic UnterminatedStringLiteral = E(1004, nameof(UnterminatedStringLiteral), "Unterminated string literal.");
    public static readonly Diagnostic InvalidEscapeSequence = E(1005, nameof(InvalidEscapeSequence), "Invalid escape sequence.");
    public static readonly Diagnostic HexadecimalDigitExpected = E(1006, nameof(HexadecimalDigitExpected), "Hexadecimal digit was expected.");
    public static readonly Diagnostic UnexpectedEndOfText = E(1007, nameof(UnexpectedEndOfText), "Unexpected end of text.");
    public static readonly Diagnostic MergeConflictMarkerEncountered = E(1008, nameof(MergeConflictMarkerEncountered), "Merge conflict marker encountered.");
    public static readonly Diagnostic Utf16EscapeSequenceMustBeBetween0AndFFFF = E(1009, nameof(Utf16EscapeSequenceMustBeBetween0AndFFFF), "A UTF-16 escape sequence must be between 0x0 and 0xFFFF inclusive.");
    public static readonly Diagnostic Utf32EscapeSequenceMustBeBetween0And10FFFF = E(1010, nameof(Utf32EscapeSequenceMustBeBetween0And10FFFF), "A UTF-32 escape sequence must be between 0x0 and 0x10FFFF inclusive.");
    public static readonly Diagnostic InvalidNumberLiteral = E(1011, nameof(InvalidNumberLiteral), "Invalid number literal.");
    public static readonly Diagnostic AnIdentifierOrKeywordCannotImmediatelyFollowANumberLiteral = E(1012, nameof(AnIdentifierOrKeywordCannotImmediatelyFollowANumberLiteral), "An identifier or keyword cannot immediately follow a number literal.");
    public static readonly Diagnostic NumericSeparatorsAreNotAllowedHere = E(1013, nameof(NumericSeparatorsAreNotAllowedHere), "Numeric separators are not allowed here.");
    public static readonly Diagnostic InvalidCharacter = E(1013, nameof(InvalidCharacter), "Invalid character.");
    public static readonly Diagnostic FileAppearsToBeBinary = E(1014, nameof(FileAppearsToBeBinary), "File appears to be binary.");
    
    // Parsing Errors
}