using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityFastToolsGenerators.Helpers.Declarations;

public static class TypeDeclarationSyntaxExtension
{
    public static string GetGenericArguments(this TypeDeclarationSyntax declaration)
    {
        var types = declaration.TypeParameterList;
        if (types == null || types.Parameters.Count == 0) return "";
        
        var genericTypes = new StringBuilder("<");
        foreach (var type in types.Parameters)
            genericTypes.Append(type.Identifier.Text);
        genericTypes.Append(">");
        
        return genericTypes.ToString();
    }
}