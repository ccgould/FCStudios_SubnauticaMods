using System;
using SMLHelper.V2.Crafting;
using UnityEngine;

#if DEBUG
// This is example code written as if it were from another mod

namespace FCSTechFabricator
{
    internal class ExampleTab : FcCraftingTab
    {
#if SUBNAUTICA
        public ExampleTab(Atlas.Sprite icon)
            : base("ExampleTab1", "displayName",icon)
        {
        }
#elif BELOWZERO
        public ExampleTab(Sprite icon)
            : base("ExampleTab1", "displayName", icon)
        {
        }
#endif

        public override string AssetBundleName { get; } = "examplebundlename";
    }

    internal class ExampleCraftable1 : FcCraftable
    {
        public ExampleCraftable1(FcCraftingTab parentTab)
            : base("ExampleCraftable1", "friendlyName", "description", parentTab)
        {
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.Machines;
        public override TechCategory CategoryForPDA { get; } = TechCategory.Machines;
        protected override string AssetBundleName { get; } = "examplebundlename";

        public override string IconFileName => "exampleiconname1";

        public override GameObject GetGameObject()
        {
            throw new NotImplementedException();
        }

#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            throw new NotImplementedException();
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            throw new NotImplementedException();
        }
#endif
    }

    internal class ExampleCraftable2 : FcCraftable
    {
        public ExampleCraftable2(FcCraftingTab parentTab)
            : base("ExampleCraftable2", "friendlyName", "description", parentTab)
        {
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.Machines;
        public override TechCategory CategoryForPDA { get; } = TechCategory.Machines;
        protected override string AssetBundleName { get; } = "examplebundlename";

        public override string IconFileName => "exampleiconname2";

        public override GameObject GetGameObject()
        {
            throw new NotImplementedException();
        }

#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            throw new NotImplementedException();
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            throw new NotImplementedException();
        }
#endif
    }


    //[QModCore]
    //public static class ExamplePatchClass
    //{
    //    [QModPatch]
    //    public static void ExamplePatchMethod()
    //    {
    //        var tab = new ExampleTab();

    //        var craftable1 = new ExampleCraftable1(tab);
    //        craftable1.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

    //        var craftable2 = new ExampleCraftable2(tab);
    //        craftable2.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
    //    }
    //}
}
#endif
