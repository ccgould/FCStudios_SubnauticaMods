using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.Stairs.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.Stairs.Buildable
{
    internal class StairsBuildable : SMLHelper.V2.Assets.Buildable
    {
        private GameObject _prefab;
        internal const string StairsClassID = "FCSStairs";
        internal const string StairsFriendly = "Door Stairs";
        internal const string StairsDescription = "N/A";
        internal const string StairsPrefabName = "Small_PlatformDoorStairs";
        internal const string StairsKitClassID = "Stairs_Kit";
        internal const string StairsTabID = "DST";
        public StairsBuildable() : base(StairsClassID, StairsFriendly, StairsDescription)
        {
           _prefab = ModelPrefab.GetPrefab(StairsPrefabName);

            OnStartedPatching += () =>
            {
                var Elevator = new FCSKit(StairsKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                Elevator.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, StairsKitClassID.ToTechType(), 45000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                prefab.name = this.PrefabFileName;

                var center = new Vector3(0f, 0.8684514f, 1.119845f);
                var size = new Vector3(3.433679f, 1.182833f, 1.716091f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Near;

                var model = prefab.FindChild("model");

                SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();
                constructable.allowedInBase = true;
                constructable.allowedInSub = true;
                constructable.allowedOutside = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOnGround = true;
                constructable.allowedOnConstructables = true;
                constructable.controlModelState = true;
                constructable.deconstructionAllowed = true;
                constructable.rotationEnabled = true;
                constructable.model = model;
                constructable.techType = base.TechType;
                constructable.surfaceType = VFXSurfaceTypes.metal;
                constructable.placeMinDistance = 0.6f;
                constructable.surfaceType = VFXSurfaceTypes.metal;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                //Add VFXSurfaces to adjust footstep sounds. This is technically not necessary for the interior colliders, however.
                var platformCollision1 = GameObjectHelpers.FindGameObject(prefab, "Unity_Collison_Object_0_3");
                var platformCollision2 = GameObjectHelpers.FindGameObject(prefab, "Unity_Collison_Object_0_4");

                var platformCollisionVfxSurface1 = platformCollision1.AddComponent<VFXSurface>();
                platformCollisionVfxSurface1.surfaceType = VFXSurfaceTypes.metal;


                var platformCollisionVfxSurface2 = platformCollision1.AddComponent<VFXSurface>();
                platformCollisionVfxSurface2.surfaceType = VFXSurfaceTypes.metal;


                prefab.AddComponent<TechTag>().type = TechType;
                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, prefab, Color.cyan);
                prefab.AddComponent<StairsController>();
                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        public override string AssetsFolder { get; } = Mod.GetAssetPath();

        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(StairsKitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorModule;
        public static TechType TechTypePatched { get; set; }
    }
}
