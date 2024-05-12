using Microsoft.CodeAnalysis;

namespace UnityFastToolsGenerators.Generator.Declarations;

public readonly struct UnityFastToolsMember<TSymbol>
    where TSymbol : ISymbol
{
    public readonly TSymbol Symbol;
    public readonly AttributeData Attribute;

    public UnityFastToolsMember(TSymbol symbol, AttributeData attribute)
    {
        Symbol = symbol;
        Attribute = attribute;
    }
}
