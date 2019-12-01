using FCSCommon.Utilities;
using FCSTechFabricator.Interfaces;
using FCSTechFabricator.Models;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FCSTechFabricator.Mono
{
    public partial class FCSTechFabricatorBuildable : Buildable
    {
        public static readonly FCSTechFabricatorBuildable Singleton = new FCSTechFabricatorBuildable();

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
            {"Alterra Shipping",new ModKey{Key = "ASU",ParentKey = "ASS"}},
        };


        internal static void AddMod(string divisionName, string divisionKey, string ModName, string ModKey)
        {
            if (!AlterraDivisions.ContainsKey(divisionName))
            {
                AlterraDivisions.Add(divisionName,divisionKey);
            }

            if (!FCSMods.ContainsKey(ModName))
            {
                FCSMods.Add(ModName,new ModKey{Key = ModKey, ParentKey = divisionKey});
            }

        }

        public FCSTechFabricatorBuildable() : base(Mod.ModName, "FCS Tech Fabricator", "The place for all your FCStudios mod needs")
        {
        }

        internal static void PatchHelper()
        {
            CreateCustomTree();
            Singleton.Patch();
        }

        private static void CreateCustomTree()
        {
            _root = CraftTreeHandler.CreateCustomCraftTreeAndType("FCSTechFabricator", out CraftTree.Type craftType);

            TechFabricatorCraftTreeType = craftType;

            QuickLogger.Debug($"Attempting to add {Singleton.ClassID} to nodes");

            foreach (var division in AlterraDivisions)
            {
                if (_root.GetNode($"{division.Value}") == null)
                {
                    QuickLogger.Debug($"{division.Key} is null creating tab");
                    var itemTab = _root.AddTabNode($"{division.Value}", $"{division.Key}", new Atlas.Sprite(ImageUtils.LoadTextureFromFile($"./QMods/{Mod.ModFolderName}/Assets/{division.Value}Icon.png")));
                    QuickLogger.Debug($"{division.Key} node tab Created");

                    foreach (var fcsMod in FCSMods)
                    {
                        if (fcsMod.Value.ParentKey == division.Value)
                        {
                            var icon = new Atlas.Sprite(ImageUtils.LoadTextureFromFile($"./QMods/{Mod.ModFolderName}/Assets/{fcsMod.Value.Key}Icon.png"));
                            itemTab.AddTabNode(fcsMod.Value.Key, fcsMod.Key, icon);
                            QuickLogger.Debug($"Child node {fcsMod.Key} tab Created");
                        }
                    }
                }
            }

            foreach (var childNode in _root.ChildNodes)
            {
                QuickLogger.Debug(childNode.Name);
            }
        }

        public override GameObject GetGameObject()
        {
            // Instantiate fabricator
            GameObject prefab = GameObject.Instantiate(OriginalFabricator);

            // Update prefab ID
            var prefabId = prefab.GetComponent<PrefabIdentifier>();
            prefabId.name = this.PrefabFileName;

            // Update tech tag
            var techTag = prefab.GetComponent<TechTag>();
            techTag.type = TechType;

            // Associate craft tree to the fabricator
            var fabricator = prefab.GetComponent<Fabricator>();
            fabricator.craftTree = TechFabricatorCraftTreeType;

            var ghost = fabricator.GetComponent<GhostCrafter>();
            var powerRelay = new PowerRelay();
            // Ignore any errors you see about this fabricator not having a power relay in its parent. It does and it works.
            FieldInfo fieldInfo = typeof(GhostCrafter).GetField("powerRelay", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(ghost, powerRelay);

            // Set where it can be built
            var constructible = prefab.GetComponent<Constructable>();
            constructible.allowedInBase = true;
            constructible.allowedInSub = true;
            constructible.allowedOutside = false;
            constructible.allowedOnCeiling = false;
            constructible.allowedOnGround = false;
            constructible.allowedOnConstructables = false;
            constructible.controlModelState = true;
            constructible.allowedOnWall = true;
            constructible.techType = TechType;

            // Set the custom texture
            Texture2D coloredTexture = QPatch.Bundle.LoadAsset<Texture2D>("FCSTechFabricator");
            SkinnedMeshRenderer skinnedMeshRenderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMeshRenderer.material.mainTexture = coloredTexture;

            prefab.AddComponent<FCSTechFabController>();

            return prefab;

        }

        /// <summary>
        /// This is the CraftTree.Type for the FCS Tech Fabricator.
        /// </summary> 
        public static CraftTree.Type TechFabricatorCraftTreeType { get; private set; }

        private static readonly GameObject OriginalFabricator = Resources.Load<GameObject>("Submarine/Build/Fabricator");
        private static ModCraftTreeRoot _root;

        //internal static List<FCSTechFabricatorItem> ItemsList = new List<FCSTechFabricatorItem>();

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;

        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;

        public override string AssetsFolder { get; } = $"{Mod.ModFolderName}/Assets";
        protected override TechData GetBlueprintRecipe()
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

            if (_root != null)
            {
                var tab = _root.GetTabNode(steps);
                if (tab.GetCraftingNode(techType) == null)
                {
                    //tab.AddModdedCraftingNode("Freon_ARS");
                    tab.AddCraftingNode(techType);
                    QuickLogger.Debug($"Added TechType {techType} to {steps[0]}");
                }
            }
        }
    }

    internal class ModKey
    {
        public string Key { get; set; }
        public string ParentKey { get; set; }
    }

}
