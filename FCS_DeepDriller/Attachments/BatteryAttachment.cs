using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_DeepDriller.Attachments
{
    internal class BatteryAttachment : MonoBehaviour
    {
        private GameObject _batteryModule;

        internal void GetGameObject(FCSDeepDrillerController mono)
        {
            var mount = mono.gameObject.FindChild("Mount_Point")?.gameObject;

            if (mount == null)
            {
                QuickLogger.Error("Couldnt find GameObject Mount_Point");
                return;
            }

            _batteryModule = GameObject.Instantiate(FCSDeepDrillerBuildable.BatteryModule, mount.transform.position, mount.transform.rotation);

            var batteryController = _batteryModule.AddComponent<FCSDeepDrillerBatteryController>();
            batteryController.Setup(mono);

            _batteryModule.SetActive(false);

            _batteryModule.transform.SetParent(mono.transform);
        }

        internal void ObjectVisibility(bool visible)
        {
            _batteryModule.SetActive(visible);
        }

        internal GameObject GetBatteryAttachment()
        {
            return _batteryModule;
        }
    }
}
