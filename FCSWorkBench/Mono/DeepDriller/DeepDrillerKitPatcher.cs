using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator.Helpers;
using FCSTechFabricator.Models;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTechFabricator.Mono.DeepDriller
{
    public partial class DeepDrillerKitBuildable : Craftable
    {
        private GameObject _prefab;
        private Text _label;
        public override string AssetsFolder { get; } = "FCSTechFabricator/Assets";
        public override string IconFileName { get; } = "Kit_FCS.png";
        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
        public override string[] StepsToFabricatorTab { get; } = new[] { "AIS", "DD" };
        internal static TechType TechTypeID { get; private set; }
        public override CraftTree.Type FabricatorType { get; } =
            FCSTechFabricatorBuildable.TechFabricatorCraftTreeType;

        public DeepDrillerKitBuildable() : base("DeepDrillerKit_DD", "Deep Driller Kit", "This kit allows you to make one Deep Driller unit")
        {
            if (!GetPrefabs())
            {
                QuickLogger.Error("Failed to retrieve all prefabs");
            }

            OnFinishedPatching = () =>
            {
                TechTypeID = this.TechType;

                //Add the new TechType Hand Equipment type
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.Hand);
            };
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate<GameObject>(QPatch.Kit);

            prefab.name = this.PrefabFileName;

            if (!FindAllComponents(prefab))
            {
                QuickLogger.Error("Failed to get all components");
                return null;
            }

            _label.text = FriendlyName;

            PrefabIdentifier prefabID = prefab.GetOrAddComponent<PrefabIdentifier>();

            prefabID.ClassId = this.ClassID;

            var techTag = prefab.GetOrAddComponent<TechTag>();
            techTag.type = TechType;

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            var inventoryItem = QPatch.Configuration.GetData(ClassID);
            TechData customFabRecipe = null;

            if (IngredientHelper.ValidateData(inventoryItem))
            {
                // Create and associate recipe to the new TechType
                customFabRecipe = new TechData()
                {
                    craftAmount = 1,
                    Ingredients = IngredientHelper.ConvertToIngredients(inventoryItem).ToList()
                };
            }
            else
            {
                // Create and associate recipe to the new TechType
                customFabRecipe = new TechData()
                {
                    craftAmount = 1,
                    Ingredients = new List<Ingredient>()
                    {
                        new Ingredient(TechType.Titanium, 1)
                    }
                };
            }


            return customFabRecipe;
        }

        public bool GetPrefabs()
        {
            QuickLogger.Debug($"AssetBundle Set");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            _prefab = QPatch.Kit;

            //If the prefab isn't null lets add the shader to the materials
            if (_prefab != null)
            {
                //Lets apply the material shader
                Shaders.ApplyKitShaders(_prefab);
            }

            return true;
        }

        private bool FindAllComponents(GameObject prefab)
        {
            var canvasObject = prefab.GetComponentInChildren<Canvas>().gameObject;
            if (canvasObject == null)
            {
                QuickLogger.Error("Could not find the canvas");
                return false;
            }

            _label = canvasObject.FindChild("Screen").FindChild("Label").GetComponent<Text>();
            return true;
        }


        //new List<Ingredient>()
        //{
        //    new Ingredient(TechType.MapRoomHUDChip, 1),
        //    new Ingredient(TechType.Titanium, 2),
        //    new Ingredient(TechType.AdvancedWiringKit, 1),
        //    new Ingredient(TechType.ExosuitDrillArmModule, 1),
        //    new Ingredient(TechType.Lubricant, 2),
        //    new Ingredient(TechType.VehicleStorageModule, 1),
        //}
    }
}
