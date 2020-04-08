using System.IO;
using FCSTechFabricator.Components;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSTechFabricator.Configuration
{
    public static class DefaultConfigurations
    {
        private static CraftingTab _dnaTab;
        public static Color DefaultColor => new Color(0.7529412f, 0.7529412f, 0.7529412f, 1f);

        public static CraftingTab DnaTab => GetDnaTab();

        private static CraftingTab GetDnaTab()
        {
            if (_dnaTab == null)
            {
                var icon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "HydroHarv.png"));
                _dnaTab = new CraftingTab("DNASample", "DNA Samples", icon);
            }

            return _dnaTab;
        }
    }
}
