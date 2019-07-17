using FCSCommon.Helpers;
using UnityEngine;

namespace FCSTechFabricator.Mono.PowerStorage
{
    public class PowerStorageKitController : MonoBehaviour
    {
        private void Awake()
        {
            MaterialHelpers.ApplyAlphaShader("FCS_SUBMods_GlobalDecals_2", gameObject);
            var model = gameObject.GetComponentInChildren<Canvas>().gameObject;

            model.FindChild("Screen").SetActive(true);
        }
    }
}
