using System.Collections.Generic;
using System.Text;

namespace Yuzu.Syntax;

internal sealed class SourceBuilder(IEnumerable<TokenInfo> items)
{
    public string BuildFromText()
    {
        var builder = new StringBuilder();
        foreach (var item in items)
        {
            switch (item.Token)
            {
                case SyntaxKind.Eof: break;
                default: builder.Append(item.Text); break;
            }
        }

        return builder.ToString();
    }

    // NB: roundtripping based on token values is much more complicated and should be coded
    // as a utility in the main assembly. Such a utility should be ableto work out a valid source code
    // based on either the stream of tokens or an AST.
}
