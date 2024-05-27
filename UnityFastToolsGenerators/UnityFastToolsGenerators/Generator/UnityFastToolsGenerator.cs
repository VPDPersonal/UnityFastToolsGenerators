using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using UnityFastToolsGenerators.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityFastToolsGenerators.Helpers.Code;
using UnityFastToolsGenerators.Generator.Bodies;
using UnityFastToolsGenerators.Helpers.Declarations;
using UnityFastToolsGenerators.Generator.Declarations;
using UnityFastToolsGenerators.Descriptions.UnityFastTools;

namespace UnityFastToolsGenerators.Generator;

[Generator]
public sealed class UnityFastToolsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax, 
                transform: static (syntaxContext, _) => FindUnityFastToolClass(syntaxContext))
            .Where(foundForSourceGenerator => foundForSourceGenerator.IsNeed)
            .Select((foundForSourceGenerator, _) => foundForSourceGenerator.Container);
        
        var combineProvider = context.CompilationProvider.Combine(provider.Collect());
        
        context.RegisterSourceOutput(
            source: combineProvider,
            action: (productionContext, tuple) => GenerateCode(productionContext, tuple.Left, tuple.Right));
    }
    
    private static FoundForGenerator<TypeDeclarationSyntax> FindUnityFastToolClass(GeneratorSyntaxContext context)
    {
        var declaration = (TypeDeclarationSyntax)context.Node;

        foreach (var member in declaration.Members)
        {
            if (member is IndexerDeclarationSyntax)
                continue;
            
            foreach (var attribute in member.AttributeLists.SelectMany(attributeList => attributeList.Attributes))
            {
                if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeSymbol)
                    continue;
                
                var attributeName = attributeSymbol.ContainingType?.ToDisplayString();
                
                switch (attributeName)
                {
                    case AttributesDescription.GetComponentFull:
                    {
                        if (member is PropertyDeclarationSyntax propertyDeclaration)
                            return propertyDeclaration.HasAccessor(SyntaxKind.SetAccessorDeclaration) ? ReturnTrue() : default;
                        
                        return ReturnTrue();
                    }
                    
                    case AttributesDescription.UnityHandlerFull:
                    {
                        if (member is PropertyDeclarationSyntax propertyDeclaration)
                            return propertyDeclaration.HasAccessor(SyntaxKind.GetAccessorDeclaration) ? ReturnTrue() : default;
                        
                        return ReturnTrue();
                    }
                    
                    case AttributesDescription.GetComponentPropertyFull: return ReturnTrue();
                }
            }
        }
        
        return default;
        
        FoundForGenerator<TypeDeclarationSyntax> ReturnTrue() => new(true, declaration);
    }

    private static void GenerateCode(SourceProductionContext context, Compilation compilation, ImmutableArray<TypeDeclarationSyntax> declarations)
    {
        foreach (var declaration in declarations)
            GenerateCode(context, compilation, declaration);
    }

    private static void GenerateCode(SourceProductionContext context, Compilation compilation, TypeDeclarationSyntax declaration)
    {
        if (!TryGetModifiers(declaration, out var modifiers)) return;
        
        var name = declaration.Identifier.Text;
        var namespaceName = declaration.GetNamespaceName();
        var genericArguments = declaration.GetGenericArgumentsForDeclaration();
        var fileType = declaration is ClassDeclarationSyntax ? "class" : "struct";

        var codeWriter = new CodeWriter();
        codeWriter.AppendLine("// <auto-generated>");
        
        if (!string.IsNullOrEmpty(namespaceName))
        {
            codeWriter
                .AppendLine($"namespace {namespaceName}")
                .BeginBlock();   
        }
        
        codeWriter
            .AppendLine($"{modifiers} {fileType} {name}{genericArguments}")
            .BeginBlock();
        
        FindNeedMembers(
            compilation,
            declaration,
            out var getComponentMembers,
            out var unityHandlerMembers,
            out var getComponentPropertyMembers);
        
        codeWriter.AppendGetComponentProperty(getComponentPropertyMembers);
        codeWriter.AppendGetComponent(getComponentMembers);
        codeWriter.AppendUnityHandler(unityHandlerMembers);
        
        if (!string.IsNullOrEmpty(namespaceName))
            codeWriter.EndBlock();
        
        codeWriter.EndBlock();
        
        context.AddSource($"{name}.FastTools.g.cs", codeWriter.GetSourceText());
    }
    
    private static bool TryGetModifiers(TypeDeclarationSyntax declaration, out StringBuilder modifiers)
    {
        var isPartial = false;
        modifiers = new StringBuilder();
        
        foreach (var modifier in declaration.Modifiers)
        {
            if (modifier.IsKind(SyntaxKind.PartialKeyword))
                isPartial = true;
            
            if (modifiers.Length > 0) modifiers.Append(" ");
            modifiers.Append(modifier.ToString());
        }
        
        return isPartial;
    }
    
    private static void FindNeedMembers(
        Compilation compilation,
        TypeDeclarationSyntax declaration,
        out List<UnityFastToolsMember> getComponentMembers,
        out List<UnityFastToolsMember> unityHandlerMembers,
        out List<UnityFastToolsMember> getComponentPropertyMembers)
    {
        getComponentMembers = new List<UnityFastToolsMember>();
        unityHandlerMembers = new List<UnityFastToolsMember>();
        getComponentPropertyMembers = new List<UnityFastToolsMember>();
        
        var semanticModel = compilation.GetSemanticModel(declaration.SyntaxTree);
        if (semanticModel.GetDeclaredSymbol(declaration) is not { } symbol) return;
        
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
                var name = attribute.AttributeClass?.ToDisplayString();
                
                switch (name)
                {
                    case AttributesDescription.GetComponentFull:
                    {
                        if (isReadOnly) continue;
                        getComponentMembers.Add(new UnityFastToolsMember(member, attribute)); 
                        
                        break;
                    }
                    
                    case AttributesDescription.UnityHandlerFull:
                    {
                        if (isWriteOnly) continue;
                        unityHandlerMembers.Add(new UnityFastToolsMember(member, attribute)); 
                        
                        break;
                    }
                    
                    case AttributesDescription.GetComponentPropertyFull: 
                        getComponentPropertyMembers.Add(new UnityFastToolsMember(member, attribute)); break;
                }
            }
        }
    }
}