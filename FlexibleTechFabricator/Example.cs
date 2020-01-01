#if DEBUG
// This is example code written as if it were from another mod

namespace SomeOtherMod
{
    using System;
    using FlexibleTechFabricator;
    using QModManager.API.ModLoading;
    using SMLHelper.V2.Crafting;
    using UnityEngine;

    internal class ExampleTab : FcCraftingTab
    {
        public ExampleTab()
            : base("ExampleTab1", "displayName")
        {
        }

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

        protected override TechData GetBlueprintRecipe()
        {
            throw new NotImplementedException();
        }
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

        protected override TechData GetBlueprintRecipe()
        {
            throw new NotImplementedException();
        }
    }


    [QModCore]
    public static class ExamplePatchClass
    {
        [QModPatch]
        public static void ExamplePatchMethod()
        {
            var tab = new ExampleTab();

            var craftable1 = new ExampleCraftable1(tab);
            craftable1.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var craftable2 = new ExampleCraftable2(tab);
            craftable2.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }
    }
}
#endif
