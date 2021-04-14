using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Configuration;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_EnergySolutions.Spawnables
{
    class PowerStorageCell : Spawnable
    {
        public override string AssetsFolder => Mod.GetAssetPath();
        public PowerStorageCell() : base("PowerStorageCell", "Power Storage Cell", "An empty powercell to be used in power storage.")
        {
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, TechType, 1000, StoreCategory.Energy);
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.PowerCellCharger);
            };
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = CraftData.GetPrefabForTechType(TechType.PrecursorIonPowerCell);
            var obj = GameObject.Instantiate(prefab);

            Battery battery = obj.GetComponent<Battery>();
            battery._capacity = 3000;
            battery.name = $"PowerStorageCell";
            battery._charge = 0f;

            SkyApplier skyApplier = obj.EnsureComponent<SkyApplier>();
            skyApplier.renderers = obj.GetComponentsInChildren<Renderer>(true);
            skyApplier.anchorSky = Skies.Auto;

            //if (CustomModelData != null)
            //{
            //    foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>(true))
            //    {
            //        if (CustomModelData.CustomTexture != null)
            //            renderer.material.SetTexture(ShaderPropertyID._MainTex, this.CustomModelData.CustomTexture);

            //        if (CustomModelData.CustomNormalMap != null)
            //            renderer.material.SetTexture(ShaderPropertyID._BumpMap, this.CustomModelData.CustomNormalMap);

            //        if (CustomModelData.CustomSpecMap != null)
            //            renderer.material.SetTexture(ShaderPropertyID._SpecTex, this.CustomModelData.CustomSpecMap);

            //        if (CustomModelData.CustomIllumMap != null)
            //        {
            //            renderer.material.SetTexture(ShaderPropertyID._Illum, this.CustomModelData.CustomIllumMap);
            //            renderer.material.SetFloat(ShaderPropertyID._GlowStrength, this.CustomModelData.CustomIllumStrength);
            //            renderer.material.SetFloat(ShaderPropertyID._GlowStrengthNight, this.CustomModelData.CustomIllumStrength);
            //        }
            //    }
            //}

            //this.EnhanceGameObject?.Invoke(obj);

            return obj;
        }
    }
}
