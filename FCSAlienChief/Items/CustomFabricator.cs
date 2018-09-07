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

        private string _classId;
        private List<IAlienChiefItem> _alienChiefItems;

        #region Alien Chief fabricator

        /// <summary>
        /// This is the CraftTree.Type for the AlienChief fabricator.
        /// </summary>
        public static CraftTree.Type AlienChiefTreeType { get; private set; }


        /// <summary>
        /// This name will be used as ID for the decorations fabricator TechType and its associated CraftTree.Type.
        /// </summary>
        public static string AlienChiefFabId = "AlienChiefFabricator";


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
            

            
            var foodTab = root.AddTabNode("FCSAlienChiefFood", "FCS Alien Food", new Atlas.Sprite(ImageUtils.LoadTextureFromFile($"./QMods/{Information.ModName}/Assets/Default.png")));

            AlienChiefTreeType = craftType;


            foreach (var alienChiefItem in alienChiefItems)
            {
                foodTab.AddCraftingNode(alienChiefItem.TechType_I);
            }
            
            
        }

        #endregion



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
