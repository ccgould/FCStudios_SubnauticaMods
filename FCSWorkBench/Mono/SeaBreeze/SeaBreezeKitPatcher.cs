using FCSCommon.Utilities;
using FCSTechFabricator.Helpers;
using SMLHelper.V2.Crafting;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTechFabricator.Mono.SeaBreeze
{
    internal class SeaBreezeKitBuildable : TechFabCraftable
    {
        private Text _label;
        public override GameObject OriginalPrefab { get; set; } = QPatch.Kit;
        public override string IconFileName { get; } = "Kit_FCS.png";
        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
        public override string[] StepsToFabricatorTab { get; } = { "ARS", "SB" };
        public override TechType TechTypeID { get; set; }

        public SeaBreezeKitBuildable() :
            base("SeaBreezeKit_SB", "Sea Breeze Kit", "A kit that allows you to build one Seabreeze refrigerator.")
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
