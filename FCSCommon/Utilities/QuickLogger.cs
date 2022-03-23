using BepInEx.Logging;
using FCS_AlterraHub;
using QModManager.API;

namespace FCSCommon.Utilities
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    //Created by PrimeSonic GitHub repo: https://github.com/PrimeSonic/PrimeSonicSubnauticaMods

    internal static class QuickLogger
    {
        internal static bool DebugLogsEnabled = false;
        internal static string ModName = "";
        private static ManualLogSource _logger;


        internal static void Initialize()
        {
            if (_logger == null)
            {
                _logger = Logger.CreateLogSource(QModServices.Main.GetMyMod().DisplayName);
            }
        }

        internal static void Info(string msg, bool showOnScreen = false)
        {
            if (QPatch.Configuration.HideAllFCSOnScreenMessages) return;
            Initialize();
            string name = Assembly.GetCallingAssembly().GetName().Name;

            _logger.LogInfo($"[{name}:INFO] {msg}");

            if (showOnScreen)
                ErrorMessage.AddMessage(msg);
        }

        internal static void Message(string msg, bool showOnScreen = false)
        {
            if (QPatch.Configuration.HideAllFCSOnScreenMessages) return;
            Initialize();
            string name = Assembly.GetCallingAssembly().GetName().Name;

            _logger.LogMessage($"[{name}] : {msg}");

            if (showOnScreen)
                ErrorMessage.AddMessage(msg);
        }

        internal static void Debug(string msg, bool showOnScreen = false)
        {
            Initialize();
            if (!DebugLogsEnabled)
                return;

            string name = Assembly.GetCallingAssembly().GetName().Name;

            _logger.LogInfo($"[{name}:DEBUG] {msg}");

            if (showOnScreen)
                ErrorMessage.AddDebug(msg);

        }

        internal static void Error(string msg, bool showOnScreen = false)
        {
            Initialize();
            string name = Assembly.GetCallingAssembly().GetName().Name;

            _logger.LogError( $"[{name}:ERROR] {msg}");

            if (showOnScreen)
                ErrorMessage.AddError(msg);
        }

        internal static void Error<T>(string msg, bool showOnScreen = false)
        {
            Initialize();
            string name = Assembly.GetCallingAssembly().GetName().Name;

            _logger.LogError($"[{name}:ERROR] {typeof(T).FullName}: {msg}");

            if (showOnScreen)
                ErrorMessage.AddError(msg);
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
            if (QPatch.Configuration.HideAllFCSOnScreenMessages) return;
            Initialize();
            string name = Assembly.GetCallingAssembly().GetName().Name;

            _logger.LogWarning($"[{name}:WARN] {msg}");

            if (showOnScreen)
                ErrorMessage.AddWarning(msg);
        }

        internal static string GetAssemblyVersion() => GetAssemblyVersion(Assembly.GetExecutingAssembly());

        internal static string GetAssemblyVersion(Assembly assembly)
        {
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return FormatToSimpleVersion(fvi);
        }

        private static string FormatToSimpleVersion(FileVersionInfo version) => $"{version.FileMajorPart}.{version.FileMinorPart}.{version.FileBuildPart}.{version.FilePrivatePart}";

        public static void ModMessage(string msg)
        {
            if (QPatch.Configuration.HideAllFCSOnScreenMessages) return;
            Initialize();
            string name = Assembly.GetCallingAssembly().GetName().Name;
            _logger.LogMessage($"[{name}] {msg}");
            ErrorMessage.AddMessage($"[{name}] {msg}");
        }

        public static void DebugError(string msg, bool showOnScreen = false)
        {
            Initialize();
            if (!DebugLogsEnabled)
                return;
            string name = Assembly.GetCallingAssembly().GetName().Name;

            _logger.LogError($"[{name}:DEBUG_ERROR] {msg}");

            if (showOnScreen)
                ErrorMessage.AddError($"[{name}:DEBUG_ERROR] {msg}");
        }
    }
}
