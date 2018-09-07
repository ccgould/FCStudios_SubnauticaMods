using System.Collections.Generic;
using System.Reflection;
using FCSAlienChief.Data;
using FCSAlienChief.Helpers;
using FCSAlienChief.Model;
using SMLHelper;
using SMLHelper.Patchers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSAlienChief.Items
{
    public class CustomFabricator : ModPrefab
    {
        // This is the original fabricator prefab.
        private static readonly GameObject OriginalFabricator = Resources.Load<GameObject>("Submarine/Build/Fabricator");

        private static string _classId;
        private List<IAlienChiefItem> _alienChiefItems;


        #region Alien Chief fabricator

        /// <summary>
        /// This is the CraftTree.Type for the AlienChief fabricator.
        /// </summary>
        public static CraftTree.Type AlienChiefTreeType { get; private set; }


        /// <summary>
        /// This name will be used as ID for the decorations fabricator TechType and its associated CraftTree.Type.
        /// </summary>
        public static string AlienChiefFabId;



        public CustomFabricator(List<IAlienChiefItem> alienChiefItems, string classId, TechType techType = TechType.None) : base(classId, $"{classId}Prefab", techType)
        {
            _alienChiefItems = alienChiefItems;
            AlienChiefFabId = classId;
        }


        /// <summary>
        /// Registers the AlienChief Fabricator
        /// </summary>
        /// <param name="alienChiefItems">A list of IAlienItems</param>
        /// 
        public void RegisterAlienChiefFabricator()
        {
            
            Log.Info("Creating alienchief craft tree...");


            CreateCustomTree( _alienChiefItems);

            Log.Info("Registering alienchief fabricator...");
            // Create a new TechType for the fabricator
            TechType = TechTypeHandler.AddTechType(AlienChiefFabId, "Alien Chief Fabricator",
                "An Alien Chief Fabricator", AssetHelper.Asset.LoadAsset<Sprite>("fabricator_icon_orange"));

            Log.Info("Adding new TechType to the buildables...");
            // Add new TechType to the buildables (Interior Module group)
            CraftDataHandler.AddBuildable(TechType);
            CraftDataHandler.AddToGroup(TechGroup.InteriorModules, TechCategory.InteriorModule, TechType);

            Log.Info(" Creating and associate recipe to the new TechType...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Titanium, 1),
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.Diamond, 1),
                    new Ingredient(TechType.Magnetite, 1)
                }
            };
            Log.Info("Setting TechData...");
            CraftDataHandler.SetTechData(TechType, customFabRecipe);

            Log.Info("Setting buildable prefab...");
            // Set buildable prefab
            PrefabHandler.RegisterPrefab(this);

        }

        private static void CreateCustomTree(List<IAlienChiefItem> alienChiefItems)
        {

            var root = CraftTreeHandler.CreateCustomCraftTreeAndType(AlienChiefFabId, out CraftTree.Type craftType);
            ModCraftTreeTab itemTab = null;


            AlienChiefTreeType = craftType;

            Log.Info($"Attemping to add {_classId} to nodes");
            foreach (var alienChiefItem in alienChiefItems)
            {

                if (alienChiefItem.ClassID_I.EndsWith("R"))
                {
                    Log.Info($"Added {_classId} to nodes");
                    AddTabNodes(ref root, ref itemTab, alienChiefItem, "Resources", "FCSResourcesTab");
                }
                else if (alienChiefItem.ClassID_I.EndsWith("C"))
                {
                    AddTabNodes(ref root, ref itemTab, alienChiefItem, "Condiments", "CondimentsTab");
                }
                else if (alienChiefItem.ClassID_I.EndsWith("F"))
                {
                    AddTabNodes(ref root, ref itemTab, alienChiefItem, "Foods", "Default");
                }
                else if (alienChiefItem.ClassID_I.EndsWith("D"))
                {
                    AddTabNodes(ref root, ref itemTab, alienChiefItem, "Drinks", "DrinkTab");
                }



            }

            #region Omit
            //switch (_category)
            //{
            //    case "Food":
            //        var foodTab = root.AddTabNode("FCSAlienChiefFood", "FCS Alien Food", new Atlas.Sprite(ImageUtils.LoadTextureFromFile($"./QMods/{Information.ModName}/Assets/Default.png")));

            //        AlienChiefTreeType = craftType;

            //        foreach (var alienChiefItem in alienChiefItems)
            //        {
            //            foodTab.AddCraftingNode(alienChiefItem.TechType_I);
            //        }
            //        break;

            //    case "Drinks":

            //        var drinksTab = root.AddTabNode("FCSAlienChiefDrinks", "FCS Alien Drinks", new Atlas.Sprite(ImageUtils.LoadTextureFromFile($"./QMods/{Information.ModName}/Assets/DrinkTab.png")));

            //        AlienChiefTreeType = craftType;

            //        foreach (var alienChiefItem in alienChiefItems)
            //        {
            //            drinksTab.AddCraftingNode(alienChiefItem.TechType_I);
            //        }
            //        break;

            //    case "Resources":

            //        var resourcesTab = root.AddTabNode("FCSAlienChiefResources", "FCS Alien Resources", new Atlas.Sprite(ImageUtils.LoadTextureFromFile($"./QMods/{Information.ModName}/Assets/FCSResourcesTab.png")));

            //        AlienChiefTreeType = craftType;

            //        foreach (var alienChiefItem in alienChiefItems)
            //        {
            //            resourcesTab.AddCraftingNode(alienChiefItem.TechType_I);
            //        }
            //        break;

            //    case "Condiments":

            //        var condimentsTab = root.AddTabNode("FCSAlienChiefCondiments", "FCS Alien Condiments", new Atlas.Sprite(ImageUtils.LoadTextureFromFile($"./QMods/{Information.ModName}/Assets/CondimentsTab.png")));

            //        AlienChiefTreeType = craftType;

            //        foreach (var alienChiefItem in alienChiefItems)
            //        {
            //            condimentsTab.AddCraftingNode(alienChiefItem.TechType_I);
            //        }
            //        break;
            //} 
            #endregion
        }

        #endregion

        private static void AddTabNodes(ref ModCraftTreeRoot root, ref ModCraftTreeTab itemTab, IAlienChiefItem alienChiefItem, string category, string icon)
        {

            if (root.GetNode($"FCSAlienChief{category}") == null)
            {
                Log.Info($"FCSAlienChief{category} is null creating tab");
                itemTab = root.AddTabNode($"FCSAlienChief{category}", $"FCS Alien {category}", new Atlas.Sprite(ImageUtils.LoadTextureFromFile($"./QMods/{Information.ModName}/Assets/{icon}.png")));
                itemTab?.AddCraftingNode(alienChiefItem.TechType_I);
                Log.Info($"FCSAlienChief{category} node tab Created");
            }
            else
            {
                Log.Info($"FCSAlienChief{category} is not null creating creating node tab");
                itemTab?.AddCraftingNode(alienChiefItem.TechType_I);
            }
        }

        public override GameObject GetGameObject()
        {
            // Instanciate fabricator
            GameObject prefab = GameObject.Instantiate(OriginalFabricator);

            prefab.name = AlienChiefFabId;

            // Update prefab ID
            var prefabId = prefab.GetComponent<PrefabIdentifier>();
            prefabId.ClassId = AlienChiefFabId;
            prefabId.name = this.PrefabFileName;

            // Update tech tag
            var techTag = prefab.GetComponent<TechTag>();
            techTag.type = TechType;

            // Associate craft tree to the fabricator
            var fabricator = prefab.GetComponent<Fabricator>();
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
            constructible.allowedOnGround = false;
            constructible.allowedOnConstructables = false;
            constructible.controlModelState = true;
            constructible.techType = TechType;

            // Set the custom texture
            Texture2D coloredTexture = AssetHelper.Asset.LoadAsset<Texture2D>("submarine_fabricator_orange");
            SkinnedMeshRenderer skinnedMeshRenderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMeshRenderer.material.mainTexture = coloredTexture;

            return prefab;
        }
    }
}
