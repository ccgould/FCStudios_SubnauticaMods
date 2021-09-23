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
        private int _slotCountY;
        private int _slotCountX;

        public CookerPatch(string classId, string friendlyName, string description, GameObject prefab, Settings settings, Vector2int containerSize, CookingMode cookingMode = CookingMode.Cooking) : base(classId, friendlyName, description, prefab, settings)
        {
            _cookingMode = cookingMode;

            if (containerSize.x == 0 || containerSize.y == 0)
            {
                _slotCountX = 1;
                _slotCountY = 1;
            }
            else
            {
                _slotCountX = containerSize.x;
                _slotCountY = containerSize.y;
            }
        }

        public override GameObject GetGameObject()
        {
            var output = base.GetGameObject();
            UWEHelpers.CreateStorageContainer(output, null, ClassID, FriendlyName, 1, 1);
            var cooker = output.AddComponent<CookerController>();
            cooker.CookingMode = _cookingMode;
            cooker.DummyFoodObject = GameObjectHelpers.FindGameObject(output, "mesh_hangingfood");
            cooker.SlotCountY = _slotCountY;
            cooker.SlotCountX = _slotCountX;
            return output;
        }
    }
}
