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
using FCS_HomeSolutions.Mods.Elevator.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.Elevator.Buildable
{
    internal class ElevatorBuildable : SMLHelper.V2.Assets.Buildable
    {
        internal const string ElevatorClassID = "Elevator";
        internal const string ElevatorFriendly = "Elevator";
        internal const string ElevatorDescription = "Get from one elevation to the next with ease. Even bring your PRAWN suit along so you don’t get lonely.";
        internal const string ElevatorPrefabName = "Elevator";
        internal const string ElevatorKitClassID = "Elevator_Kit";
        internal const string ElevatorTabID = "EV";
        public ElevatorBuildable() : base(ElevatorClassID, ElevatorFriendly, ElevatorDescription)
        {

            OnStartedPatching += () =>
            {
                var Elevator = new FCSKit(ElevatorKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                Elevator.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, ElevatorKitClassID.ToTechType(), 45000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.ElevatorPrefab);

                prefab.name = this.PrefabFileName;
                
                var center = new Vector3(0f, 1.040041f, 0.4868276f);
                var size = new Vector3(5.322001f, 1.899893f, 6.316751f);

                GameObjectHelpers.AddConstructableBounds(prefab,size,center);

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
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = false;
                constructable.allowedInBase = false;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = true;
                constructable.forceUpright = true;
                constructable.rotationEnabled = true;
                constructable.placeMaxDistance = 10f;
                constructable.placeDefaultDistance = 5f;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                //Add VFXSurfaces to adjust footstep sounds. This is technically not necessary for the interior colliders, however.
                var platformCollision = GameObjectHelpers.FindGameObject(prefab, "Unity_Collison_Object_0_10");
                var platformCollisionVfxSurface = platformCollision.AddComponent<VFXSurface>();
                platformCollisionVfxSurface.surfaceType = VFXSurfaceTypes.metal;


                prefab.AddComponent<TechTag>().type = TechType;
                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, prefab, Color.cyan);
                prefab.AddComponent<FCSElevatorController>();
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
                    new Ingredient(ElevatorKitClassID.ToTechType(), 1)
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
    }
}
