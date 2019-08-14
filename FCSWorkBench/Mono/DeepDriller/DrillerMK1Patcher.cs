using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using System.Collections.Generic;
using UnityEngine;

namespace FCSTechFabricator.Mono.DeepDriller
{
    public partial class DrillerMK1Craftable : Craftable
    {
        internal static TechType TechTypeID { get; private set; }

        public override CraftTree.Type FabricatorType { get; } =
            FCSTechFabricatorBuildable.TechFabricatorCraftTreeType;

        public override string[] StepsToFabricatorTab { get; } = new[] { "AIS", "DD" };

        public override string AssetsFolder { get; } = "FCSTechFabricator/Assets";

        public DrillerMK1Craftable() : base("DrillerMK1_DD", "Deep Driller MK 1", "This upgrade allows deep driller to drill 15 resources per day.")
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
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Lithium, 2),
                    new Ingredient(TechType.Diamond, 1),
                    new Ingredient(TechType.Titanium, 3),
                }
            };

            return customFabRecipe;
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.AdvancedMaterials;
    }
}
