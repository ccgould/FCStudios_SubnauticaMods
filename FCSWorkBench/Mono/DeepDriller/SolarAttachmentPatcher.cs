using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator.Models;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTechFabricator.Mono.PowerStorage
{
    public class SolarAttachmentBuildable : Craftable
    {
        private GameObject _prefab;
        private Text _label;
        public override string AssetsFolder { get; } = "FCSTechFabricator/Assets";
        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
        public override string[] StepsToFabricatorTab { get; } = new[] { "AIS", "DD" };
        internal static TechType TechTypeID { get; private set; }
        public override CraftTree.Type FabricatorType { get; } =
            FCSTechFabricatorBuildable.TechFabricatorCraftTreeType;

        public SolarAttachmentBuildable() : base("SolarAttachment_DD", "Deep Driller Solar Attachment", "This specially made attachment allows you to run your deep driller off solar power.")
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

            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Copper, 1),
                    new Ingredient(TechType.Glass, 2),
                    new Ingredient(TechType.WiringKit, 1),
                    new Ingredient(TechType.Titanium, 1),
                }
            };

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
    }
}
