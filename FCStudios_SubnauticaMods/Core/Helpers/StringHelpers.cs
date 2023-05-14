using System;
using System.Text;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using FCSCommon.Utilities;

namespace FCS_AlterraHub.Core.Helpers;

public static class StringHelpers
{
    private static readonly StringBuilder Sb = new();
    public static string TruncateWEllipsis(this string value, int maxChars)
    {
        if (value == null)
        {
            QuickLogger.Error($"[TruncateWEllipisis] : String is null");
            return string.Empty;
        }
        return value.Length <= maxChars ? value : value.Substring(0, maxChars - 3) + "...";
    }

    public static string CombineWithNewLine(string[] strings)
    {
        Sb.Clear();
        if (strings == null) return string.Empty;
        CreateText(strings);
        return Sb.ToString();
    }

    public static string CombineWithNewLineEx(this string[] strings)
    {
        Sb.Clear();
        if (strings == null) return string.Empty;
        CreateText(strings);
        return Sb.ToString();
    }

    public static void HandHoverPDAHelper(this string[] strings)
    {
        var main = HandReticle.main;
        main.SetIcon(HandReticle.IconType.Info);

        Sb.Clear();

        if (strings == null || main == null) return;
        CreateText(strings);
        main.SetTextRaw(HandReticle.TextType.Hand, $"{Sb}/n {LanguageService.ViewInPDA()}");

    }

    public static void HandHoverPDAHelperEx(this string[] strings, TechType techType, HandReticle.IconType icon = HandReticle.IconType.Info, float progess = 0f)
    {
        var main = HandReticle.main;
        var pda = FCSPDAController.Main;

        QuickLogger.Debug($"PDA Is Null Check {pda is null}",true);

        if (strings == null || main == null) return;

        QuickLogger.Debug("1");

        var text = pda?.Screen?.CheckIfPDAHasEntry(techType) ?? false ? LanguageService.ViewInPDA() : string.Empty;

        QuickLogger.Debug("2");

        CreateText(strings);

        QuickLogger.Debug("3");

        main.SetText(HandReticle.TextType.Hand, Sb.ToString(), false, GameInput.Button.None);

        QuickLogger.Debug("4");

        main.SetText(HandReticle.TextType.HandSubscript, text, false, GameInput.Button.None);

        QuickLogger.Debug("5");

        main.SetIcon(icon, 1f);

        QuickLogger.Debug("6");


        Sb.Clear();
        QuickLogger.Debug("1");

        if (icon == HandReticle.IconType.Progress)
        {
            main.SetProgress(progess);
        }
    }

    public static void HandHoverPDAHelperEx(this string[] strings, string techType, HandReticle.IconType icon = HandReticle.IconType.Info, float progess = 0f)
    {
        var main = HandReticle.main;
        var pda = FCSPDAController.Main;

        if (strings == null || main == null) return;

        main.SetIcon(icon);

        Sb.Clear();

        Sb.Append($"{Language.main.Get(techType)}: ");

        CreateText(strings);

        var text = pda?.Screen.CheckIfPDAHasEntry(techType) ?? false ? LanguageService.ViewInPDA() : string.Empty;
        main.SetTextRaw(HandReticle.TextType.Count, $"{Sb}/n {text}");


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
            Sb.Append(s);
            if (strings.Length == 1 || i == strings.Length - 1) continue;
            Sb.Append(Environment.NewLine);
        }
    }
}
