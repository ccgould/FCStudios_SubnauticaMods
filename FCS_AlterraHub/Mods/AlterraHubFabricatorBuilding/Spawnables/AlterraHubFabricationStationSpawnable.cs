using System;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
using UWE;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#endif

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Spawnables
{
    internal class AlterraHubFabricationStationSpawnable : Spawnable
    {
        private Color _color;
        public override string AssetsFolder => Mod.GetAssetPath();

        public AlterraHubFabricationStationSpawnable() : base("AlterraHubFabricationStation", "AlterraHub Fabrication Station", "N/A")
        {
            OnFinishedPatching += () =>
            {
                Mod.DronePortPadHubNewFragmentTechType = TechType;
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(AlterraHub.AlterraHubFabricatorPrefab);

                PrefabIdentifier prefabIdentifier = prefab.EnsureComponent<PrefabIdentifier>();
                prefabIdentifier.ClassId = this.ClassID;

                prefab.AddComponent<TechTag>().type = TechType;

                var lw = prefab.AddComponent<LargeWorldEntity>();
                lw.cellLevel = LargeWorldEntity.CellLevel.Global;

                var pickUp = prefab.AddComponent<Pickupable>();
                pickUp.isPickupable = false;

                //Renderer
                var renderer = prefab.GetComponentInChildren<Renderer>();

                var rb = prefab.GetComponentInChildren<Rigidbody>();

                if (rb == null)
                {
                    rb = prefab.EnsureComponent<Rigidbody>();
                    rb.isKinematic = true;
                }
                

                // Update sky applier
                var applier = prefab.EnsureComponent<SkyApplier>();
                applier.renderers = new Renderer[] { renderer };
                applier.anchorSky = Skies.Auto;

                WorldHelpers.CreateBeacon(prefab, Mod.AlterraHubStationPingType, "Alterra Hub Station");
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
                var station = prefab.AddComponent<AlterraFabricatorStationController>();
                prefab.AddComponent<PortManager>();
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
