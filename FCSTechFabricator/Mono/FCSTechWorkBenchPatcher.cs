using FCSCommon.Utilities;
using FCSTechFabricator.Interfaces;
using FCSTechFabricator.Models;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FCSTechFabricator.Mono
{
    public partial class FCSTechFabricatorBuildable : CustomFabricator
    {
        public static readonly FCSTechFabricatorBuildable Singleton = new FCSTechFabricatorBuildable();

        public override Models Model => Models.Custom;

        public override string AssetsFolder => $"{Mod.ModFolderName}/Assets";

        private static readonly Dictionary<string, string> AlterraDivisions = new Dictionary<string, string>()
        {
            {"Alterra Industrial Solutions","AIS"},
            {"Alterra Medical Solutions","AMS"},
            {"Alterra Refrigeration Solutions","ARS"},
            {"Alterra Shipping Solutions","ASS"},
            {"Alterra Storage Solutions","ASTS"},
            {"Alterra Electric","AE"}
        };

        internal static readonly Dictionary<string, ModKey> FCSMods = new Dictionary<string, ModKey>()
        {
            {"Marine Turbines",new ModKey{Key = "MT",ParentKey = "AIS"}},
            {"Deep Driller",new ModKey{Key = "DD",ParentKey = "AIS"}},
            {"Power Storage",new ModKey{Key = "PS",ParentKey = "AIS"}},
            {"SeaBreeze",new ModKey{Key = "SB",ParentKey = "ARS"}},
            {"Ex-Storage",new ModKey{Key = "ES",ParentKey = "ASTS"}},
            {"Sea Cooker",new ModKey{Key = "SC",ParentKey = "AE"}},
            {"Mini Fountain Filter",new ModKey{Key = "MFF",ParentKey = "AE"}},
            {"Mini MedBay",new ModKey{Key = "MMB",ParentKey = "AMS"}},
            {"Powercell Socket",new ModKey{Key = "PSS",ParentKey = "AIS"}},
            {"Intra-Base Teleporter",new ModKey{Key = "IBT",ParentKey = "AE"}},
            {"Quantum Teleporter",new ModKey{Key = "QT",ParentKey = "AE"}},
            {"Alterra Shipping",new ModKey{Key = "ASU",ParentKey = "ASS"}},
        };

        public FCSTechFabricatorBuildable() : base(Mod.ModName, Mod.ModFriendly, Mod.ModDescription)
        {
        }

        internal static void PatchHelper()
        {
            CreateCustomTree();
            MakeBuildable();
            Singleton.Patch();
        }

        private static void MakeBuildable()
        {
            CraftDataHandler.SetTechData(Singleton.TechType, GetBlueprintRecipe());
            CraftDataHandler.AddToGroup(GroupForPDA, CategoryForPDA, Singleton.TechType);
            CraftDataHandler.AddBuildable(Singleton.TechType);
        }

        private static void CreateCustomTree()
        {
            QuickLogger.Debug($"Attempting to add {Singleton.ClassID} to nodes");

            foreach (var division in AlterraDivisions)
            {
                QuickLogger.Debug($"Creating tab {division.Key}");
                Singleton.AddTabNode($"{division.Value}", $"{division.Key}", new Atlas.Sprite(ImageUtils.LoadTextureFromFile($"./QMods/{Mod.ModFolderName}/Assets/{division.Value}Icon.png")));
                QuickLogger.Debug($"{division.Key} node tab Created");

                foreach (var fcsMod in FCSMods)
                {
                    var icon = new Atlas.Sprite(ImageUtils.LoadTextureFromFile($"./QMods/{Mod.ModFolderName}/Assets/{fcsMod.Value.Key}Icon.png"));
                    Singleton.AddTabNode(fcsMod.Value.Key, fcsMod.Key, icon);
                    QuickLogger.Debug($"Child node {fcsMod.Key} tab Created");
                }
            }

            Singleton.CreateCustomCraftTree(out CraftTree.Type craftType);
        }

        protected override GameObject GetCustomCrafterPreFab()
        {
            // Instantiate fabricator
            GameObject prefab = GameObject.Instantiate(OriginalFabricator);
            
            // Set the custom texture
            Texture2D coloredTexture = QPatch.Bundle.LoadAsset<Texture2D>("FCSTechFabricator");
            SkinnedMeshRenderer skinnedMeshRenderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMeshRenderer.material.mainTexture = coloredTexture;

            prefab.AddComponent<FCSTechFabController>();

            return prefab;
        }


        private static readonly GameObject OriginalFabricator = Resources.Load<GameObject>("Submarine/Build/Fabricator");

        internal static TechGroup GroupForPDA => TechGroup.InteriorModules;

        internal static TechCategory CategoryForPDA => TechCategory.InteriorModule;
        
        protected static TechData GetBlueprintRecipe()
        {
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Titanium, 1),
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.JeweledDiskPiece, 1),
                    new Ingredient(TechType.Magnetite, 1)
                }
            };

            return customFabRecipe;
        }

        internal static void AddTechType(TechType techType, string[] steps)
        {
            QuickLogger.Debug($"Attempting to add TechType {techType} to {steps[0]}");
            Singleton.AddCraftNode(techType,steps.Last());
            QuickLogger.Debug($"Added TechType {techType} to {steps[0]}");
        }
    }

    internal class ModKey
    {
        public string Key { get; set; }
        public string ParentKey { get; set; }
    }

}
