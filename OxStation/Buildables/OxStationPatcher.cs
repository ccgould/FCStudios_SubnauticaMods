using MAC.OxStation.Config;
using MAC.OxStation.Managers;
using MAC.OxStation.Mono;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;
using System.IO;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using UnityEngine;

namespace MAC.OxStation.Buildables
{
    internal partial class OxStationBuildable : Buildable
    {
        #region Public Overrides
        public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorModule;
        public override string AssetsFolder { get; } = $"{Mod.ModFolderName}/Assets";
        #endregion

        #region Private Members
        private static readonly OxStationBuildable Singleton = new OxStationBuildable();
        #endregion

        #region Constructor
        public OxStationBuildable() : base(Mod.ClassID, Mod.FriendlyName, Mod.Description)
        {
            OnFinishedPatching += AdditionalPatching;
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

                var pFX = prefab.AddComponent<PowerFX>();
                
                var attachPoint = prefab.FindChild("model").FindChild("static_objects").FindChild("CylinderNull")?.gameObject;
                
                if (attachPoint == null)
                {
                    QuickLogger.Error("AttachPoint CylinderNull was not found on the Oxstation model");
                }
                else
                {
                    pFX.attachPoint = attachPoint.transform;
                }

                prefab.AddComponent<PowerManager>();
                prefab.AddComponent<PowerRelay>();
                prefab.AddComponent<AnimationManager>();
                prefab.AddComponent<OxStationController>();

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
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = false;
                constructable.allowedInBase = false;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = true;
                constructable.model = _prefab.FindChild("model");
                constructable.techType = TechType;
                constructable.rotationEnabled = true;

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = _prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Global;

                _prefab.AddComponent<PlayerInteractionManager>();
                _prefab.AddComponent<FMOD_CustomLoopingEmitter>();
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
                    new Ingredient(Mod.OxstationKitClassID.ToTechType(),1)
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
                    new Ingredient(Mod.OxstationKitClassID.ToTechType(),1)
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
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }
            Singleton.Register();
            Singleton.Patch();
        }
        #endregion
    }
}
