using Microsoft.CodeAnalysis;

namespace UnityFastToolsGenerators.Helpers.Extensions.Symbols;

public static class FieldSymbolExtensions
{
    public static string GetPropertyName(this IFieldSymbol symbol) =>
        GetPropertyName(symbol.Name);
    
    public static string GetPropertyName(string name)
    {
        var prefixCount = GetPrefixCount();
        if (prefixCount > 0) name = name.Remove(0, prefixCount);

        var firstSymbol = name[0];
        if (char.IsLower(firstSymbol))
        {
            name = name.Remove(0, 1);
            name = char.ToUpper(firstSymbol) + name;
        }
        
        return name;
        
        // TODO Custom prefix from config
        int GetPrefixCount() =>
            name.StartsWith("_") ? 1 : name.StartsWith("m_") ? 2 : 0;
    }
}