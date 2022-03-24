using System;
using System.Collections;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.Mods.WindSurfer.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#endif

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
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.WindSurferKitClassID.ToTechType(), 243000, StoreCategory.Energy);
                KnownTechHandler.SetAnalysisTechEntry(Mod.WindSurferOperatorClassName.ToTechType(), new TechType[1] { TechType });
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

                //prefab.AddComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
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

                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, prefab, Color.cyan);
                MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, prefab, 4f);
                MaterialHelpers.ApplyEmissionShader("fcs_WS_BP", "fcs_WS_E",prefab,ModelPrefab.ModBundle,Color.white);
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);


                //Add VFXSurfaces to adjust footstep sounds. This is technically not necessary for the interior colliders, however.
                foreach (Collider col in prefab.GetComponentsInChildren<Collider>())
                {
                    var vfxSurface = col.gameObject.AddComponent<VFXSurface>();
                    vfxSurface.surfaceType = VFXSurfaceTypes.metal;
                }

                var ps = prefab.AddComponent<PowerSource>();
                ps.maxPower = 500f;

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

            yield break;
        }
#endif
        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
