using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using UnityFastToolsGenerators.Helpers;
using UnityFastToolsGenerators.Helpers.UnityFastTools;
using UnityFastToolsGenerators.Generator.Declarations;
using UnityFastToolsGenerators.Descriptions.UnityEngine;

namespace UnityFastToolsGenerators.Generator.Bodies;

public static class GetComponentCodeWriterExtension
{
    private const string MethodParameterName = "unityComponent";
    // TODO Add custom name from config
    private const string GetComponentMethod = $"private void GetUnityComponents({ClassesDescription.ComponentFull} {MethodParameterName})";
    
    public static void AppendGetComponent(
        this CodeWriter writer,
        IReadOnlyCollection<UnityFastToolsMember> members)
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

    private static void Append(CodeWriter writer, UnityFastToolsMember member)
    {
        var symbol = member.Symbol;
        var type = GetSymbolType(symbol);
        if (type == null) return;
        
        GetAttributeArguments(member.Attribute, out var whereGet);
        writer.AppendLine($"{symbol.Name} = {MethodParameterName}.{GetComponentMethodHelper.Get(type, whereGet)};");
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
