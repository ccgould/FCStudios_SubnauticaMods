using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Configuration;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#endif

namespace FCS_EnergySolutions.Mods.Spawnables
{
    class PowerStorageCell : Spawnable
    {
        public override string AssetsFolder => Mod.GetAssetPath();

        public PowerStorageCell() : base("PowerStorageCell", "Power Storage Cell", "An empty powercell to be used in power storage.")
        {

            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, TechType, 495000, StoreCategory.Energy);
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.PowerCellCharger);
                TechDataHelpers.AddPowercell(TechType);
                KnownTechHandler.SetAnalysisTechEntry(TechType.PowerCell, new TechType[1] { TechType });
                CraftDataHandler.AddToGroup(TechGroup.Resources, TechCategory.Electronics, TechType);
            };
        }

#if SUBNAUTICA_STABLE
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

            return obj;
        }
#else
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var taskResult = CraftData.GetPrefabForTechTypeAsync(TechType.PrecursorIonPowerCell);
            yield return taskResult;
            var obj = GameObject.Instantiate(taskResult.GetResult());

            Battery battery = obj.GetComponent<Battery>();
            battery._capacity = 3000;
            battery.name = $"PowerStorageCell";
            battery._charge = 0f;

            SkyApplier skyApplier = obj.EnsureComponent<SkyApplier>();
            skyApplier.renderers = obj.GetComponentsInChildren<Renderer>(true);
            skyApplier.anchorSky = Skies.Auto;

            gameObject.Set(obj);
        }
#endif
        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
