using System.Text;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using UnityFastToolsGenerators.Data;
using UnityFastToolsGenerators.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityFastToolsGenerators.Generator.Bodies;
using UnityFastToolsGenerators.Generator.Declarations;
using UnityFastToolsGenerators.Helpers.Code;

namespace UnityFastToolsGenerators.Generator;

[Generator]
public class UnityFastToolsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // TODO add StructDeclarationSyntax
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is TypeDeclarationSyntax, 
                transform: static (syntaxContext, _) => FindUnityFastToolClass(syntaxContext))
            .Where(foundForSourceGenerator => foundForSourceGenerator.IsNeed)
            .Select((foundForSourceGenerator, _) => foundForSourceGenerator.Container);

        var combineProvider = context.CompilationProvider.Combine(provider.Collect());
        
        context.RegisterSourceOutput(
            source: combineProvider,
            action: (productionContext, tuple) => GenerateCode(productionContext, tuple.Left, tuple.Right));
    }

    // TODO Finish after FindUnityFastToolsMembers
    private static FoundForSourceGenerator<UnityFastToolsClass> FindUnityFastToolClass(GeneratorSyntaxContext context)
    {
        var declaration = (TypeDeclarationSyntax)context.Node;
        
        if (declaration is not ClassDeclarationSyntax && declaration is not StructDeclarationSyntax)
            return new FoundForSourceGenerator<UnityFastToolsClass>(false, default);
        
        if (context.SemanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol symbol) 
            return new FoundForSourceGenerator<UnityFastToolsClass>(false, default);

        FindUnityFastToolsMembers(
            symbol,
            out var getComponentMembers,
            out var unityHandlerMembers,
            out var getComponentPropertyMembers);
        
        if (declaration is StructDeclarationSyntax)
        {
            getComponentMembers.Clear();
            getComponentPropertyMembers.Clear();
        }
        
        if (getComponentMembers.Count > 0 || unityHandlerMembers.Count > 0 || getComponentPropertyMembers.Count > 0)
            return new FoundForSourceGenerator<UnityFastToolsClass>(true, 
                new UnityFastToolsClass(declaration, unityHandlerMembers, getComponentMembers, getComponentPropertyMembers)); 
        
        return new FoundForSourceGenerator<UnityFastToolsClass>(false, default); 
    }

    // TODO Finish
    private static void FindUnityFastToolsMembers(
        INamedTypeSymbol symbol,
        out List<UnityFastToolsMember<ISymbol>> getComponentMembers,
        out List<UnityFastToolsMember<ISymbol>> unityHandlerMembers,
        out List<UnityFastToolsMember<ISymbol>> getComponentPropertyMembers)
    {
        getComponentMembers = new List<UnityFastToolsMember<ISymbol>>();
        unityHandlerMembers = new List<UnityFastToolsMember<ISymbol>>();
        getComponentPropertyMembers = new List<UnityFastToolsMember<ISymbol>>();
        
        foreach (var member in symbol.GetMembers())
        {
            if (member is not IFieldSymbol && member is not IPropertySymbol) continue;

            var isReadOnly = false;
            var isWriteOnly = false;

            if (member is IPropertySymbol propertySymbol)
            {
                if (propertySymbol.IsIndexer) continue;

                isReadOnly = propertySymbol.IsReadOnly;
                isWriteOnly = propertySymbol.IsWriteOnly;
            }

            foreach (var attribute in member.GetAttributes())
            {
                var attributeName = attribute.AttributeClass?.Name ?? "";
                
                if (attributeName == AttributesData.GetComponentName)
                {
                    if (isReadOnly) continue;
                    getComponentMembers.Add(new UnityFastToolsMember<ISymbol>(member, attribute));
                }
                else if (attributeName == AttributesData.UnityHandlerName)
                {
                    if (isWriteOnly) continue;
                    unityHandlerMembers.Add(new UnityFastToolsMember<ISymbol>(member, attribute));
                }
                else if (attributeName == AttributesData.GetComponentPropertyName)
                {
                    getComponentPropertyMembers.Add(new UnityFastToolsMember<ISymbol>(member, attribute));
                }
            }
        }
    }

    private static void GenerateCode(SourceProductionContext context, Compilation compilation, ImmutableArray<UnityFastToolsClass> unityFastToolsClasses)
    {
        foreach (var unityFastToolsClass in unityFastToolsClasses)
            GenerateCode(context, compilation, unityFastToolsClass);
    }

    private static void GenerateCode(SourceProductionContext context, Compilation compilation, UnityFastToolsClass unityFastToolsClass)
    {
        var declaration = unityFastToolsClass.DeclarationSyntax;
        var semanticModel = compilation.GetSemanticModel(declaration.SyntaxTree);
        if (semanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol symbol) return;
        
        var name = symbol.Name;
        var fileType = declaration is ClassDeclarationSyntax ? "class" : "struct";
        
        var modifiers = new StringBuilder();
        foreach (var modifier in declaration.Modifiers)
        {
            if (modifier.ToString() == "partial") continue;
            if (modifiers.Length > 0) modifiers.Append(" ");
            modifiers.Append(modifier.ToString());
        }
        
        var isGlobalNamespace = symbol.ContainingNamespace.IsGlobalNamespace;
        var minIndentLevel = isGlobalNamespace ? 1 : 2;
        
        var getComponentBody = new GetComponentBody(minIndentLevel);
        var unityHandlerBody = new UnityHandlerBody(minIndentLevel);
        var getComponentPropertyBody = new GetComponentPropertyBody(minIndentLevel);
        
        getComponentBody.Initialize(unityFastToolsClass.GetComponentMembers);
        unityHandlerBody.Initialize(unityFastToolsClass.UnityHandlerMembers);
        getComponentPropertyBody.Initialize(unityFastToolsClass.GetComponentPropertyMembers);
        
        var body = new StringBuilder();
        if (getComponentPropertyBody.Length > 0) body.AppendLine(getComponentPropertyBody.ToString());
        if (getComponentBody.Length > 0) body.AppendLine(getComponentBody.ToString());
        if (unityHandlerBody.Length > 0) body.AppendLine(unityHandlerBody.ToString());
        if (body.Length > 1) body.Length -= 2;
        
        var code = GetCode(symbol, isGlobalNamespace, modifiers.ToString(), fileType, name, body.ToString());
        context.AddSource($"{name}.FastTools.g.cs", SourceText.From(code, Encoding.UTF8));
    }
    
    private static string GetCode(INamedTypeSymbol symbol, bool isGlobalNamespace, string modifiers, string fileType, string name, string body)
    {
        var code = new CodeWriter();
        code.AppendLine("// <auto-generated/>");
        
        var genericTypes = new StringBuilder();
        
        if (symbol.IsGenericType)
        {
            genericTypes.Append("<");
            foreach (var parameter in symbol.TypeParameters)
                genericTypes.Append(parameter);
            genericTypes.Append(">");
        }
        
        if (!isGlobalNamespace)
        {
            var namespaceName = symbol.ContainingNamespace.ToDisplayString();
            code.AppendLine($"namespace {namespaceName}")
                .AppendLine("{")
                .IncreaseIndent();
        }
        
        code.AppendLine($"{modifiers} partial {fileType} {name}{genericTypes}")
            .AppendLine("{");
        
        if (!isGlobalNamespace)
            code.DecreaseIndent();
        
        code.AppendLine($"{body}");
        
        if (!isGlobalNamespace)
            code.IncreaseIndent();
        
        code.AppendLine("}");
        
        if (!isGlobalNamespace)
        {
            code.DecreaseIndent()
                .AppendLine("}");
        }
        
        return code.ToString();
    }
}