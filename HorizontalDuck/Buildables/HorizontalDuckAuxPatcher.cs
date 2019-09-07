using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;
using System;

namespace HorizontalDuck.Buildables
{
    internal partial class HorizontalDuckBuildable
    {
        private void AdditionalPatching()
        {
            QuickLogger.Debug("Additional Properties");
            LanguageHandler.SetLanguageLine(HDResizeMessageKey, "Hold 'E' and click to adjust size" + Environment.NewLine);
        }

        private const string HDResizeMessageKey = "HD_ResizeMessage";

        public static string ResizeMessage()
        {
            return Language.main.Get(HDResizeMessageKey);
        }
    }
}
