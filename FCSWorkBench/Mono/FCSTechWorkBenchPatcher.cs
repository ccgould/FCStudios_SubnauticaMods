using FCSCommon.Utilities;
using FCSTechWorkBench.Abstract_Classes;
using FCSTechWorkBench.Interfaces;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FCSTechWorkBench.Mono
{
    public partial class FCSTechWorkBenchBuildable : Buildable
    {
        public static readonly FCSTechWorkBenchBuildable Singleton = new FCSTechWorkBenchBuildable();

        public FCSTechWorkBenchBuildable() : base("FCSTechWorkBench", "FCS Tech WorkBench", "The place for all your FCStudios mod needs")
        {
        }

        internal static void PatchHelper()
        {
            CreateCustomTree();
            Singleton.Patch();
        }

        private static void CreateCustomTree()
        {
            var root = CraftTreeHandler.CreateCustomCraftTreeAndType("FCSTechWorkBench", out CraftTree.Type craftType);
            ModCraftTreeTab itemTab = null;

            Singleton.AlienChiefTreeType = craftType;

            QuickLogger.Debug($"Attempting to add {Singleton.ClassID} to nodes");

            foreach (var item in ItemsList)
            {

                if (string.IsNullOrEmpty(item.ClassID_I))
                {
                    QuickLogger.Debug("Item ClassID_I was null");
                    return;
                }

                QuickLogger.Debug("In foreach");

                if (item.ClassID_I.EndsWith("_ARS"))
                {
                    QuickLogger.Debug($"Added {item.ClassID_I} to nodes");
                    AddTabNodes(ref root, ref itemTab, item, "ARSolutions", "ARSIcon");
                }
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
            var fabricator = prefab.GetComponent<Workbench>();
            fabricator.craftTree = AlienChiefTreeType;

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
            constructible.allowedOnGround = true;
            constructible.allowedOnConstructables = false;
            constructible.controlModelState = true;
            constructible.allowedOnWall = false;
            constructible.techType = TechType;

            // Set the custom texture
            Texture2D coloredTexture = QPatch.Bundle.LoadAsset<Texture2D>("FCSWorkbench");
            SkinnedMeshRenderer skinnedMeshRenderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMeshRenderer.material.mainTexture = coloredTexture;
            return prefab;

        }

        private static void AddTabNodes(ref ModCraftTreeRoot root, ref ModCraftTreeTab itemTab, IFCSTechWorkBenchItem fcsTechWorkBenchItem, string category, string icon)
        {

            if (root.GetNode($"{category}") == null)
            {
                QuickLogger.Debug($"{category} is null creating tab");
                itemTab = root.AddTabNode($"{category}", $"FCS Work Bench {category}", new Atlas.Sprite(ImageUtils.LoadTextureFromFile($"./QMods/FCSTechWorkBench/Assets/{icon}.png")));
                itemTab?.AddCraftingNode(fcsTechWorkBenchItem.TechTypeID);
                QuickLogger.Debug($"{category} node tab Created");
            }
            else
            {
                QuickLogger.Debug($"{category} is not null creating node tab");
                itemTab?.AddCraftingNode(fcsTechWorkBenchItem.TechTypeID);
            }
        }

        /// <summary>
        /// This is the CraftTree.Type for the AlienChief fabricator.
        /// </summary>
        public CraftTree.Type AlienChiefTreeType { get; private set; }

        private static readonly GameObject OriginalFabricator = Resources.Load<GameObject>("Submarine/Build/Workbench");

        internal static List<FCSTechWorkBenchItem> ItemsList = new List<FCSTechWorkBenchItem>();

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;

        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;

        public override string AssetsFolder { get; } = "FCSTechWorkBench/Assets";
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


    }
}
