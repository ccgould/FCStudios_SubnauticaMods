using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Oculus.Newtonsoft.Json;

namespace FCSCommon.Utilities.Language
{
    public static class LanguageSystem
    {
        public static CultureInfo CultureInfo { get; private set; }

        /// <summary>
        /// Loads a language file into a list of a type of T
        /// </summary>
        /// <typeparam name="T">Type for the list.</typeparam>
        /// <param name="region">The region to locate.</param>
        /// <param name="languagesFilePath">The path to the language file</param>
        /// <returns></returns>
        public static List<T> LoadCurrentRegion<T>(string languagesFilePath)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(languagesFilePath));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        /// <summary>
        /// Gets the current system language
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentSystemLanguageInfo()
        {
            StringBuilder sb = new StringBuilder();

           CultureInfo = CultureInfo.CurrentUICulture;

            sb.Append("Default Language Info:");
            sb.Append(Environment.NewLine);
            sb.Append($"* Name: {CultureInfo.Name}");
            sb.Append(Environment.NewLine);
            sb.Append($"* Display Name: {CultureInfo.DisplayName}");
            sb.Append(Environment.NewLine);
            sb.Append($"* English Name: {CultureInfo.EnglishName}");
            sb.Append(Environment.NewLine);
            sb.Append($"* 2-letter ISO Name: {CultureInfo.TwoLetterISOLanguageName}");
            sb.Append(Environment.NewLine);
            sb.Append($"* 3-letter ISO Name: {CultureInfo.ThreeLetterISOLanguageName}");
            sb.Append(Environment.NewLine);
            sb.Append($"* 3-letter Win32 API Name: {CultureInfo.ThreeLetterWindowsLanguageName}");

            return sb.ToString();
        }
    }
}
