using Microsoft.CodeAnalysis;

namespace UnityFastToolsGenerators.Helpers;

public static class GetComponentMethodHelper
{
    public static string Get(ITypeSymbol type, int getType)
    {
        var method = "";
        var typeName = type.Name;

        method += "GetComponent";
        if (type is IArrayTypeSymbol arrayTypeSymbol)
        {
            method +="s";
            typeName = arrayTypeSymbol.ElementType.Name;
        }
        
        switch (getType)
        {
            case 1: method += "InChildren"; break;
            case 2: method += "InParent"; break;
        }

        method += $"<{typeName}>()";
        return method;
    }
}
