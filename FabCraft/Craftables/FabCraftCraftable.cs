using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System;
using UnityEngine;

namespace FabCraft.Craftables
{
    internal partial class FabCraftCraftable : Craftable
    {
        public FabCraftCraftable() : base("", "", "")
        {
        }

        public override GameObject GetGameObject()
        {
            var glass = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.Glass));
            return glass;
        }

        public override string AssetsFolder { get; }
        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                Ingredients =
                {
                    new Ingredient(TechType.Titanium, 1),
                }
            };
        }

        public override TechGroup GroupForPDA => TechGroup.Resources;
        public override TechCategory CategoryForPDA => TechCategory.BasicMaterials;
        public override CraftTree.Type FabricatorType => CraftTree.Type.Fabricator;

        internal static void PatchSMLHelper()
        {
            throw new NotImplementedException();
        }
    }
}
