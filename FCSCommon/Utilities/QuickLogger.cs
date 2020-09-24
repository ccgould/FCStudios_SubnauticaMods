namespace FCSCommon.Utilities
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    //Created by PrimeSonic GitHub repo: https://github.com/PrimeSonic/PrimeSonicSubnauticaMods

    internal static class QuickLogger
    {
        internal static bool DebugLogsEnabled = false;

        internal static void Info(string msg, bool showOnScreen = false)
        {
            string name = Assembly.GetCallingAssembly().GetName().Name;

            Console.WriteLine($"[{name}:INFO] {msg}");

            if (showOnScreen)
                ErrorMessage.AddMessage(msg);
        }

        internal static void Message(string msg, bool showOnScreen = false)
        {
            string name = Assembly.GetCallingAssembly().GetName().Name;

            Console.WriteLine($"[{name}] : {msg}");

            if (showOnScreen)
                ErrorMessage.AddMessage(msg);
        }

        internal static void Debug(string msg, bool showOnScreen = false)
        {
            if (!DebugLogsEnabled)
                return;

            string name = Assembly.GetCallingAssembly().GetName().Name;

            Console.WriteLine($"[{name}:DEBUG] {msg}");

            if (showOnScreen)
                ErrorMessage.AddDebug(msg);

        }

        internal static void Error(string msg, bool showOnScreen = false)
        {
            string name = Assembly.GetCallingAssembly().GetName().Name;

            Console.WriteLine($"[{name}:ERROR] {msg}");

            if (showOnScreen)
                ErrorMessage.AddError(msg);
        }

        internal static void Error<T>(string msg, bool showOnScreen = false)
        {
            string name = Assembly.GetCallingAssembly().GetName().Name;

            Console.WriteLine($"[{name}:ERROR] {typeof(T).FullName}: {msg}");

            if (showOnScreen)
                ErrorMessage.AddError(msg);
        }

        internal static void Error(string msg, Exception ex)
        {
            string name = Assembly.GetCallingAssembly().GetName().Name;

            Console.WriteLine($"[{name}:ERROR] {msg}{Environment.NewLine}{ex.ToString()}");
        }

        internal static void Error(Exception ex)
        {
            string name = Assembly.GetCallingAssembly().GetName().Name;

            Console.WriteLine($"[{name}:ERROR] {ex.ToString()}");
        }

        internal static void Warning(string msg, bool showOnScreen = false)
        {
            string name = Assembly.GetCallingAssembly().GetName().Name;

            Console.WriteLine($"[{name}:WARN] {msg}");

            if (showOnScreen)
                ErrorMessage.AddWarning(msg);
        }

        internal static string GetAssemblyVersion() => GetAssemblyVersion(Assembly.GetExecutingAssembly());

        internal static string GetAssemblyVersion(Assembly assembly)
        {
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return FormatToSimpleVersion(fvi);
        }

        private static string FormatToSimpleVersion(FileVersionInfo version) => $"{version.FileMajorPart}.{version.FileMinorPart}.{version.FileBuildPart}";

        public static void ModMessage(string msg)
        {
            string name = Assembly.GetCallingAssembly().GetName().Name;
            Console.WriteLine($"[{name}] {msg}");
            ErrorMessage.AddMessage($"[{name}] {msg}");
        }
    }
}
