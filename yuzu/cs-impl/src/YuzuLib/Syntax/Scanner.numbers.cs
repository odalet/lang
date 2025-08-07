using System;
using System.Text;
using static Yuzu.Diagnostics;

namespace Yuzu.Syntax;

partial class Scanner
{
    private SyntaxKind ScanNumber()
    {
        if (Peek() == '0')
        {
            var next = Peek(1);
            if (next is 'b' or 'B') { state.pos += 2; return ScanInteger(true, TokenFlags.BinarySpecifier, Utils.IsBinaryDigit); }
            if (next is 'o' or 'O') { state.pos += 2; return ScanInteger(true, TokenFlags.OctalSpecifier, Utils.IsOctalDigit); }
            if (next is 'x' or 'X') { state.pos += 2; return ScanInteger(true, TokenFlags.HexSpecifier, Utils.IsHexDigit); }
        }

        // Decimal; either integer or floating number though
        var start = state.pos;
        var startsWithDot = Peek() == '.';
        var integerPart = startsWithDot ? "0" : ScanNumberFragment(false); // This handles .123 syntax
        var isInteger = false;
        if (!startsWithDot)
        {
            // Let's look for integer suffixes
            var ch1 = Peek();
            switch (ch1)
            {
                case 'u' or 'U':
                    if (Peek(1) is 'l' or 'L') { state.pos += 2; state.tokenFlags |= TokenFlags.ULongSuffix; }
                    else { state.pos++; state.tokenFlags |= TokenFlags.UIntSuffix; }
                    isInteger = true;
                    break;
                case 'l' or 'L': state.pos++; state.tokenFlags |= TokenFlags.LongSuffix;
                    isInteger = true;
                    break;
            }
        }

        if (isInteger)
        {
            state.tokenValue = integerPart;

            if (Utils.IsIdentifierStart(Peek()))
                Error(AnIdentifierOrKeywordCannotImmediatelyFollowANumberLiteral, state.pos + 1, 1);

            return SyntaxKind.NumberLiteral;
        }

        var fractionalPart = "";
        var exponentSign = "";
        var exponentPart = "";

        var invalidNumber = false;
        if (Peek() == '.')
        {
            state.pos++;
            fractionalPart = ScanNumberFragment(false);
            if (string.IsNullOrEmpty(fractionalPart))
            {
                invalidNumber = true;
                Error(InvalidNumberLiteral, start, state.pos);
            }
        }

        if (Peek() is 'e' or 'E')
        {
            state.pos++;
            if (Peek() is '+' or '-')
            {
                exponentSign = Peek() == '+' ? "+" : "-";
                state.pos++;
            }
            else exponentSign = "+"; // Explicit +
            exponentPart = ScanNumberFragment(false);
            if (string.IsNullOrEmpty(exponentPart) && !invalidNumber) // No need for twice the error
                Error(InvalidNumberLiteral, start, state.pos);
        }

        var ch2 = Peek();
        switch (ch2)
        {
            case 'f' or 'F': state.pos++; state.tokenFlags |= TokenFlags.FloatSuffix; break;
            case 'd' or 'D': state.pos++; state.tokenFlags |= TokenFlags.DoubleSuffix; break;
        }

        var builder = new StringBuilder(integerPart);
        if (!string.IsNullOrEmpty(fractionalPart))
            builder.Append('.').Append(fractionalPart);
        if (!string.IsNullOrEmpty(exponentPart))
            builder.Append('E').Append(exponentSign).Append(exponentPart);

        state.tokenValue = builder.ToString();

        if (Utils.IsIdentifierStart(Peek()))
            Error(AnIdentifierOrKeywordCannotImmediatelyFollowANumberLiteral, state.pos + 1, 1);

        return SyntaxKind.NumberLiteral;
    }

    private SyntaxKind ScanInteger(bool allowInitialSeparator, TokenFlags tokenFlags, Func<char, bool>? isDigit = null)
    {
        var start = state.pos;
        state.tokenFlags |= tokenFlags;

        var fragment = ScanNumberFragment(allowInitialSeparator, isDigit);
        if (string.IsNullOrEmpty(fragment))
        {
            Error(InvalidNumberLiteral, state.pos - 2, 2);
            return SyntaxKind.NumberLiteral;
        }

        fragment = fragment.ToLowerInvariant(); // Normalize ABCDEF -> abcdef
        var ch = Peek();
        switch (ch)
        {
            case 'u' or 'U':
                if (Peek(1) is 'l' or 'L') { state.pos += 2; state.tokenFlags |= TokenFlags.ULongSuffix; }
                else { state.pos++; state.tokenFlags |= TokenFlags.UIntSuffix; }
                break;
            case 'l' or 'L': state.pos++; state.tokenFlags |= TokenFlags.LongSuffix; break;
        }

        if (Utils.IsDigit(ch)) // We have a digit that doess not belong to the base
            Error(InvalidNumberLiteral, start, state.pos);

        if (Utils.IsIdentifierStart(Peek()))
            Error(AnIdentifierOrKeywordCannotImmediatelyFollowANumberLiteral, state.pos + 1, 1);

        state.tokenValue = fragment;

        return SyntaxKind.NumberLiteral;
    }

    private string ScanNumberFragment(bool allowInitialSeparator, Func<char, bool>? isDigit = null)
    {
        isDigit ??= Utils.IsDigit;

        var lastNumberPos = state.pos;
        var endsWithSeparator = false;
        var allowSeparator = allowInitialSeparator;
        var builder = new StringBuilder();
        while (true)
        {
            var ch = Peek();
            if (ch == '_')
            {
                state.tokenFlags |= TokenFlags.ContainsSeparator;
                if (allowSeparator) builder.Append(ch);
                else Error(NumericSeparatorsAreNotAllowedHere, state.pos, 1);
                state.pos++;
                endsWithSeparator = true;
                continue;
            }

            if (isDigit(ch))
            {
                allowSeparator = true;
                builder.Append(ch);
                state.pos++;
                lastNumberPos = state.pos;
                endsWithSeparator = false;
                continue;
            }

            break;
        }

        if (endsWithSeparator)
            Error(NumericSeparatorsAreNotAllowedHere, lastNumberPos + 1, 1);

        return builder.ToString();
    }
}