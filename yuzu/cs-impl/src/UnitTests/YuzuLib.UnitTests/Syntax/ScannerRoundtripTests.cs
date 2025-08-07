using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Yuzu.Syntax;

namespace Yuzu.Syntax;

[ExcludeFromCodeCoverage]
public class ScannerRoundtripTests
{
    private const string sourceCode =
"""
/// Test Function
fun foo(i: int, j: float): float {
    /* This is a test */
    val c = '\uABCD';
    val text = "Hello, World!\n";

    val b = 0b010_101L;
    val o = 0o777ul;
    val h = 0xABCD;
    val f = 3.14f;
    val d = 0.314E-5D;

    val k = i + j;

    for (var conter = 0; counter < 10; couter++) {
        print(counter);
    }

    k;
}
""";

    [Fact]
    public void Valid_text_roundtrips_when_using_original_text_and_keeping_trivia()
    {
        var (_, _, tokens) = ScannerTestUtils.Execute(sourceCode, shouldSkipTrivia: false);
        var output = new SourceBuilder(tokens).BuildFromText();

        Assert.Equal(sourceCode, output);
    }
}
