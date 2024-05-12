using System.Text;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using UnityFastToolsGenerators.Data;
using UnityFastToolsGenerators.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityFastToolsGenerators.Generator.Bodies;
using UnityFastToolsGenerators.Generator.Declarations;

namespace UnityFastToolsGenerators.Generator;

[Generator]
public class UnityFastToolsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // TODO add StructDeclarationSyntax
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax, 
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
        var declaration = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol symbol) 
            return new FoundForSourceGenerator<UnityFastToolsClass>(false, default);

        FindUnityFastToolsMembers(
            symbol,
            out var getComponentMembers,
            out var unityHandlerMembers,
            out var getComponentPropertyMembers);
        
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
        if (symbol.ContainingNamespace.IsGlobalNamespace) return;

        var namespaceName = symbol.ContainingNamespace.ToDisplayString();
        var name = symbol.Name;

        var getComponentBody = new GetComponentBody();
        var unityHandlerBody = new UnityHandlerBody();
        var getComponentPropertyBody = new GetComponentPropertyBody();
        
        unityHandlerBody.Initialize(unityFastToolsClass.UnityHandlerMembers);
        getComponentBody.Initialize(unityFastToolsClass.GetComponentMembers);
        getComponentPropertyBody.Initialize(unityFastToolsClass.GetComponentPropertyMembers);

        // TODO Custom modificator for type
        // TODO Custom type class or struct
        var code = $@"//<auto-generated/>
namespace {namespaceName}
{{
    public partial class {name}
    {{
{getComponentPropertyBody}
{getComponentBody}
{unityHandlerBody}
    }}
}}
";
        
        context.AddSource($"{name}.g.cs", SourceText.From(code, Encoding.UTF8));
    }
}