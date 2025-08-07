using System.Globalization;
using System.Text;
using static Yuzu.Diagnostics;

namespace Yuzu.Syntax;

partial class Scanner
{
    private enum UnicodeEscapeKind { Bits8, Bits16, Bits32 }

    private string ScanCharLiteral()
    {
        state.pos++;
        var result = "";
        var ch = Peek();
        switch (ch)
        {
            case '\r' or '\n' or char.MaxValue:
                state.tokenFlags |= TokenFlags.Unterminated;
                state.pos++;
                Error(UnterminatedCharLiteral);
                return result;
            case '\'':
                Error(EmptyCharLiteral);
                state.pos++;
                return result;
            case '\\':
                result = ScanEscapeSequence(false); // characters are 16-bit only
                break;
            default:
                result = ch.ToString();
                state.pos++;
                break;
        }

        ch = Peek(); // Looking for the termination character: '
        if (ch == '\'') state.pos++;
        else Error(TooManyCharactersInCharLiteral);

        return result;
    }

    private string ScanStringLiteral()
    {
        state.pos++;
        var start = state.pos;
        var builder = new StringBuilder();
        while (true)
        {
            var ch = Peek();
            if (ch is '\r' or '\n' or char.MaxValue)
            {
                builder.Append(text[start..state.pos]);
                state.tokenFlags |= TokenFlags.Unterminated;
                Error(UnterminatedStringLiteral);
                break;
            }

            if (ch == '\\')
            {
                builder.Append(text[start..state.pos]);
                builder.Append(ScanEscapeSequence(true));
                start = state.pos;
                continue;
            }

            if (ch == '"')
            {
                builder.Append(text[start..state.pos]);
                state.pos++;
                break;
            }

            state.pos++;
        }

        return builder.ToString();
    }

    // NB: we differ from C# in how we treat \x:
    // Instead of allowing from1 to 4 digits after \x, we only allow for exactly 2.
    // The rationale behind this is that it's dangerous to have a variable-length escape sequence:
    // Should "\x56ff" be interpreted as "囿" or "Vff"? We choose the latter.
    private string ScanEscapeSequence(bool allowUtf32)
    {
        state.pos++;
        var ch = Peek();
        if (ch == char.MaxValue)
        {
            Error(UnexpectedEndOfText);
            return "";
        }

        state.pos++;
        // Let's apply rules similar to C#'s (except for \x).
        // See https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/strings/#string-escape-sequences
        switch (ch)
        {
            case '\'': return "'";
            case '"': return "\"";
            case '\\': return "\\";
            case '0': return "\0";
            case 'a': return "\a";
            case 'b': return "\b";
            case 'e': return "\e";
            case 'f': return "\f";
            case 'n': return "\n";
            case 'r': return "\r";
            case 't': return "\t";
            case 'v': return "\v";
            case 'x': return ScanUnicodeEscape(UnicodeEscapeKind.Bits8);// \xHH
            case 'u': return ScanUnicodeEscape(UnicodeEscapeKind.Bits16); // \uHHHH
            case 'U':
                if (allowUtf32)
                    return ScanUnicodeEscape(UnicodeEscapeKind.Bits32); // \U00HHHHHH
                break;
        }

        state.tokenFlags |= TokenFlags.ContainsInvalidEscape;
        Error(InvalidEscapeSequence, state.pos - 2, 2);
        return ch.ToString();
    }

    private string ScanUnicodeEscape(UnicodeEscapeKind kind)
    {
        string scanHexDigits(int count)
        {
            static char normalize(char c) => c switch
            {
                'A' => 'a',
                'B' => 'b',
                'C' => 'c',
                'D' => 'd',
                'E' => 'e',
                'F' => 'f',
                _ => c
            };

            var builder = new StringBuilder();
            while (builder.Length < count)
            {
                var ch = Peek();
                if (Utils.IsHexDigit(ch))
                {
                    ch = normalize(ch); // Normalize to lowercase
                    builder.Append(ch);
                }
                else break;

                state.pos++;
            }

            return builder.Length < count ? "" : builder.ToString();
        }

        state.tokenFlags |= TokenFlags.UnicodeEscape;
        var hexDigits = kind switch
        {
            UnicodeEscapeKind.Bits8 => scanHexDigits(2),
            UnicodeEscapeKind.Bits16 => scanHexDigits(4),
            UnicodeEscapeKind.Bits32 => scanHexDigits(8),
            _ => throw new ShouldNotHappenException($"Unsupported {nameof(UnicodeEscapeKind)}: {kind}")
        };

        if (string.IsNullOrEmpty(hexDigits))
        {
            state.tokenFlags |= TokenFlags.ContainsInvalidEscape;
            Error(HexadecimalDigitExpected);
            return "";
        }

        _ = int.TryParse(hexDigits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var hexValue);

        if (kind == UnicodeEscapeKind.Bits32)
        {
            if (hexValue > 0x10FFFF)
            {
                Error(Utf32EscapeSequenceMustBeBetween0And10FFFF);
                return "";
            }
        }
        else if (hexValue > 0xFFFF)
        {
            Error(Utf16EscapeSequenceMustBeBetween0AndFFFF);
            return "";
        }

        return char.ConvertFromUtf32(hexValue);
    }
}
