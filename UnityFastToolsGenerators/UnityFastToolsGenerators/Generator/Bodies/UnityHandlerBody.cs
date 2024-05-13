using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using UnityFastToolsGenerators.Helpers.Code;
using UnityFastToolsGenerators.Generator.Declarations;

namespace UnityFastToolsGenerators.Generator.Bodies;

public sealed class UnityHandlerBody
{
    // TODO Add custom name from config
    private const string SubscribesMethod = "private void SubsribesUnityHandler()";
    private const string UnsubscribesMethod = "private void UnsubsribesUnityHandler()";
    
    private readonly CodeWriter _subscribesMethods;
    private readonly CodeWriter _unsubscribesMethods;
    
    public int Length => _subscribesMethods.Length + _unsubscribesMethods.Length;
    
    public UnityHandlerBody(int minIndentLevel)
    {
        _subscribesMethods = new CodeWriter(minIndentLevel);
        _unsubscribesMethods = new CodeWriter(minIndentLevel);
    }
    
    public void Initialize(IReadOnlyList<UnityFastToolsMember<ISymbol>> members)
    {
        Clear();
        if (members.Count == 0) return;

        _subscribesMethods.AppendLine(SubscribesMethod).BeginBlock();
        _unsubscribesMethods.AppendLine(UnsubscribesMethod).BeginBlock();
        
        for (var i = 0; i < members.Count; i++)
            Append(i, members[i]);
        
        _subscribesMethods.EndBlock();
        _unsubscribesMethods.EndBlock();
    }

    private void Append(int index, UnityFastToolsMember<ISymbol> member)
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
            var foreachText = $"foreach(var {itemName} in {symbolName})";
            
            if (index > 0)
            {
                _subscribesMethods.AppendLine();
                _unsubscribesMethods.AppendLine();
            }
            
            _subscribesMethods
                .AppendLine(foreachText)
                .IncreaseIndent()
                .AppendLine(GetAddListener(itemName))
                .DecreaseIndent();
            
            _unsubscribesMethods
                .AppendLine(foreachText)
                .IncreaseIndent()
                .AppendLine(GetRemoveListener(itemName))
                .DecreaseIndent();
        }
        else
        {
            _subscribesMethods.AppendLine(GetAddListener(symbolName));
            _unsubscribesMethods.AppendLine(GetRemoveListener(symbolName));
        }
        return;

        string GetAddListener(string name) =>
            $"{name}.{eventName}.AddListener({methodName});";
        
        string GetRemoveListener(string name) =>
            $"{name}.{eventName}.RemoveListener({methodName});";
    }

    public override string ToString()
    {
        return _subscribesMethods.Length + _unsubscribesMethods.Length > 0 
            ? $"{_subscribesMethods}\n{_unsubscribesMethods}" 
            : "";
    }

    private void Clear()
    {
        _subscribesMethods.Clear();
        _unsubscribesMethods.Clear();
    }
}
