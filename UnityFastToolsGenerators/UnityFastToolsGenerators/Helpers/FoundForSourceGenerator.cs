namespace UnityFastToolsGenerators.Helpers;

public readonly struct FoundForSourceGenerator<T>
{
    public readonly T Container;
    public readonly bool IsNeed;
    
    public FoundForSourceGenerator(bool isAnyNeedField, T container)
    {
        Container = container;
        IsNeed = isAnyNeedField;
    }
}