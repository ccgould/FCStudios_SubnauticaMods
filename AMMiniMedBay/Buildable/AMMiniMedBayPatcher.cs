using AMMiniMedBay.Mono;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using AMMiniMedBay.Configuration;
using FCSCommon.Helpers;

namespace AMMiniMedBay.Buildable
{
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Crafting;
    using UnityEngine;

    internal partial class AMMiniMedBayBuildable : Buildable
    {
        private static readonly AMMiniMedBayBuildable Singleton = new AMMiniMedBayBuildable();
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        public override string AssetsFolder { get; } = $"{Mod.ModFolderName}/Assets";

        public AMMiniMedBayBuildable() : base(Mod.ClassID, Mod.ModFriendlyName, Mod.ModDescription)
        {
            OnFinishedPatching += AdditionalPatching;
        }

        public static void PatchHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }

            Singleton.Patch();
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                prefab = GameObject.Instantiate(_Prefab);

                var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.EnsureComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.model = prefab.FindChild("model");
                constructable.techType = TechType;
                constructable.rotationEnabled = true;
                
                var center = new Vector3(0.04392624f, 1.421124f, 0f);
                var size = new Vector3(2.401972f, 2.700523f, 2.280661f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                prefab.EnsureComponent<PrefabIdentifier>().ClassId = this.ClassID;
                prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                prefab.EnsureComponent<AMMiniMedBayController>();

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechTypeHelpers.GetTechType("AMMiniMedBayKit_AMS"), 1)
                }
            };
            QuickLogger.Debug($"Created Ingredients");
            return customFabRecipe;
        }
    }
}