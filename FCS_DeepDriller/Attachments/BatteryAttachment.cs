using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Enumerators;
using FCS_DeepDriller.Mono;
using UnityEngine;

namespace FCS_DeepDriller.Attachments
{
    internal class BatteryAttachment : MonoBehaviour
    {
        private GameObject _batteryModule;
        private FCSDeepDrillerBatteryController _batteryController;

        internal void GetGameObject(FCSDeepDrillerController mono)
        {
            var mount = mono.ComponentManager.GetMount(mono, DeepDrillerMountSpot.PowerSupply);

            _batteryModule = GameObject.Instantiate(FCSDeepDrillerBuildable.BatteryModule, mount.transform.position, mount.transform.rotation);

            var rb = _batteryModule.GetComponent<Rigidbody>();

            // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
            var lwe = _batteryModule.AddComponent<LargeWorldEntity>();
            lwe.cellLevel = LargeWorldEntity.CellLevel.Global;

            DestroyObject(rb);

            _batteryController = _batteryModule.AddComponent<FCSDeepDrillerBatteryController>();
            _batteryController.Setup(mono);

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

        internal FCSDeepDrillerBatteryController GetController()
        {
            return _batteryController;
        }

        private void OnDestroy()
        {
            Destroy(_batteryModule);
        }
    }
}
