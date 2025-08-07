using System;

namespace Yuzu.Syntax;

[Flags]
public enum TokenFlags
{
    None = 0,
    PrecedingLineBreak = 1 << 0,
    Unterminated = 1 << 1, // e.g. "... or '...
    // Numbers
    Scientific = 1 << 2, // e.g. `10e2`
    HexSpecifier = 1 << 3, // e.g. `0x00000000`
    BinarySpecifier = 1 << 4, // e.g. `0b0110010000000000`
    OctalSpecifier = 1 << 5, // e.g. `0o777`
    ContainsSeparator = 1 << 6, // e.g. `0b1100_0101`
    UIntSuffix = 1 << 7, // e.g. 123u or 123U
    LongSuffix = 1 << 8, // e.g. 123l or 123L
    ULongSuffix = 1 << 9, // e.g. 123ul or 123uL or 123Ul or 123UL
    FloatSuffix = 1 << 10, // e.g. 3.14f or 3.14F
    DoubleSuffix = 1 << 11, // e.g. 3.14d or 3.14D
    // Chars ans strings
    UnicodeEscape = 1 << 12, // e.g. `\u00a0` or \U or \x
    ContainsInvalidEscape = 1 << 13, // e.g. `\uhello`

    // Combined flags
    //BinaryOrOctalSpecifier = BinarySpecifier | OctalSpecifier,
    //WithSpecifier = HexSpecifier | BinaryOrOctalSpecifier,
    //StringLiteralFlags = Unterminated | HexEscape | UnicodeEscape | ExtendedUnicodeEscape | ContainsInvalidEscape | SingleQuote,
    //NumericLiteralFlags = Scientific | ContainsLeadingZero | WithSpecifier | ContainsSeparator | ContainsInvalidSeparator,
    //TemplateLiteralLikeFlags = Unterminated | HexEscape | UnicodeEscape | ExtendedUnicodeEscape | ContainsInvalidEscape,
    //IsInvalid = ContainsLeadingZero | ContainsInvalidSeparator | ContainsInvalidEscape
}
