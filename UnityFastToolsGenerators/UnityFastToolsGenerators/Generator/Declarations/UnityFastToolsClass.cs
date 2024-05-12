using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityFastToolsGenerators.Generator.Declarations;

public readonly struct UnityFastToolsClass
{
    public readonly ClassDeclarationSyntax DeclarationSyntax;
    public readonly IReadOnlyList<UnityFastToolsMember<ISymbol>> UnityHandlerMembers;
    public readonly IReadOnlyList<UnityFastToolsMember<ISymbol>> GetComponentMembers;
    public readonly IReadOnlyList<UnityFastToolsMember<ISymbol>> GetComponentPropertyMembers;

    public UnityFastToolsClass(
        ClassDeclarationSyntax declarationSyntax, 
        IReadOnlyList<UnityFastToolsMember<ISymbol>> unityHandlerMembers,
        IReadOnlyList<UnityFastToolsMember<ISymbol>> getComponentMembers,
        IReadOnlyList<UnityFastToolsMember<ISymbol>> getComponentPropertyMembers)
    {
        DeclarationSyntax = declarationSyntax;
        UnityHandlerMembers = unityHandlerMembers;
        GetComponentMembers = getComponentMembers;
        GetComponentPropertyMembers = getComponentPropertyMembers;
    }
}
