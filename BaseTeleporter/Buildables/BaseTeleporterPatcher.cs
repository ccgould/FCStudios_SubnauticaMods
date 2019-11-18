using System;
using System.Collections.Generic;
using System.IO;
using AE.BaseTeleporter.Configuration;
using AE.BaseTeleporter.Managers;
using AE.BaseTeleporter.Mono;
using FCSCommon.Controllers;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace AE.BaseTeleporter.Buildables
{
    internal partial class BaseTeleporterBuildable : Buildable
    {
        private static readonly BaseTeleporterBuildable Singleton = new BaseTeleporterBuildable();

        public BaseTeleporterBuildable() : base(Mod.ClassID, Mod.FriendlyName, Mod.Description)
        {
            OnFinishedPatching += AdditionalPatching;
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {

                prefab = UnityEngine.Object.Instantiate(_prefab);

                prefab.name = this.PrefabFileName;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return prefab;
        }

        public override string AssetsFolder { get; } = $"{Mod.ModName}/Assets";
        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechTypeHelpers.GetTechType(FCSTechFabricator.Configuration.BaseTeleporterClassID), 1)
                }
            };
            return customFabRecipe;
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;

        internal static void PatchSMLHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }

            Register();

            Singleton.Patch();
        }

        private static void Register()
        {
            if (_prefab != null)
            {
                var model = _prefab.FindChild("model");

                SkyApplier skyApplier = _prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = _prefab.AddComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = false;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.model = model;
                constructable.rotationEnabled = true;
                constructable.techType = Singleton.TechType;

                PrefabIdentifier prefabID = _prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = Singleton.ClassID;

                _prefab.AddComponent<AnimationManager>();
                _prefab.AddComponent<TeleportManager>();
                _prefab.AddComponent<BTDisplayManager>();
                _prefab.AddComponent<TechTag>().type = Singleton.TechType;
                _prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                _prefab.AddComponent<BaseTeleporterController>();
            }
        }
    }
}
