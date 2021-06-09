using System;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Registration;
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
    internal class KeyCardSpawnable : Spawnable
    {
        private Color _color;
        public override string AssetsFolder => Mod.GetAssetPath();

        public KeyCardSpawnable() : base("StaffKeyCard", "Staff Key Card", "Fredo Jr. Lachance Staff Key Card")
        {
            OnFinishedPatching += () =>
            {
                Mod.StaffKeyCardTechType = TechType;
                CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(TechType,new Vector3(69.8f, -309.2f, -1418.7f),new Quaternion(-0.1f, -0.5f, 0.4f, 0.8f)));
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(AlterraHub.StaffKeyCardPrefab);

                prefab.transform.localScale = new Vector3(0.40f,0.40f,0.40f);

                prefab.AddComponent<PrefabIdentifier>();
                prefab.AddComponent<TechTag>().type = TechType;

                var lw = prefab.AddComponent<LargeWorldEntity>();
                lw.cellLevel = LargeWorldEntity.CellLevel.Near;

                var pickUp = prefab.AddComponent<Pickupable>();
                pickUp.randomizeRotationWhenDropped = true;
                pickUp.isPickupable = true;

                var rigidBody = prefab.EnsureComponent<Rigidbody>();

                // Make the object drop slowly in water
                var wf = prefab.AddComponent<WorldForces>();
                wf.underwaterGravity = 0;
                wf.underwaterDrag = 10f;
                wf.enabled = true;
                wf.useRigidbody = rigidBody;


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
