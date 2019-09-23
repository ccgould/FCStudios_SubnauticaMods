using FCSTechFabricator.Helpers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCSTechFabricator.Mono.DeepDriller
{
    public partial class DrillerMK3Craftable : Craftable
    {
        internal static TechType TechTypeID { get; private set; }

        public override CraftTree.Type FabricatorType { get; } =
            FCSTechFabricatorBuildable.TechFabricatorCraftTreeType;

        public override string[] StepsToFabricatorTab { get; } = new[] { "AIS", "DD" };

        public override string AssetsFolder { get; } = "FCSTechFabricator/Assets";

        public DrillerMK3Craftable() : base("DrillerMK3_DD", "Deep Driller MK 3", "This upgrade allows deep driller to drill 30 resources per day.")
        {
            OnFinishedPatching = () =>
            {
                TechTypeID = this.TechType;
                //Add the new TechType Hand Equipment type
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.CyclopsModule);
            };
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.CyclopsShieldModule));

            prefab.name = this.PrefabFileName;

            prefab.AddComponent<FCSTechFabricatorTag>();

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            return IngredientHelper.GetCustomRecipe(ClassID);
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
    }
}
