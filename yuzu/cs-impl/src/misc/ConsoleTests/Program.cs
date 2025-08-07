using System;
using System.Text;

namespace ConsoleTests;

// See https://stackoverflow.com/questions/67508469/how-to-show-emoji-in-c-sharp-console-output
internal static class Program
{
    private static void Main()
    {
        const int emojisPerLine = 10;
        var emojiRanges = new (int begin, int end)[]
        {
            (0x1F300, 0x1F5FF), (0x1F600, 0x1F64F), (0x1F680, 0x1F6FF), (0x1F900, 0x1F9FF), (0x1FA70, 0x1FAFF)
        };

        var count = 0;
        var sb = new StringBuilder();
        Console.OutputEncoding = Encoding.UTF8;
        foreach (var (begin, end) in emojiRanges)
        {
            var codePoint = 0;
            for (codePoint = begin; codePoint <= end; codePoint++)
            {
                var emoji = char.ConvertFromUtf32(codePoint);
                sb.Append(emoji);
                count++;
                if (count % emojisPerLine == 0)
                {
                    Console.WriteLine($"{sb} {codePoint - 9:X}- {codePoint:X}");
                    sb.Clear();
                }
            }

            Console.WriteLine(sb.ToString() + " " + (codePoint - 9).ToString("X") + (codePoint.ToString("X"))); // print remaining emojis
        }
    }
}
