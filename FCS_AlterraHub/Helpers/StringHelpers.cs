using System;
using System.Text;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCSCommon.Utilities;

namespace FCS_AlterraHub.Helpers
{
    public static class StringHelpers
    {
        private static StringBuilder _sb = new();
        public static string TruncateWEllipsis(this string value, int maxChars)
        {
            if(value == null)
            {
                QuickLogger.Error($"[TruncateWEllipisis] : String is null");
                return string.Empty;
            }
            return value.Length <= maxChars ? value : value.Substring(0, maxChars - 3) + "...";
        }

        public static string CombineWithNewLine(string[] strings)
        {
            _sb.Clear();
            if (strings == null) return string.Empty;
            CreateText(strings);
            return _sb.ToString();
        }

        public static string CombineWithNewLineEx(this string[] strings)
        {
            _sb.Clear();
            if (strings == null) return string.Empty;
            CreateText(strings);
            return _sb.ToString();
        }
        
        public static void HandHoverPDAHelper(this string[] strings)
        {
            var main = HandReticle.main;
            main.SetIcon(HandReticle.IconType.Info);

            _sb.Clear();

            if (strings == null || main == null) return;
            CreateText(strings);
            main.SetInteractTextRaw(_sb.ToString(), AlterraHub.ViewInPDA());
        }

        public static void HandHoverPDAHelperEx(this string[] strings,TechType techType, HandReticle.IconType icon = HandReticle.IconType.Info, float progess = 0f)
        {
            var main = HandReticle.main;
            main.SetIcon(icon);

            _sb.Clear();

            if (strings == null || main == null) return;
            _sb.Append($"{Language.main.Get(techType)}: ");
            CreateText(strings);
            
            main.SetInteractTextRaw(_sb.ToString(), FCSPDAController.Instance.CheckIfPDAHasEntry(techType) ? AlterraHub.ViewInPDA() : string.Empty);
            
            if (icon == HandReticle.IconType.Progress)
            {
                main.SetProgress(progess);
            }
        }

        private static void CreateText(string[] strings)
        {
            for (var i = 0; i < strings.Length; i++)
            {
                string s = strings[i];
                _sb.Append(s);
                if (strings.Length == 1 || i == strings.Length - 1) continue;
                _sb.Append(Environment.NewLine);
            }
        }
    }
}
