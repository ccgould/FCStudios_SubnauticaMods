using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Enumerators;
using FCS_DeepDriller.Managers;
using FCS_DeepDriller.Mono;
using FCS_DeepDriller.Mono.Handlers;
using UnityEngine;

namespace FCS_DeepDriller.Attachments
{
    internal class FocusAttachment : MonoBehaviour
    {
        private GameObject _focusModule;

        internal void GetGameObject(FCSDeepDrillerController mono)
        {
            var mount = DeepDrillerComponentManager.GetMount(mono, DeepDrillerMountSpot.Screen);

            _focusModule = GameObject.Instantiate(FCSDeepDrillerBuildable.FocusModule, mount.transform.position, mount.transform.rotation);

            var rb = _focusModule.GetComponent<Rigidbody>();

            DestroyObject(rb);

            var focusDisplayController = _focusModule.AddComponent<FCSDeepDrillerFocusDisplay>();
            focusDisplayController.Setup(mono);

            // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
            var lwe = _focusModule.AddComponent<LargeWorldEntity>();
            lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

            _focusModule.SetActive(false);

            _focusModule.transform.SetParent(mono.transform);
        }

        internal void ObjectVisibility(bool visible)
        {
            _focusModule.SetActive(visible);
        }

        internal GameObject GetFocusAttachment()
        {
            return _focusModule;
        }

        private void OnDestroy()
        {
            Destroy(_focusModule);
        }
    }
}
