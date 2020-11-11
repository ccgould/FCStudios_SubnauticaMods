using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mono.OutDoorPlanters;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UnityEngine;
using Valve.VR;

namespace FCS_HomeSolutions.Buildables.OutDoorPlanters
{
    internal class DemoBuildingPatch : Buildable
    {
        private GameObject _prefab;
        private Settings _settings;
        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();
        public DemoBuildingPatch(string classId, string friendlyName, string description, GameObject prefab, Settings settings) : base(classId, friendlyName, description)
        {
            _settings = settings;
            _prefab = prefab;
            OnStartedPatching += () =>
            {
                //QuickLogger.Debug("Patched Kit");
                //var outDoorPlanterkit = new FCSKit(_settings.KitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                //outDoorPlanterkit.Patch();
            };

            OnFinishedPatching += () =>
            {
                //FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.SmartPlanterPotKitClassID.ToTechType(), 50, StoreCategory.Home);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                //Disable the object so we can fill in the properties before awake
                GameObjectHelpers.AddConstructableBounds(prefab, _settings.Size, _settings.Center);

                var model = prefab;

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();

                constructable.allowedOutside = true;
                constructable.allowedInBase = false;
                constructable.allowedOnGround = true;
                constructable.allowedOnWall = false;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = false;
                constructable.allowedOnConstructables = true;
                constructable.model = model;
                constructable.techType = TechType;

                var wClip = prefab.AddComponent<WaterClipProxy>();
                //wClip.clipMaterial = null;
                wClip.immovable = true;
                wClip.shape = WaterClipProxy.Shape.Box;
                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }

#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            return Mod.SmartPlanterIngredients;
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            return Mod.SmartPlanterIngredients;
        }
#endif
    }
}
