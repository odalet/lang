using static Yuzu.Diagnostics;

namespace Yuzu.Syntax;

partial class Scanner
{
    // All conflict markers consist of the same character repeated 7 times. If it is
    // a <<<<<<< or >>>>>>> marker then it is also followed by a space.
    private static readonly int mergeConflictMarkerLength = "<<<<<<<".Length;

    // Conflict markers

    private static bool IsConflictMarker(string text, int pos)
    {
        if (pos < 0) throw new ShouldNotHappenException(
            "Assertion failed: pos should be >= 0");

        // Conflict markers must be at the start of a line.
        var prev = pos > 0 ? text[pos - 1] : '\0';
        if (pos == 0 || Utils.IsLineBreak(prev))
        {
            var c = text[pos];
            if ((pos + mergeConflictMarkerLength) < text.Length)
            {
                for (var i = 0; i < mergeConflictMarkerLength; ++i)
                    if (text[pos + i] != c) return false;
            }

            return c == '=' || text[pos + mergeConflictMarkerLength] == ' ';
        }
        return false;
    }

    private int ScanConflictMarker(string text, int pos)
    {
        Error(MergeConflictMarkerEncountered, pos, mergeConflictMarkerLength);
        var c = text[pos];
        var len = text.Length;
        if (c == '<' || c == '>')
        {
            while (pos < len && !Utils.IsLineBreak(c))
            {
                pos++;
                c = text[pos];
            }
        }
        else
        {
            if (c != '=' && c != '|') throw new ShouldNotHappenException(
                "Assertion failed: ch must be either '|' or '='");

            // Consume everything from the start of a ||||||| or ======= marker to the start of the next ======= or >>>>>>> marker.
            while (pos < len)
            {
                var current = text[pos];
                if ((current == '=' || current == '>') && current != c && IsConflictMarker(text, pos))
                    break;
                pos++;
            }
        }

        return pos;
    }
    
    // Comments

    private SyntaxKind ScanComment()
    {
        state.pos++;
        if (Peek() == '/') return ScanSingleLineComment();
        return ScanMultiLineComment(); // NB: the caller makes sure that we are scanning either //... or /*...
    }

    private SyntaxKind ScanSingleLineComment()
    {
        state.pos++;

        // NB: Exactly 3 slashes means Doc Comment
        var kind = Peek() == '/' && Peek(1) != '/' ? SyntaxKind.DocCommentTrivia : SyntaxKind.SingleLineCommentTrivia;

        while (true)
        {
            var c = Peek();
            if (Utils.IsLineBreak(c) || c == char.MaxValue)
                break;
            else state.pos++;
        }

        return kind;
    }

    private SyntaxKind ScanMultiLineComment()
    {
        var start = state.pos - 1;
        state.pos++;
        var openCount = 1; // NB: We do support nested comments
        while (openCount > 0)
        {
            var c = Peek();
            if (c == '/' && Peek(1) == '*') { state.pos += 2; openCount++; }
            else if (c == '*' && Peek(1) == '/') { state.pos += 2; openCount--; }
            else if (c == char.MaxValue)
            {
                Error(UnterminatedComment, start, state.pos - start);
                break;
            }
            else state.pos++;
        }

        return SyntaxKind.MultiLineCommentTrivia;
    }
}
