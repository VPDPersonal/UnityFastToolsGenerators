using Microsoft.CodeAnalysis;

namespace UnityFastToolsGenerators.Generator.Declarations;

public readonly struct UnityFastToolsMember
{
    public readonly ISymbol Symbol;
    public readonly AttributeData Attribute;

    public UnityFastToolsMember(ISymbol symbol, AttributeData attribute)
    {
        Symbol = symbol;
        Attribute = attribute;
    }
}
