using System.Text;
using Microsoft.CodeAnalysis;

namespace UnityFastToolsGenerators.Helpers;

public static class MethodHelper
{
    public static StringBuilder GetParametersText(IMethodSymbol symbol)
    {
        var text = new StringBuilder();
        
        foreach (var parameter in symbol.Parameters)
        {
            if (text.Length > 0) 
                text.Append(", ");
            
            text.Append(parameter.ContainingType.ToDisplayString() + " ");
            text.Append(parameter.Name);
        }
        
        return text;
    }
}