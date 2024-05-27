using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using UnityFastToolsGenerators.Helpers.Code;
using UnityFastToolsGenerators.Helpers.UnityFastTools;
using UnityFastToolsGenerators.Generator.Declarations;

namespace UnityFastToolsGenerators.Generator.Bodies;

public static class GetComponentCodeWriterExtension 
{
    // TODO Add custom name from config
    private const string GetComponentMethod = "private void GetUnityComponents(UnityEngine.Object unityObject)";
    
    public static void AppendGetComponent(
        this CodeWriter writer,
        IReadOnlyCollection<UnityFastToolsMember<ISymbol>> members)
    {
        if (members.Count == 0) return;
        
        writer.AppendLine(GetComponentMethod);
        using (writer.BeginBlockScope())
        {
            foreach (var member in members)
                Append(writer, member);
        }
        
        writer.AppendLine();
    }

    private static void Append(CodeWriter writer, UnityFastToolsMember<ISymbol> member)
    {
        var symbol = member.Symbol;
        var type = GetSymbolType(symbol);
        if (type == null) return;
        
        GetAttributeArguments(member.Attribute, out var whereGet);
        writer.AppendLine($"{symbol.Name} = unityObject.{GetComponentMethodHelper.Get(type, whereGet)};");
    }
    
    private static ITypeSymbol? GetSymbolType(ISymbol symbol) => symbol switch
    {
        IFieldSymbol fieldSymbol => fieldSymbol.Type,
        IPropertySymbol propertySymbol => propertySymbol.Type,
        _ => null
    };
    
    private static void GetAttributeArguments(AttributeData attribute, out int whereGet)
    {
        var constructorArguments = attribute.ConstructorArguments;
        whereGet = constructorArguments.Length == 1 ? (int)(constructorArguments[0].Value ?? 0) : 0;
    }
}
