using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FCSCommon.Controllers;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using QuantumTeleporter.Configuration;
using QuantumTeleporter.Managers;
using QuantumTeleporter.Mono;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace QuantumTeleporter.Buildable
{
    using SMLHelper.V2.Assets;
    internal partial class QuantumTeleporterBuildable : Buildable
    {
        private static readonly QuantumTeleporterBuildable Singleton = new QuantumTeleporterBuildable();
        public sealed override TechType RequiredForUnlock => TechType.PrecursorIonCrystal;

        public QuantumTeleporterBuildable() : base(Mod.ClassID, Mod.FriendlyName, Mod.Description)
        {
            OnFinishedPatching = () =>
            {
                AdditionalPatching();
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                prefab.name = this.PrefabFileName;

                //var ping = prefab.EnsureComponent<PingInstance>();
                //ping.origin = prefab.transform;
                //ping.pingType = PingType.Signal;


                var center = new Vector3(0f, 1.433978f, 0f);
                var size = new Vector3(2.274896f, 2.727271f, 2.069269f);
                GameObjectHelpers.AddConstructableBounds(prefab, size, center);
                
                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        public override string AssetsFolder { get; } = $"{Mod.ModFolderName}/Assets";
#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.QuantumTeleporterKitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.QuantumTeleporterKitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }

#endif

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;

        public static void PatchSMLHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                //throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
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
                _prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                _prefab.AddComponent<QTDisplayManager>();
                _prefab.AddComponent<QTDoorManager>();
                _prefab.AddComponent<QuantumTeleporterController>();
            }
        }
    }
}
