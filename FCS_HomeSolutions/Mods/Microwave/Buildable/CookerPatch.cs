using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Enums;
using FCS_HomeSolutions.Mods.Microwave.Mono;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Microwave.Buildable
{
   internal class CookerPatch : DecorationEntryPatch
    {
        private readonly CookingMode _cookingMode;

        public CookerPatch(string classId, string friendlyName, string description, GameObject prefab, Settings settings,CookingMode cookingMode = CookingMode.Cooking) : base(classId, friendlyName, description, prefab, settings)
        {
            _cookingMode = cookingMode;
        }

        public override GameObject GetGameObject()
        {
            var output = base.GetGameObject();
            UWEHelpers.CreateStorageContainer(output, null, ClassID, "Microwave", 1, 1);
            var cooker = output.AddComponent<CookerController>();
            cooker.CookingMode = _cookingMode;
            cooker.DummyFoodObject = GameObjectHelpers.FindGameObject(output, "mesh_hangingfood");
            return output;
        }
    }
}
