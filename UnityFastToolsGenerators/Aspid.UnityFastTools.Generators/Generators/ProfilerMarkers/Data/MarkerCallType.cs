using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace UnityFastToolsGenerators.Generators.ProfilerMarkers.Data;

public readonly struct MarkerCallType(
    ISymbol symbol,
    ImmutableArray<MarkerCall> markerCalls)
{
    public readonly ISymbol Symbol = symbol;
    public readonly ImmutableArray<MarkerCall> MarkerCalls = markerCalls;
}