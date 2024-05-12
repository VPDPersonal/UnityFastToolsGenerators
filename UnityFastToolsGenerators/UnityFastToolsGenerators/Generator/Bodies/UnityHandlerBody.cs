using System.Text;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using UnityFastToolsGenerators.Generator.Declarations;

namespace UnityFastToolsGenerators.Generator.Bodies;

public sealed class UnityHandlerBody
{
    // TODO Add custom name from config
    private const string SubscribesMethod = "private void SubsribesUnityHandler()";
    private const string UnsubscribesMethod = "private void UnsubsribesUnityHandler()";
    
    private readonly StringBuilder _subscribesMethods = new();
    private readonly StringBuilder _unsubscribesMethods = new();

    public void Initialize(IReadOnlyCollection<UnityFastToolsMember<ISymbol>> members)
    {
        Clear();
        if (members.Count == 0) return;

        _subscribesMethods.AppendLine($"\t\t{SubscribesMethod}\n\t\t{{");
        _unsubscribesMethods.AppendLine($"\t\t{UnsubscribesMethod}\n\t\t{{");
        
        foreach (var member in members)
            Append(member);
        
        _subscribesMethods.AppendLine("\t\t}");
        _unsubscribesMethods.AppendLine("\t\t}");
    }

    private void Append(UnityFastToolsMember<ISymbol> member)
    {
        var attribute = member.Attribute;
        if (attribute.NamedArguments.Length + attribute.ConstructorArguments.Length < 2) return;

        var constructorArguments = attribute.ConstructorArguments;
        var eventName = (string)(constructorArguments[0].Value ?? "");
        var methodName = (string)(constructorArguments[1].Value ?? "");
        
        if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(methodName)) return;

        var symbol = member.Symbol;
        var symbolName = symbol.Name;
        var isCollection = symbol
            is IFieldSymbol { Type: IArrayTypeSymbol }
            or IPropertySymbol { Type: IArrayTypeSymbol };

        if (isCollection)
        {
            const string itemName = "item";
            var foreachText = $"\t\t\tforeach(var {itemName} in {symbolName})";
            
            _subscribesMethods.AppendLine(foreachText);
            _subscribesMethods.AppendLine($"\t{GetAddListener(itemName)}");
            
            _unsubscribesMethods.AppendLine(foreachText);
            _unsubscribesMethods.AppendLine($"\t{GetRemoveListener(itemName)}");
        }
        else
        {
            _subscribesMethods.AppendLine(GetAddListener(symbolName));
            _unsubscribesMethods.AppendLine(GetRemoveListener(symbolName));
        }
        return;

        string GetAddListener(string name) =>
            $"\t\t\t{name}.{eventName}.AddListener({methodName});";
        
        string GetRemoveListener(string name) =>
            $"\t\t\t{name}.{eventName}.RemoveListener({methodName});";
    }

    public override string ToString()
    {
        return _subscribesMethods.Length > 0 
            ? $"{_subscribesMethods}\n{_unsubscribesMethods}" 
            : "";
    }

    private void Clear()
    {
        _subscribesMethods.Clear();
        _unsubscribesMethods.Clear();
    }
}
