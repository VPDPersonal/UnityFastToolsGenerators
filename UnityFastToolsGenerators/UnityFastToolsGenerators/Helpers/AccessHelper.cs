namespace UnityFastToolsGenerators.Helpers;

public static class AccessHelper
{
    public static string Get(int access) => access switch
    {
        1 => "protected",
        2 => "public",
        _ => "private"
    };
}
