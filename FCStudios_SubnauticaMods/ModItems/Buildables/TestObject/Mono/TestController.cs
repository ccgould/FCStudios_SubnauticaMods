using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.TestObject.Mono
{
    internal class TestController : MonoBehaviour
    {
        private Material _material;
        private Texture _textureGeo;
        private Texture _texture;
        private GameObject _currentVehGeo;

        private void Awake()
        {
            _material = MaterialHelpers.GetMaterial(GameObjectHelpers.FindGameObject(gameObject, "mesh"), ModPrefabService.BasePrimaryCol);

            _material.DisableKeyword("_SPECGLOSSMAP");
            _material.DisableKeyword("_EMISSION");

            _material.DisableKeyword("_METALLICGLOSSMAP");

            _material.DisableKeyword("_NORMALMAP");

            _texture = _material?.GetTexture("_MultiColorMask");
        }

        public void GetPlayerVehicleMaterial()
        {
            _textureGeo = null;
            var veh = Player.main.GetVehicle();
            if (veh == null)
            {
                QuickLogger.Error("Vehicle not Found", true);
                return;
            }

            var g = GameObjectHelpers.FindGameObject(veh.gameObject, "Submersible_SeaMoth_geo");
            _currentVehGeo = GameObjectHelpers.FindGameObject(g, "Submersible_SeaMoth_geo");
            
            if (_currentVehGeo == null) 
            {
                QuickLogger.Error("Seamoth Geo not Found", true);
                return;
            }

            _material = MaterialHelpers.GetMaterial(_currentVehGeo, "Submersible_SeaMoth (Instance)");

            //_material.DisableKeyword("_SPECGLOSSMAP");
            //_material.DisableKeyword("_EMISSION");

            //_material.DisableKeyword("_METALLICGLOSSMAP");

            //_material.DisableKeyword("_NORMALMAP");

            _textureGeo = _material?.GetTexture("_MultiColorMask");

        }

    }
}
