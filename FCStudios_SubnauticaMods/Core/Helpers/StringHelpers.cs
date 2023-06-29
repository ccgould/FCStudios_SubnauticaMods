using FCSCommon.Utilities;

namespace FCS_AlterraHub.Core.Helpers;

public static class StringHelpers
{
    public static string TruncateWEllipsis(this string value, int maxChars)
    {
        if (value == null)
        {
            QuickLogger.Error($"[TruncateWEllipisis] : String is null");
            return string.Empty;
        }
        return value.Length <= maxChars ? value : value.Substring(0, maxChars - 3) + "...";
    }

    //public static string CombineWithNewLine(string[] strings)
    //{
    //    Sb.Clear();
    //    if (strings == null) return string.Empty;
    //    CreateText(strings);
    //    return Sb.ToString();
    //}

    //public static string CombineWithNewLineEx(this string[] strings)
    //{
    //    Sb.Clear();
    //    if (strings == null) return string.Empty;
    //    CreateText(strings);
    //    return Sb.ToString();
    //}

    //public static void HandHoverPDAHelper(this string[] strings)
    //{
    //    var main = HandReticle.main;
    //    main.SetIcon(HandReticle.IconType.Info);

    //    Sb.Clear();

    //    if (strings == null || main == null) return;
    //    CreateText(strings);
    //    main.SetTextRaw(HandReticle.TextType.Hand, $"{Sb}/n {LanguageService.ViewInPDA()}");

    //}

    
    //public static void HandHoverPDAHelperEx(this string[] strings, string techType, HandReticle.IconType icon = HandReticle.IconType.Info, float progess = 0f)
    //{
    //    var main = HandReticle.main;
    //    var pda = FCSPDAController.Main;

    //    if (strings == null || main == null) return;

    //    main.SetIcon(icon);

    //    Sb.Clear();

    //    Sb.Append($"{Language.main.Get(techType)}: ");

    //    CreateText(strings);

    //    var text = pda?.GetGUI().CheckIfPDAHasEntry(techType) ?? false ? LanguageService.ViewInPDA() : string.Empty;
    //    main.SetTextRaw(HandReticle.TextType.Count, $"{Sb}/n {text}");


    //    if (icon == HandReticle.IconType.Progress)
    //    {
    //        main.SetProgress(progess);
    //    }
    //}
}
