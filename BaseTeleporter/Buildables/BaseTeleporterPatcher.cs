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

                if (prefab != null)
                {
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
                    constructable.allowedInBase = true;
                    constructable.allowedOnCeiling = false;
                    constructable.allowedOutside = false;
                    constructable.model = model;
                    constructable.rotationEnabled = true;
                    constructable.techType = TechType;

                    PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                    prefabID.ClassId = ClassID;

                    prefab.AddComponent<AnimationManager>();
                    prefab.AddComponent<TeleportManager>();
                    prefab.AddComponent<BTDisplayManager>();
                    prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                    prefab.AddComponent<BaseTeleporterController>();
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return prefab;
        }

        public override string AssetsFolder { get; } = $"{Mod.ModFolderName}/Assets";
        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechTypeHelpers.GetTechType("IntraBaseTeleporterKit_AE"), 1)
                }
            };
            return customFabRecipe;
        }

        public override string IconFileName => "BaseTeleporter.png";

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
            
        }
    }
}
