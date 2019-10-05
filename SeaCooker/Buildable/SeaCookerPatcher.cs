using AE.SeaCooker.Configuration;
using AE.SeaCooker.Managers;
using AE.SeaCooker.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AE.SeaCooker.Buildable
{
    using SMLHelper.V2.Assets;
    using System;
    internal partial class SeaCookerBuildable : Buildable
    {
        private static readonly SeaCookerBuildable Singleton = new SeaCookerBuildable();

        //FCSAESeaCooker
        public SeaCookerBuildable() : base(Mod.ClassID, Mod.FriendlyName, Mod.Description)
        {
            OnFinishedPatching += AdditionalPatching;
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {

                prefab = GameObject.Instantiate(_prefab);

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
                    new Ingredient(TechType.Titanium, 1)

                    //new Ingredient(TechTypeHelpers.GetTechType("SeaCookerKit_SC"), 1)
                }
            };
            return customFabRecipe;
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;

        public static void PatchSMLHelper()
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
                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = _prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

                var model = _prefab.FindChild("model");

                SkyApplier skyApplier = _prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //Shader shader = Shader.Find("MarmosetUBER");
                //Renderer[] renderers = _prefab.GetComponentsInChildren<Renderer>();
                //foreach (Renderer renderer in renderers)
                //{
                //    renderer.material.shader = shader;
                //}

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = _prefab.AddComponent<Constructable>();
                constructable.allowedOnWall = true;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.model = model;
                constructable.rotationEnabled = true;
                constructable.techType = Singleton.TechType;

                PrefabIdentifier prefabID = _prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = Singleton.ClassID;

                _prefab.AddComponent<AnimationManager>();
                _prefab.AddComponent<TechTag>().type = Singleton.TechType;
                _prefab.AddComponent<PlayerInteraction>();
                _prefab.AddComponent<SeaCookerController>();
            }
        }
    }
}
