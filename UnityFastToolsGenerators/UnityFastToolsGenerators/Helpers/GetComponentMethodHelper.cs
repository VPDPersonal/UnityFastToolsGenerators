using Microsoft.CodeAnalysis;

namespace UnityFastToolsGenerators.Helpers;

public static class GetComponentMethodHelper
{
    public static string Get(ITypeSymbol type, int getType)
    {
        var method = "";

        method += "GetComponent";
        if (type is IArrayTypeSymbol arrayTypeSymbol)
        {
            method +="s";
            type = arrayTypeSymbol.ElementType;
        }
        
        switch (getType)
        {
            case 1: method += "InChildren"; break;
            case 2: method += "InParent"; break;
        }

        method += $"<{type}>()";
        return method;
    }
}
