using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Mods.Microwave.Mono;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Microwave.Buildable
{
   internal class MicrowavePatch : DecorationEntryPatch
    {
        public MicrowavePatch(string classId, string friendlyName, string description, GameObject prefab, Settings settings) : base(classId, friendlyName, description, prefab, settings)
        {
        }

        public override GameObject GetGameObject()
        {
            var output = base.GetGameObject();
            UWEHelpers.CreateStorageContainer(output, null, ClassID, "Microwave", 1, 1);
            output.AddComponent<MicrowaveController>();

            return output;
        }
    }
}
