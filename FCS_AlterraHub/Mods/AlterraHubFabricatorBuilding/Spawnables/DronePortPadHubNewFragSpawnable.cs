using System;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#endif

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Spawnables
{
    internal class DronePortPadHubNewFragSpawnable : Spawnable
    {
        private Color _color;
        public override string AssetsFolder => Mod.GetAssetPath();

        public DronePortPadHubNewFragSpawnable() : base("DronePortPadHubNewFragPart", "Drone Port Pad Fragment", "Fragment for the Drone Port Pad")
        {
            OnFinishedPatching += () =>
            {
                Mod.DronePortPadHubNewTechType = TechType;
                CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(TechType,new Vector3(76.0f, -304.7f, -1430.8f),new Quaternion(-0.2f, -0.2f, 0.5f, 0.8f)));
                CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(TechType,new Vector3(60.3f, -318.8f, -1417.2f),new Quaternion(-0.2f, -0.2f, 0.5f, 0.8f)));
                CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(TechType,new Vector3(64.9f, -305.0f, -1420.4f),new Quaternion(0.0f, -0.9f, 0.4f, 0.2f)));
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(AlterraHub.DronePortFragmentsPrefab);
                
                prefab.AddComponent<PrefabIdentifier>();
                prefab.AddComponent<TechTag>().type = TechType;

                var lw = prefab.AddComponent<LargeWorldEntity>();
                lw.cellLevel = LargeWorldEntity.CellLevel.Near;

                var pickUp = prefab.AddComponent<Pickupable>();
                pickUp.isPickupable = false;

                //Renderer
                var renderer = prefab.GetComponentInChildren<Renderer>();

                // Update sky applier
                var applier = prefab.GetComponent<SkyApplier>();
                if (applier == null)
                    applier = prefab.AddComponent<SkyApplier>();
                applier.renderers = new Renderer[] { renderer };
                applier.anchorSky = Skies.Auto;

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
