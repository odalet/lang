using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Yuzu.Syntax;

namespace Yuzu;

internal static class Program
{
    [SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "Test Code")]
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        var skipTrivia = true;

        // To Test:
        // '\u56ff'     -> '囿'
        // "\U000056ff" -> "囿"
        // "\U0001f47d" -> "👽"

        //var text = "\"String\", 'c', { }   0x0000001234; 0b010102x";
        var text = "'c' '\\x56' '\\u56ff'";
        //var text = "'\\x56'";

        Console.WriteLine($"Scanning \"{text}\"; Skip Triva: {skipTrivia}");
        Console.WriteLine("-------------------");

        var scanner = new Scanner(text, HandleError, skipTrivia);
        var count = 0;
        var max = 20;
        while (true)
        {
            var tok = scanner.Next();
            Dump(tok, scanner);
            count++;
            Console.WriteLine("-------------------");
            if (tok == SyntaxKind.Eof || count >= max)
                break;
        }
    }

    private static void Dump(SyntaxKind tok, Scanner scanner)
    {
        Console.WriteLine($"{tok}: [{scanner.FullStartPosition}, {scanner.TokenStart} -> {scanner.Position}]");
        if (!string.IsNullOrEmpty(scanner.TokenValue))
            Console.WriteLine($"=> [<{scanner.TokenValue}>]");
        if (scanner.TokenFlags != TokenFlags.None)
            Console.WriteLine($"=> {scanner.TokenFlags}");
    }

    private static void HandleError(Diagnostic diagnostic, int start, int length, object[] args)
    {
        string makeCode()
        {
            var cat = diagnostic.Category switch
            {
                DiagnosticCategory.Error => "E",
                DiagnosticCategory.Warning => "W",
                DiagnosticCategory.Message => "M",
                DiagnosticCategory.Suggestion => "S",
                _ => "X"
            };

            return $"{cat}{diagnostic.Code}";
        }

        string makeLoc() => length == 0 ? start.ToString() : $"{start}..{start + length}";

        Console.WriteLine($"{makeCode()} @{makeLoc()} - {string.Format(diagnostic.Text, args)}");
    }
}
