using FCSCommon.Utilities;
using FCSTechFabricator.Helpers;
using SMLHelper.V2.Crafting;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTechFabricator.Mono.PowerStorage
{
    internal class PowerStorageKitBuildable : TechFabCraftable
    {
        private Text _label;

        public override GameObject OriginalPrefab { get; set; } = QPatch.Kit;
        public override string IconFileName { get; } = "Kit_FCS.png";
        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public  override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
        public override string[] StepsToFabricatorTab { get; } = { "AIS", "PS" };
        public override TechType TechTypeID { get; set; }

        public PowerStorageKitBuildable() : 
            base("PowerStorageKit_PS", "Power Storage Kit", "A kit that allows you to build one Power Storage Unit")
        {
            
        }

        public override void GetGameObjectExt(GameObject instantiatedPrefab)
        {
            FindAllComponents(instantiatedPrefab);
        }

        protected override TechData GetBlueprintRecipe()
        {
            return IngredientHelper.GetCustomRecipe(ClassID);
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
            _label.text = FriendlyName;
            return true;
        }
    }
}
