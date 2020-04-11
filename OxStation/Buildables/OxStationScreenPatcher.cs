using MAC.OxStation.Config;
using MAC.OxStation.Managers;
using MAC.OxStation.Mono;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;
using System.IO;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace MAC.OxStation.Buildables
{
    internal class OxStationScreenBuildable : Buildable
    {
        #region Public Overrides
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        public override string AssetsFolder { get; } = $"{Mod.ModFolderName}/Assets";
        #endregion

        #region Private Members
        private static readonly OxStationScreenBuildable Singleton = new OxStationScreenBuildable();
        private readonly GameObject _prefab = OxStationModelPrefab.OxstationScreenPrefab;
        #endregion

        #region Constructor
        public OxStationScreenBuildable() : base(Mod.ScreenClassID, Mod.ScreenFriendlyName, Mod.ScreenDescription)
        {

        }
        #endregion

        #region Buildable Overrides

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                prefab = GameObject.Instantiate<GameObject>(_prefab);

                prefab.name = this.PrefabFileName;

                PrefabIdentifier prefabID = prefab.EnsureComponent<PrefabIdentifier>();
                prefabID.ClassId = this.ClassID;
                
                prefab.AddComponent<AnimationManager>();
                prefab.AddComponent<OxStationScreenController>();

                var techTag = prefab.EnsureComponent<TechTag>();
                techTag.type = TechType;

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return prefab;
        }

        private void Register()
        {
            if (_prefab != null)
            {
                var meshRenderers = _prefab.GetComponentsInChildren<MeshRenderer>();

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = _prefab.GetComponentsInChildren<Renderer>(true);
                SkyApplier skyApplier = _prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = _prefab.EnsureComponent<Constructable>();
                constructable.allowedOnWall = true;
                constructable.allowedOnGround = false;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.model = _prefab.FindChild("model");
                constructable.techType = TechType;
                
                GameObjectHelpers.AddConstructableBounds(_prefab, new Vector3(0.4528692f, 0.6682342f, 0.03642998f), 
                    new Vector3(0, -0.02660185f, 0.05301314f));
            }
        }

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
                    new Ingredient(Mod.OxstationScreenKitClassID.ToTechType(),1)
                }
            };
            QuickLogger.Debug($"Created Ingredients");
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
                    new Ingredient(Mod.OxstationScreenKitClassID.ToTechType(),1)
                }
            };
            QuickLogger.Debug($"Created Ingredients");
            return customFabRecipe;
        }

#endif
        #endregion

        #region Internal Methods
        /// <summary>
        /// Patches the mod for use in subnautica
        /// </summary>
        internal static void PatchHelper()
        {
            if (!OxStationModelPrefab.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }
            Singleton.Register();
            Singleton.Patch();
        }
        #endregion
    }
}
