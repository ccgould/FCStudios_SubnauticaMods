using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.AlienChef.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif


namespace FCS_HomeSolutions.Mods.AlienChief.Buildables
{
    internal partial class AlienChefBuildable : Buildable
    {
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        public override string AssetsFolder { get; } = Mod.GetAssetPath();

        public AlienChefBuildable() : base(Mod.AlienChefClassID, Mod.AlienChefFriendly, Mod.AlienChefDescription)
        {

            OnStartedPatching += () =>
            {
                var miniFountainKit = new FCSKit(Mod.AlienChiefKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                miniFountainKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.AlienChiefKitClassID.ToTechType(), 105000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.AlienChefPrefab);

                prefab.name = this.PrefabFileName;
                
                var center = new Vector3(0f, 1.467808f, 0f);
                var size = new Vector3(2.108935f, 2.653688f, 1f);

                GameObjectHelpers.AddConstructableBounds(prefab,size,center);

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
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.rotationEnabled = true;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;

                prefab.SetActive(false);
                var ss = prefab.AddComponent<FCSStorage>();
                ss.Initialize(48, 6, 8, "Alien Chef Storage", Mod.AlienChefClassID);
                prefab.SetActive(true);

                prefab.AddComponent<AlienChefController>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }
        
        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            return Mod.AlienChiefIngredients;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
