// using System.Threading;
// using System.Diagnostics;
// using Microsoft.CodeAnalysis;
// using System.Collections.Generic;
// using Microsoft.CodeAnalysis.CSharp;
// using System.Runtime.CompilerServices;
// using UnityFastToolsGenerators.Helpers;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
// using UnityFastToolsGenerators.Helpers.Extensions.Symbols;
// using UnityFastToolsGenerators.Generators.HandlerGenerator.Data;
//
// namespace UnityFastToolsGenerators.Generators.HandlerGenerator;
//
// [Generator(LanguageNames.CSharp)]
// public sealed class HandlerGenerator : IIncrementalGenerator
// {
//     public void Initialize(IncrementalGeneratorInitializationContext context)
//     {
//         var provider = context.SyntaxProvider.CreateSyntaxProvider(SyntacticPredicate, FindHandlers)
//             .Where(foundForGenerator => foundForGenerator.IsNeed)
//             .Select((foundForGenerator, _) => foundForGenerator.Container);
//         
//         context.RegisterSourceOutput(
//             source: provider,
//             action: GenerateCode);
//     }
//
//     private static bool SyntacticPredicate(SyntaxNode node, CancellationToken _)
//     {
//         var candidate = node switch
//         {
//             ClassDeclarationSyntax or StructDeclarationSyntax => node as TypeDeclarationSyntax,
//             _ => null
//         };
//
//         return candidate is not null
//             && candidate.Modifiers.Any(SyntaxKind.PartialKeyword)
//             && !candidate.Modifiers.Any(SyntaxKind.StaticKeyword);
//     }
//
//     private static FoundForGenerator<HandlerData> FindHandlers(GeneratorSyntaxContext context, CancellationToken cancellationToken)
//     {
//         Debug.Assert(context.Node is TypeDeclarationSyntax);
//         var candidate = Unsafe.As<TypeDeclarationSyntax>(context.Node);
//         var symbol = context.SemanticModel.GetDeclaredSymbol(candidate, cancellationToken);
//         if (symbol == null) return default;
//
//         var fields = new List<IFieldSymbol>();
//         var methods = new List<IMethodSymbol>();
//         
//         foreach (var member in symbol.GetMembers())
//         {
//             if (member is IFieldSymbol field)
//             {
//                 if (field.HasAttribute(""))
//                     fields.Add(field);
//             }
//             else if (member is IMethodSymbol method)
//             {
//                 if (method.HasAttribute(""))
//                     methods.Add(method);
//             }
//         }
//
//         if (fields.Count + methods.Count == 0) return default;
//         return new FoundForGenerator<HandlerData>(true, new HandlerData());
//     }
//
//     private static void GenerateCode(SourceProductionContext context, HandlerData handlerData)
//     {
//         
//     }
// }