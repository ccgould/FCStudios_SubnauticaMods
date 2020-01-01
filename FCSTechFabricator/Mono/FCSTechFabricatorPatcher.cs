using System.Collections.Generic;
using System.IO;
using FCSCommon.Utilities;
using FCSTechFabricator.Models;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSTechFabricator.Mono
{
    public class FCSTechFabricatorBuildable : CustomFabricator
    {
        public override Models Model => Models.Fabricator;

        internal const string TabIndustrialSolutions = "AIS";
        internal const string TabMedicalSolutions = "AMS";
        internal const string TabRefrigerationSolutions = "ARS";
        internal const string TabShippingSolutions = "ASS";
        internal const string TabStorageSolutions = "ASTS";
        internal const string TabElectric = "AE";

        internal const string SubTabMarineTurbines = "MT";
        internal const string SubTabDeepDriller = "DD";
        internal const string SubTabPowerStorage = "PS";
        internal const string SubTabSeaBreeze = "SB";
        internal const string SubTabExStorage = "ES";
        internal const string SubTabCooker = "SC";
        internal const string SubTabFountainFilter = "MFF";
        internal const string SubTabMedBay = "MMB";
        internal const string SubTabPowercellSocket = "PSS";
        internal const string SubTabIntraBaseTeleporter = "IBT";
        internal const string SubTabQuantumTeleporter = "QT";
        internal const string SubTabShipping = "ASU";

        private static readonly Dictionary<string, string> MainTabs = new Dictionary<string, string>()
        {
            { TabIndustrialSolutions, "Alterra Industrial Solutions" },
            { TabMedicalSolutions, "Alterra Medical Solutions"},
            { TabRefrigerationSolutions, "Alterra Refrigeration Solutions" },
            { TabShippingSolutions, "Alterra Shipping Solutions" },
            { TabStorageSolutions, "Alterra Storage Solutions" },
            { TabElectric, "Alterra Electric" }
        };

        private static readonly Dictionary<string, string> SubTabs = new Dictionary<string, string>()
        {
            { SubTabMarineTurbines, TabIndustrialSolutions },
            { SubTabDeepDriller, TabIndustrialSolutions },
            { SubTabPowerStorage, TabIndustrialSolutions },
            { SubTabSeaBreeze, TabRefrigerationSolutions },
            { SubTabExStorage, TabStorageSolutions },
            { SubTabCooker, TabElectric },
            { SubTabFountainFilter, TabElectric },
            { SubTabMedBay, TabMedicalSolutions },
            { SubTabPowercellSocket, TabIndustrialSolutions },
            { SubTabIntraBaseTeleporter, TabElectric },
            { SubTabQuantumTeleporter, TabElectric },
            { SubTabShipping, TabShippingSolutions },
        };

        private static readonly Dictionary<string, string> SubTabNames = new Dictionary<string, string>()
        {
            { SubTabMarineTurbines, "Marine Turbines" },
            { SubTabDeepDriller, "Deep Driller" },
            { SubTabPowerStorage, "Power Storage" },
            { SubTabSeaBreeze, "SeaBreeze" },
            { SubTabExStorage, "Ex-Storage" },
            { SubTabCooker, "Sea Cooker" },
            { SubTabFountainFilter, "Mini Fountain Filter" },
            { SubTabMedBay, "Mini MedBay" },
            { SubTabPowercellSocket, "Powercell Socket" },
            { SubTabIntraBaseTeleporter, "Intra-Base Teleporter" },
            { SubTabQuantumTeleporter, "Quantum Teleporter" },
            { SubTabShipping, "Alterra Shipping" },
        };

        public FCSTechFabricatorBuildable() : base(Mod.ModName, "FCS Tech Fabricator", "The place for all your FCStudios mod needs")
        {
            AddCraftingTabs();

            OnFinishedPatching = () =>
            {
                SetBlueprintRecipe();
                AddBuildable();
                AddToGroup();
            };
        }

        private void AddCraftingTabs()
        {
            foreach (KeyValuePair<string, string> mainTab in MainTabs)
            {
                string tabId = mainTab.Key;
                string tabName = mainTab.Value;
                var icon = new Atlas.Sprite(ImageUtils.LoadTextureFromFile(Path.Combine(Mod.GetAssetPath(), $"{tabId}Icon.png")));

                base.AddTabNode(tabId, tabName, icon);
            }

            foreach (KeyValuePair<string, string> subTab in SubTabs)
            {
                string tabId = subTab.Key;
                string parentTab = subTab.Value;
                string tabName = SubTabNames[tabId];
                var icon = new Atlas.Sprite(ImageUtils.LoadTextureFromFile(Path.Combine(Mod.GetAssetPath(),$"{tabId}Icon.png")));

                base.AddTabNode(tabId, tabName, icon, parentTab);
            }
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = base.GetGameObject();

            // Set the custom texture
            Texture2D coloredTexture = QPatch.Bundle.LoadAsset<Texture2D>($"{Mod.ModName}");
            SkinnedMeshRenderer skinnedMeshRenderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMeshRenderer.material.mainTexture = coloredTexture;
            return prefab;
        }

        public override string AssetsFolder { get; } = $"{Mod.GetAssetPath()}";

        private void SetBlueprintRecipe()
        {
            CraftDataHandler.SetTechData(this.TechType, new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Titanium, 1),
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.JeweledDiskPiece, 1),
                    new Ingredient(TechType.Magnetite, 1)
                }
            });
        }

        private void AddToGroup()
        {
            CraftDataHandler.AddToGroup(TechGroup.InteriorModules, TechCategory.InteriorModule, TechType);
        }

        private void AddBuildable()
        {
            CraftDataHandler.AddBuildable(TechType);
        }
    }
    
    internal class ModKey
    {
        public string Key { get; set; }
        public string ParentKey { get; set; }
    }
}
