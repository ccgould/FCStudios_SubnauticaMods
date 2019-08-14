using FCSCommon.Helpers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_DeepDriller.Ores.Craftables
{
    internal class Glass : Craftable
    {
        private static readonly Glass Singleton = new Glass();

        public Glass() : base("", "", "")
        {

        }


        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.Glass));

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
                    new Ingredient(TechTypeHelpers.GetTechType("Sand_DD"), 1),
                },
                LinkedItems = new List<TechType>()
                {
                    TechType.Glass,
                    TechType.Glass,
                    TechType.Glass,
                }
            };

            return customFabRecipe;
        }

        internal static void PatchHelper()
        {

        }

        public override TechGroup GroupForPDA => TechGroup.Resources;
        public override TechCategory CategoryForPDA => TechCategory.BasicMaterials;
        public override CraftTree.Type FabricatorType { get; } = CraftTree.Type.Fabricator;
        public override string[] StepsToFabricatorTab { get; } = new[] { "AIS", "DD" };
        public override string AssetsFolder => $"FCS_DeepDriller/Assets";

    }
}
