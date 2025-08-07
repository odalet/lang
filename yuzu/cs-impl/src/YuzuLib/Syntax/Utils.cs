using System.Globalization;

namespace Yuzu.Syntax;

internal static class Utils
{
    public const char FirstNonAscii = '\x80';
    public const char NextLine = '\x85';
    public const char UnicodeReplacementCharacter = '\uFFFD';

    // NB: \r, \n and the likes are not considered whitespaces here!
    public static bool IsStrictlyWhitespace(char c) =>
        c is ' ' // space
        or '\t' // tab
        or '\v' // vertical tab
        or '\f' // form feed
        or NextLine // next line
        or '\x00A0' // non breaking space
        or '\x1680' // ogham
        or '\x2000' // enQuad
        or '\x2001' // emQuad
        or '\x2002' // enSpace
        or '\x2003' // emSpace
        or '\x2004' // threePerEmSpace
        or '\x2005' // fourPerEmSpace
        or '\x2006' // sixPerEmSpace
        or '\x2007' // figureSpace
        or '\x2008' // punctuationEmSpace
        or '\x2009' // thinSpace
        or '\x200A' // hairSpace
        or '\x200B' // zeroWidthSpace
        or '\x202F' // narrowNoBreakSpace
        or '\x205F' // mathematicalSpace
        or '\x3000' // ideographicSpace
        or '\xFEFF' // BOM
        ;

    public static bool IsLineBreak(char c) =>
        c is '\r' // CR
        or '\n' // LF
        or '\u2028' // Line Separator
        or '\u2029' // Paragraph Separator
        ;

    public static bool IsBinaryDigit(char c) => c is '0' or '1';
    public static bool IsOctalDigit(char c) => c >= '0' && c <= '7';
    public static bool IsDigit(char c) => c >= '0' && c <= '9';
    public static bool IsHexDigit(char c) => c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f';

    public static bool IsIdentifierStart(char c) => IsAsciiLetter(c) || c == '_' || c > FirstNonAscii && c != char.MaxValue && IsUnicodeIdentifierStart(c);
    public static bool IsIdentifierPart(char c) => IsDigit(c) || IsAsciiLetter(c) || c == '_' || c > FirstNonAscii && c != char.MaxValue && IsUnicodeIdentifierPart(c);

    public static bool IsAsciiLetter(char c) => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';


    // See:
    // * https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/lexical-structure#643-identifiers
    // * https://learn.microsoft.com/en-us/dotnet/standard/base-types/character-classes-in-regular-expressions#SupportedUnicodeGeneralCategories
    // NB: we don't support Unicode escape sequences (at least for now)

    private static bool IsUnicodeIdentifierStart(char c) => char.GetUnicodeCategory(c) is
        // L
        UnicodeCategory.UppercaseLetter or
        UnicodeCategory.LowercaseLetter or
        UnicodeCategory.TitlecaseLetter or
        UnicodeCategory.ModifierLetter or
        UnicodeCategory.OtherLetter or
        UnicodeCategory.LetterNumber // Nl
        ;

    private static bool IsUnicodeIdentifierPart(char c) => char.GetUnicodeCategory(c) is
        // L
        UnicodeCategory.UppercaseLetter or
        UnicodeCategory.LowercaseLetter or
        UnicodeCategory.TitlecaseLetter or
        UnicodeCategory.ModifierLetter or
        UnicodeCategory.OtherLetter or
        UnicodeCategory.LetterNumber or // Nl
        UnicodeCategory.DecimalDigitNumber or // Nd
        UnicodeCategory.ConnectorPunctuation or // Pc
        UnicodeCategory.NonSpacingMark or // Mn
        UnicodeCategory.SpacingCombiningMark or // Mc
        UnicodeCategory.Format // Cf
        ;
}
