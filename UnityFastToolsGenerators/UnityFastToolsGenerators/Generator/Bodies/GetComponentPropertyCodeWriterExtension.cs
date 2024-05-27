using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using UnityFastToolsGenerators.Helpers;
using UnityFastToolsGenerators.Helpers.Code;
using UnityFastToolsGenerators.Helpers.UnityFastTools;
using UnityFastToolsGenerators.Generator.Declarations;
using UnityFastToolsGenerators.Descriptions.UnityFastTools;

namespace UnityFastToolsGenerators.Generator.Bodies;

public static class GetComponentPropertyCodeWriterExtension
{
    // TODO Add custom name from config
    private const string PrefixName = "Cached";
    
    public static void AppendGetComponentProperty(this CodeWriter writer, IReadOnlyCollection<UnityFastToolsMember> members)
    {
        foreach (var member in members)
            Append(writer, member);
        
        if (members.Count > 0)
            writer.AppendLine();
    }

    private static void Append(CodeWriter writer, UnityFastToolsMember member)
    {
        var symbol = member.Symbol;
        var type = GetSymbolType(symbol);
        if (type == null) return;
        
        GetAttributeArguments(member.Attribute, out var access, out var whereGet);
        writer.AppendLine(GetMethod(symbol, type, access, whereGet));
    }
    
    private static ITypeSymbol? GetSymbolType(ISymbol symbol) => symbol switch
    {
        IFieldSymbol fieldSymbol => fieldSymbol.Type,
        IPropertySymbol propertySymbol => propertySymbol.Type,
        _ => null
    };
    
    private static void GetAttributeArguments(AttributeData attribute, out int access, out int whereGet)
    {
        access = 0;
        whereGet = 0;

        foreach (var argument in attribute.ConstructorArguments)
        {
            switch (argument.Type?.ToString())
            {
                case EnumsDescription.WhereGet: whereGet = (int)(argument.Value ?? 0); break;
                case EnumsDescription.AccessFull: access = (int)(argument.Value ?? 0); break;
            }
        }
    }
    
    private static string GetMethod(ISymbol symbol, ITypeSymbol type, int access, int whereGet)
    {
        var name = symbol.Name;
        var propertyName = $"{PrefixName}{FieldHelper.GetPropertyNameFromField(name)}";
        var boolValue = type is IArrayTypeSymbol ? $"{name} != null && {name}.Length > 0" : name;
        var getMethod = GetComponentMethodHelper.Get(type, whereGet);
        
        return $"{AccessHelper.Get(access)} {type} {propertyName} => {boolValue} ? {name} : ({name} = {getMethod});";
    }
}
