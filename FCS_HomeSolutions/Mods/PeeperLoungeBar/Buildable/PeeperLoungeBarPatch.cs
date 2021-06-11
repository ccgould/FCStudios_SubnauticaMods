using System;
using System.IO;
using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.PeeperLoungeBar.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;

#endif
namespace FCS_HomeSolutions.Mods.PeeperLoungeBar.Buildable
{
    internal class PeeperLoungeBarPatch : DecorationEntryPatch
    {
        public PeeperLoungeBarPatch(string classId, string friendlyName, string description, GameObject prefab, Settings settings) : base(classId, friendlyName, description, prefab, settings)
        {

        }

        public override GameObject GetGameObject()
        {
            try
            {

                var prefab = GameObject.Instantiate(_prefab);

                //Disable the object so we can fill in the properties before awake
                prefab.SetActive(false);

                GameObjectHelpers.AddConstructableBounds(prefab, _settings.Size, _settings.Center);

                var model = prefab.FindChild("model");

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();

                constructable.allowedOutside = _settings.AllowedOutside;
                constructable.allowedInBase = _settings.AllowedInBase;
                constructable.allowedOnGround = _settings.AllowedOnGround;
                constructable.allowedOnWall = _settings.AllowedOnWall;
                constructable.rotationEnabled = _settings.RotationEnabled;
                constructable.allowedOnCeiling = _settings.AllowedOnCeiling;
                constructable.allowedInSub = _settings.AllowedInSub;
                constructable.allowedOnConstructables = _settings.AllowedOnConstructables;
                constructable.model = model;
                constructable.techType = TechType;
                
                prefab.AddComponent<PrefabIdentifier>().ClassId = ClassID;
                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<DecorationController>();
                prefab.AddComponent<PeeperLoungeBarController>();

                //CreateWaterPark(prefab, constructable);
                CreateAquarium(prefab);

                prefab.SetActive(true);
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
            }

            return null;
        }

        private void CreateAquarium(GameObject prefab)
        {
            var aquarium = prefab.AddComponent<Aquarium>();

            var sRootGo = GameObjectHelpers.FindGameObject(prefab, "StorageRoot");
            var sc = prefab.AddComponent<StorageContainer>();
            var sRoot = sRootGo.EnsureComponent<ChildObjectIdentifier>();
            sRoot.classId = ClassID;
            sc.storageRoot = sRoot;
            sc.width = 1;
            sc.height = 3;

            aquarium.fishRoot = GameObjectHelpers.FindGameObject(prefab, "grownPlant");
            aquarium.trackObjects = new[]
            {
                GameObjectHelpers.FindGameObject(prefab, "FishItem1"),
                GameObjectHelpers.FindGameObject(prefab, "FishItem2"),
                GameObjectHelpers.FindGameObject(prefab, "FishItem3"),
            };
            aquarium.storageContainer = sc;
            sc.enabled = false;
        }

        private void CreateWaterPark(GameObject prefab, Constructable constructable)
        {
            var sRootGo = GameObjectHelpers.FindGameObject(prefab, "StorageRoot");
            var slot1 = GameObjectHelpers.FindGameObject(prefab, "slot1");
            var slot2 = GameObjectHelpers.FindGameObject(prefab, "slot2");
            var slot3 = GameObjectHelpers.FindGameObject(prefab, "slot3");
            var slot4 = GameObjectHelpers.FindGameObject(prefab, "slot4");

            var gPlant = GameObjectHelpers.FindGameObject(prefab, "grownPlant");
            var gPlantCo = gPlant.AddComponent<ChildObjectIdentifier>();
            gPlantCo.classId = ClassID;
            var sc = prefab.AddComponent<StorageContainer>();
            var sRoot = sRootGo.EnsureComponent<ChildObjectIdentifier>();
            sRoot.classId = ClassID;
            sc.storageRoot = sRoot;
            sc.width = 2;
            sc.height = 2;
            var planter = prefab.AddComponent<Planter>();
            planter.storageContainer = sc;
            planter.slots = new[]
            {
                slot1.transform,
                slot2.transform,
                slot3.transform,
                slot4.transform,
            };

            planter.grownPlantsRoot = gPlant.transform;
            planter.environment = Planter.PlantEnvironment.Water;
            planter.constructable = constructable;
            planter.isIndoor = true;

            //Reactivate mesh now that settings are set

            var wp = prefab.AddComponent<WaterPark>();
            wp.planter = planter;
            wp.wpPieceCapacity = 2;
            wp.itemsRoot = sRootGo.transform;
            //wp.height = 0.4824555f;
        }
    }
}
