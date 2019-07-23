using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCSTechFabricator.Mono.DeepDriller
{
    public partial class SolarAttachmentBuildable
    {
        private GameObject _prefab;

        public bool GetPrefabs()
        {
            QuickLogger.Debug($"AssetBundle Set");

            //We have found the asset bundle and now we are going to continue by looking for the model.
            _prefab = QPatch.Bundle.LoadAsset<GameObject>("Freon_Bottle");

            //If the prefab isn't null lets add the shader to the materials
            if (_prefab != null)
            {

                //Lets apply the material shader
                ApplyShaders(_prefab);

                QuickLogger.Debug($"{this.FriendlyName_I} Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"{this.FriendlyName_I} Prefab Not Found!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        private void ApplyShaders(GameObject prefab)
        {
            MaterialHelpers.ApplyNormalShader("Freon_Bottle", "Freon_Freon_Bottle_Normal", prefab, QPatch.Bundle);
            MaterialHelpers.ApplySpecShader("Freon_Bottle", "Freon_Freon_Bottle_Specular", prefab, 1f, 4f, QPatch.Bundle);
        }
    }
}
