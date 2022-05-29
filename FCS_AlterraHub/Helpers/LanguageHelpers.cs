namespace FCS_AlterraHub.Helpers
{
    internal static class LanguageHelpers
    {
        internal static string GetLanguage(string key)
        {
            return Language.main.Get(key);
        }

        internal static string GetLanguage(TechType techType)
        {
            return Language.main.Get(techType);
        }
    }
}
