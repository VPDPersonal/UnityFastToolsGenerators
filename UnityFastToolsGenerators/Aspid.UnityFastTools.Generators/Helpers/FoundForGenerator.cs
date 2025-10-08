namespace UnityFastToolsGenerators.Helpers;

public readonly struct FoundForGenerator<T>
{
    public readonly T Container;
    public readonly bool IsNeed;
    
    public FoundForGenerator(bool isNeed, T container)
    {
        IsNeed = isNeed;
        Container = container;
    }
}