using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTechWorkBench.Mono.PowerStorage
{
    public partial class PowerStorageKitBuildable
    {
        private GameObject _prefab;
        private Text _label;

        public bool GetPrefabs()
        {
            QuickLogger.Debug($"AssetBundle Set");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            _prefab = QPatch.Kit;

            //If the prefab isn't null lets add the shader to the materials
            if (_prefab != null)
            {

                //Lets apply the material shader
                ApplyShaders(_prefab);

            }

            return true;
        }

        /// <summary>
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        private void ApplyShaders(GameObject prefab)
        {
            MaterialHelpers.ApplyEmissionShader("UnitContainerKit_BaseColor", "UnitContainerKit_Emissive", prefab, QPatch.Bundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyAlphaShader("UnitContainerKit_BaseColor", prefab);
            MaterialHelpers.ApplyNormalShader("UnitContainerKit_BaseColor", "UnitContainerKit_Norm", prefab, QPatch.Bundle);
            MaterialHelpers.ApplySpecShader("UnitContainerKit_BaseColor", "UnitContainerKit_Spec", prefab, 1f, 6f, QPatch.Bundle);

        }

        private bool FindAllComponents(GameObject prefab)
        {
            var canvasObject = prefab.GetComponentInChildren<Canvas>().gameObject;
            if (canvasObject == null)
            {
                QuickLogger.Error("Could not find the canvas");
                return false;
            }

            _label = canvasObject.FindChild("Screen").FindChild("Label").GetComponent<Text>();
            return true;
        }
    }
}
