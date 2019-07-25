using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Enumerators;
using FCS_DeepDriller.Managers;
using FCS_DeepDriller.Mono;
using FCS_DeepDriller.Mono.Handlers;
using UnityEngine;

namespace FCS_DeepDriller.Attachments
{
    internal class SolarAttachment : MonoBehaviour
    {
        private GameObject _solarModule;

        internal void GetGameObject(FCSDeepDrillerController mono)
        {
            var mount = DeepDrillerComponentManager.GetMount(mono, DeepDrillerMountSpot.PowerSupply);

            var difference = 0.486178f;
            var newYPos‬ = mount.transform.position.y - difference;

            var position = new Vector3(mount.transform.position.x, newYPos‬, mount.transform.position.z);

            _solarModule = GameObject.Instantiate(FCSDeepDrillerBuildable.SolarModule, position, mount.transform.rotation);

            var rb = _solarModule.GetComponent<Rigidbody>();
            var pickupable = _solarModule.GetComponent<Pickupable>();
            DestroyObject(rb);
            DestroyObject(pickupable);

            var solarController = _solarModule.AddComponent<FCSDeepDrillerSolarController>();
            solarController.Setup(mono);

            _solarModule.SetActive(false);

            _solarModule.transform.SetParent(mono.transform);
        }

        internal void ObjectVisibility(bool visible)
        {
            _solarModule.SetActive(visible);
        }

        internal GameObject GetSolarAttachment()
        {
            return _solarModule;
        }
    }
}
