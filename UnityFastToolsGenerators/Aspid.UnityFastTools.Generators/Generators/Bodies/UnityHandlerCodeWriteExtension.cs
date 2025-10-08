using System.Text;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using UnityFastToolsGenerators.Helpers;
using UnityFastToolsGenerators.Generator.Declarations;
using UnityFastToolsGenerators.Descriptions.UnityEngine;

namespace UnityFastToolsGenerators.Generator.Bodies;

public static class UnityHandlerCodeWriteExtension
{
    // TODO Add custom name from config
    private const string SubscribeMethod = "private void SubsribesUnityHandler()";
    private const string UnsubscribeMethod = "private void UnsubsribesUnityHandler()";
    
    public  static void AppendUnityHandler(this CodeWriter writer, IReadOnlyList<UnityFastToolsMember> members)
    {
        if (members.Count == 0) return;
        
        var minIdentity = writer.Indent;
        var subscribeBody = new List<string>();
        var unsubscribeBody = new List<string>();
        
        for (var i = 0; i < members.Count; i++)
        {
            var subscribe = new StringBuilder();
            var unsubscribe = new StringBuilder();
            
            Append(subscribe, unsubscribe, minIdentity + 1, i, members[i]);
            
            subscribeBody.Add(subscribe.ToString());
            unsubscribeBody.Add(unsubscribe.ToString());
        }
        
        AddMethod(writer, SubscribeMethod, minIdentity + 1, subscribeBody);
        AddMethod(writer, UnsubscribeMethod, minIdentity + 1, unsubscribeBody);
    }

    private static void Append(
        StringBuilder subscribe, StringBuilder unsubscribe, int minIndent, int index, UnityFastToolsMember member)
    {
        var attribute = member.Attribute;
        if (attribute.ConstructorArguments.Length < 1) return;
        
        var constructorArguments = attribute.ConstructorArguments;
        
        string? eventName = null;
        string? methodName = null;

        var constructorArgumentsCount = constructorArguments.Length;
        
        switch (constructorArgumentsCount)
        {
            case 1: methodName = constructorArguments[0].Value?.ToString(); break;
            case 2:
                {
                    var eventValue = constructorArguments[0].Value;
                    
                    if (eventValue is int intValue)
                        eventName = GetEventName(intValue);
                    else eventName = eventValue?.ToString();
                    
                    methodName = constructorArguments[1].Value?.ToString();
                    break;
                }
        }
        
        var symbol = member.Symbol;
        var symbolName = symbol.Name;
        
        if (constructorArgumentsCount == 1)
        {
            var type = symbol switch
            {
                IPropertySymbol propertySymbol => propertySymbol.Type.ToDisplayString(),
                IFieldSymbol fieldSymbol => fieldSymbol.Type.ToDisplayString(),
                _ => null
            };
            
            type = type?.Replace("[]", "");
            
            // TODO Add custom type from config
            eventName = type switch
            {
                ClassesDescription.ButtonFull => GetEventName(0),
                ClassesDescription.ToggleFull or ClassesDescription.SliderFull or ClassesDescription.ScrollRectFull => GetEventName(1),
                _ => eventName
            };
        }
        
        // TODO Add Diagnostic
        if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(methodName)) return;
        
        // var enableSenderArgument =
        //     attribute.NamedArguments.Length > 0 &&
        //     attribute.NamedArguments[0].Key == "EnableSender" &&
        //     (bool)(attribute.NamedArguments[0].Value.Value ?? false);
        
        var isCollection = symbol
            is IFieldSymbol { Type: IArrayTypeSymbol }
            or IPropertySymbol { Type: IArrayTypeSymbol };
        
        if (isCollection)
        {
            const string itemName = "item";
            var foreachText = $"foreach(var {itemName} in {symbolName})";
            
            if (index > 0)
            {
                subscribe.AppendLine();
                unsubscribe.AppendLine();
            }
            
            subscribe
                .AppendLine(Tab(minIndent) + foreachText)
                .Append(GetAddListener(minIndent + 1, itemName));
            
            unsubscribe
                .AppendLine(Tab(minIndent) + foreachText)
                .Append(GetRemoveListener(minIndent + 1, itemName));
        }
        else
        {
            subscribe.Append(GetAddListener(minIndent, symbolName));
            unsubscribe.Append(GetRemoveListener(minIndent, symbolName));
        }
        return;
        
        string GetAddListener(int indent, string name) =>
            $"{Tab(indent)}{name}.{eventName}.AddListener({methodName});";
        
        string GetRemoveListener(int indent, string name) =>
            $"{Tab(indent)}{name}.{eventName}.RemoveListener({methodName});";
        
        string Tab(int indent) => new(' ', indent * 4);
    }
    
    // private void AddAnonymousMethod(string methodName, ISymbol symbol, IMethodSymbol method)
    // {
    //     var parameters = MethodHelper.GetParametersText(method);
    //     if (parameters.Length > 0)
    //         parameters.Append(", ");
    //     
    //     _anonymousMethods.AppendLine($"private void {method.Name}Anonymous({parameters}{symbol.ContainingType.ToDisplayString()} sender)");
    //     
    //     using (_anonymousMethods.BeginBlockScope())
    //     {
    //         _anonymousMethods.AppendLine(methodName);
    //     }
    // }
    
    private static string GetEventName(int value)
    {
        return value switch
        {
            0 => "onClick",
            1 => "onValueChanged",
            
            // TODO Continue and Analizer
            _ => value.ToString()
        };
    }
    
    private static void AddMethod(CodeWriter writer, string name, int identity, List<string> body)
    {
        if (body.Count <= 0) return;
        
        writer
            .AppendLine(name)
            .BeginBlock();
        
        writer.Indent = 0;
        
        foreach (var line in body)
            writer.AppendLine(line);
        
        writer.Indent = identity;
        
        writer
            .EndBlock()
            .AppendLine();
    }
}
