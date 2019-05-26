namespace FCSCommon.Helpers
{
    public static class LanguageHelpers
    {
        public static string GetLanguage(string key)
        {
            return Language.main.Get(key);
        }
    }
}
