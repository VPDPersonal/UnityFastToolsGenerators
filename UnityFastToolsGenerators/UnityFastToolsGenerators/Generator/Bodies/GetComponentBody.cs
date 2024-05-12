using System.Text;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using UnityFastToolsGenerators.Generator.Declarations;
using UnityFastToolsGenerators.Helpers;

namespace UnityFastToolsGenerators.Generator.Bodies;

public sealed class GetComponentBody
{
    // TODO Add custom name from config
    private const string GetComponentMethod = "private void GetUnityComponents()";
    
    private readonly StringBuilder _body = new();

    public void Initialize(IReadOnlyCollection<UnityFastToolsMember<ISymbol>> members)
    {
        Clear();
        if (members.Count == 0) return;
        
        _body.AppendLine($"\t\t{GetComponentMethod}\n\t\t{{");
        
        foreach (var member in members)
            Append(member);
        
        _body.AppendLine("\t\t}");
    }

    private void Append(UnityFastToolsMember<ISymbol> member)
    {
        ITypeSymbol type;
        var symbol = member.Symbol;
        
        var constructorArguments = member.Attribute.ConstructorArguments;
        var getType = constructorArguments.Length == 1 ? (int)(constructorArguments[0].Value ?? 0) : 0;
        
        switch (symbol)
        {
            case IFieldSymbol fieldSymbol: type = fieldSymbol.Type; break;
            case IPropertySymbol propertySymbol: type = propertySymbol.Type; break;
            default: return;
        }

        _body.AppendLine($"\t\t\t{symbol.Name} = {GetComponentMethodHelper.Get(type, getType)};");
    }

    public override string ToString() => 
        _body.ToString();
    
    private void Clear() =>
        _body.Clear();
}
