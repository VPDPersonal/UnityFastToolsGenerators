using System;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using UnityFastToolsGenerators.Data;
using UnityFastToolsGenerators.Helpers;
using UnityFastToolsGenerators.Generator.Declarations;

namespace UnityFastToolsGenerators.Generator.Bodies;

public sealed class GetComponentPropertyBody
{
    // TODO Add custom name from config
    private const string PrefixName = "Cached";
    
    private readonly StringBuilder _body = new();
    
    public void Initialize(IReadOnlyCollection<UnityFastToolsMember<ISymbol>> members)
    {
        Clear();

        foreach (var member in members)
            Append(member);
    }

    private void Append(UnityFastToolsMember<ISymbol> member)
    {
        ITypeSymbol type;
        
        var access = 0;
        var getType = 0;
        var symbol = member.Symbol;
        var constructorArguments = member.Attribute.ConstructorArguments;

        if (constructorArguments.Length > 0)
        {
            foreach (var argument in constructorArguments)
            {
                switch (argument.Type?.ToString())
                {
                    case ClassesData.GetComponentTypeFull: getType = (int)(argument.Value ?? 0); break;
                    case ClassesData.PropertyAccessFull: access = (int)(argument.Value ?? 0); break;
                }
            }
        }
        
        switch (symbol)
        {
            case IFieldSymbol fieldSymbol: type = fieldSymbol.Type; break;
            case IPropertySymbol propertySymbol: type = propertySymbol.Type; break;
            default: return;
        }

        var name = symbol.Name;
        
        var propertyName = $"{PrefixName}{FieldHelper.GetPropertyNameFromField(name)}";
        var boolValue = type is IArrayTypeSymbol ? $"{name} != null && {name}.Length > 0" : name;
        
        _body.AppendLine($"\t\t{AccessHelper.Get(access)} {type} {propertyName} => {boolValue} ? {name} : ({name} = {GetComponentMethodHelper.Get(type, getType)});");
    }

    public override string ToString() =>
        _body.ToString();

    private void Clear() =>
        _body.Clear();
}
