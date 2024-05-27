using System.Text;
using Microsoft.CodeAnalysis;

namespace UnityFastToolsGenerators.Helpers.UnityFastTools;

public static class GetComponentMethodHelper
{
    public static string Get(ITypeSymbol type, int whereGet)
    {
        var method = new StringBuilder();

        method.Append("GetComponent");
        if (type is IArrayTypeSymbol arrayTypeSymbol)
        {
            method.Append("s");
            type = arrayTypeSymbol.ElementType;
        }
        
        switch (whereGet)
        {
            case 1: method.Append("InChildren"); break;
            case 2: method.Append("InParent"); break;
        }

        method.Append($"<{type}>()");
        return method.ToString();
    }
}
