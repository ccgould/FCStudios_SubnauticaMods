
namespace FCSCommon.Utilities;

using BepInEx.Logging;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

//Created by PrimeSonic GitHub repo: https://github.com/PrimeSonic/PrimeSonicSubnauticaMods

internal static class QuickLogger
{
    internal static bool DebugLogsEnabled = false;
    internal static string ModName = "";
    private static ManualLogSource _logger;
    internal static bool HideAllFCSOnScreenMessages;
    internal static bool ShowCreditMessages;
    
    internal static void Initialize()
    {
        if (_logger == null)
        {
            _logger = Logger.CreateLogSource(FCS_AlterraHub.PluginInfo.PLUGIN_NAME);
        }
    }

    internal static void Info(string msg, bool showOnScreen = false)
    {
        if (HideAllFCSOnScreenMessages) return;
        Initialize();
        string name = Assembly.GetCallingAssembly().GetName().Name;

        _logger.LogInfo($"[{name}:INFO] {msg}");

        if (showOnScreen)
            AddMessage(msg);
    }

    internal static void Message(string msg, bool showOnScreen = false)
    {
        if (HideAllFCSOnScreenMessages) return;
        Initialize();
        string name = Assembly.GetCallingAssembly().GetName().Name;

        _logger.LogMessage($"[{name}] : {msg}");

        if (showOnScreen) 
            AddMessage(msg);
    }

    internal static void Debug(string msg, bool showOnScreen = false)
    {
        Initialize();
        if (!DebugLogsEnabled)
            return;

        string name = Assembly.GetCallingAssembly().GetName().Name;

        _logger.LogInfo($"[{name}:DEBUG] {msg}");

        if (showOnScreen)
            AddDebug(msg);

    }

    internal static void Error(string msg, bool showOnScreen = false)
    {
        Initialize();
        string name = Assembly.GetCallingAssembly().GetName().Name;

        _logger.LogError( $"[{name}:ERROR] {msg}");

        if (showOnScreen)
            AddError(msg);
    }

    internal static void Error<T>(string msg, bool showOnScreen = false)
    {
        Initialize();
        string name = Assembly.GetCallingAssembly().GetName().Name;

        _logger.LogError($"[{name}:ERROR] {typeof(T).FullName}: {msg}");

        if (showOnScreen)
            AddError(msg);
    }

    internal static void Error(string msg, Exception ex)
    {
        Initialize();
        string name = Assembly.GetCallingAssembly().GetName().Name;

        _logger.LogError($"[{name}:ERROR] {msg}{Environment.NewLine}{ex.ToString()}");
    }

    internal static void Error(Exception ex)
    {
        Error(ex.ToString());
    }

    internal static void Warning(string msg, bool showOnScreen = false)
    {
        if (HideAllFCSOnScreenMessages) return;
        Initialize();
        string name = Assembly.GetCallingAssembly().GetName().Name;

        _logger.LogWarning($"[{name}:WARN] {msg}");

        if (showOnScreen)
            AddWarning(msg);
    }

    

    internal static string GetAssemblyVersion() => GetAssemblyVersion(Assembly.GetExecutingAssembly());

    internal static string GetAssemblyVersion(Assembly assembly)
    {
        var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        return FormatToSimpleVersion(fvi);
    }

    private static string FormatToSimpleVersion(FileVersionInfo version) => $"{version.FileMajorPart}.{version.FileMinorPart}.{version.FileBuildPart}.{version.FilePrivatePart}";

    public static void CreditMessage(string msg)
    {
        if (!ShowCreditMessages) return;
        Initialize();
        string name = Assembly.GetCallingAssembly().GetName().Name;
        _logger.LogMessage($"[{name}] {msg}");
        AddMessage($"[{name}] {msg}");
    }        
    
    public static void ModMessage(string msg)
    {
        if (HideAllFCSOnScreenMessages) return;
        Initialize();
        string name = Assembly.GetCallingAssembly().GetName().Name;
        _logger.LogMessage($"[{name}] {msg}");
        AddMessage($"[{name}] {msg}");
    }



    public static void DebugError(string msg, bool showOnScreen = false)
    {
        Initialize();
        if (!DebugLogsEnabled)
            return;
        string name = Assembly.GetCallingAssembly().GetName().Name;

        _logger.LogError($"[{name}:DEBUG_ERROR] {msg}");

        if (showOnScreen)
            AddError($"[{name}:DEBUG_ERROR] {msg}");
    }

    private static void AddError(string message)
    {
        ErrorMessage.AddError(message);
    }

    private static void AddMessage(string message)
    {
        ErrorMessage.AddMessage(message);
    }

    private static void AddWarning(string message)
    {
        ErrorMessage.AddWarning(message);
    }

    private static void AddDebug(string message)
    {
        ErrorMessage.AddDebug(message);
    }
}

internal class FCSLog
{
    public string DateTimeString { get; set; }
    public string SenderName { get; set; }
    public string SenderType { get; set; }
    public string Value { get; set; }          

    public FCSLog(string senderName,Type sender,string value = null) 
    {
        DateTimeString = DateTime.Now.ToString();
        SenderName = senderName;
        SenderType = sender.Name;
        Value = value;

    }
}
