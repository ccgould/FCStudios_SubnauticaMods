namespace FCSCommon.Helpers
{
    internal static class LanguageHelpers
    {
        internal static string GetLanguage(string key)
        {
            return Language.main.Get(key);
        }
    }
}
