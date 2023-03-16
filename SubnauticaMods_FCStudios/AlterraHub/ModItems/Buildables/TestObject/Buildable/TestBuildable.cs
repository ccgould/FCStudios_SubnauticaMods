using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.ModItems.TestObject.Mono;
using FCSCommon.Helpers;
using SMLHelper.Crafting;
using System.Collections;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.TestObject.Buildable
{
    internal class TestBuildable : FCSBuildableModBase
    {
        public TestBuildable() : base(Main.MODNAME, "fcs_DummyTest", FileSystemHelper.ModDirLocation, "TestObject","Test Object")
        {

        }

        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            yield return base.GetGameObjectAsync(gameObject);

            Prefab.AddComponent<TestController>();

            yield break;
        }

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients =
                {      
                new Ingredient(TechType.Glass, 2),
                new Ingredient(TechType.TitaniumIngot, 1),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.ComputerChip, 1)
                }
            };
        }
    }
}
