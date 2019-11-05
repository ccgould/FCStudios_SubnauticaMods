using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator.Helpers;
using FCSTechFabricator.Models;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTechFabricator.Mono.SeaCooker
{
    internal class SeaCookerBuildable : TechFabCraftable
    {
        private Text _label;

        public override GameObject OriginalPrefab { get; set; } = QPatch.Kit;
        public override string IconFileName { get; } = "Kit_FCS.png";
        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
        public override string[] StepsToFabricatorTab { get; } = { "AE", "SC" };
        public override TechType TechTypeID { get; set; }

        public SeaCookerBuildable() :
            base("SeaCookerBuildableKit_SC", "Sea Cooker Unit Kit", "This kit allows you to make one Sea Cooker unit")
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
