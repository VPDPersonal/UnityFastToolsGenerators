namespace UnityFastToolsGenerators.Helpers;

public static class FieldHelper
{
    public static string GetPropertyNameFromField(string name)
    {
        if (name.StartsWith("_")) name = name.Remove(0, 1);
        else if (name.StartsWith("m_")) name = name.Remove(0, 2);
        
        var firstSymbol = name[0];
        if (char.IsLower(firstSymbol))
        {
            name = name.Remove(0, 1);
            name = char.ToUpper(firstSymbol) + name;
        }
        
        return name;
    }
}