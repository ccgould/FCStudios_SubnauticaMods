using System;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.Mods.WindSurfer.Mono;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using UnityEngine;

namespace FCS_EnergySolutions.Spawnables
{
    class WindSurferSpawnable : Spawnable
    {
        public override string AssetsFolder => Mod.GetAssetPath();
        public WindSurferSpawnable() : base(Mod.WindSurferClassName, Mod.WindSurferFriendlyName, Mod.WindSurferDescription)
        {
            OnStartedPatching += () =>
            {
                var WindSurferKit = new FCSKit(Mod.WindSurferKitClassID, Mod.WindSurferFriendlyName,Path.Combine(AssetsFolder, $"{ClassID}.png"));
                WindSurferKit.Patch();
            };

            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.WindSurferKitClassID.ToTechType(), 180000, StoreCategory.Energy);
            };
        }

#if SUBNAUTICA_STABLE
        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.GetPrefab(Mod.WindSurferPrefabName, true)); 

                PrefabIdentifier prefabIdentifier = prefab.AddComponent<PrefabIdentifier>();
                prefabIdentifier.ClassId = this.ClassID;
                prefab.AddComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
                prefab.AddComponent<TechTag>().type = this.TechType;

                var rb = prefab.GetComponentInChildren<Rigidbody>();

                if (rb == null)
                {
                    rb = prefab.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                }

                var pc = prefab.AddComponent<PlatformController>();
                pc.Ports = new[]
                {
                    GameObjectHelpers.FindGameObject(prefab,"Port_1"),
                    GameObjectHelpers.FindGameObject(prefab,"Port_2"),
                    GameObjectHelpers.FindGameObject(prefab,"Port_3"),
                    GameObjectHelpers.FindGameObject(prefab,"Port_4"),
                };
                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseEmissiveDecalsController, prefab, Color.cyan);
                MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseEmissiveDecals, prefab, 4f);

                //Add VFXSurfaces to adjust footstep sounds. This is technically not necessary for the interior colliders, however.
                foreach (Collider col in prefab.GetComponentsInChildren<Collider>())
                {
                    var vfxSurface = col.gameObject.AddComponent<VFXSurface>();
                    vfxSurface.surfaceType = VFXSurfaceTypes.metal;
                }

                prefab.AddComponent<WindSurferController>();
                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Error(e.Message);
                return null;
            }
        }
#else
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {

        }
#endif
    }
}
