using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_LifeSupportSolutions.Buildable;
using FCS_LifeSupportSolutions.Configuration;
using FCS_LifeSupportSolutions.Mods.EnergyPillVendingMachine.mono;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Mods.EnergyPillVendingMachine.Buildable
{
    internal class EnergyPillVendingMachinePatcher : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        private string _assetFolder => Mod.GetAssetFolder();
        public override string AssetsFolder => _assetFolder;

        public EnergyPillVendingMachinePatcher() : base(Mod.EnergyPillVendingMachineClassName, Mod.EnergyPillVendingMachineFriendlyName, Mod.EnergyPillVendingMachineDescription)
        {
            OnStartedPatching += () =>
            {
                var powerStorageKit = new FCSKit(Mod.EnergyPillVendingMachineKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                powerStorageKit.Patch();
            };

            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.EnergyPillVendingMachineKitClassID.ToTechType(), 50550, StoreCategory.LifeSupport);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.EnergyPillVendingMachinePrefab);

                var center = new Vector3(0f, 1.150356f, 0.0657346f);
                var size = new Vector3(1.355157f, 2.173833f, 0.8685308f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                var model = prefab.FindChild("model");

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();
                constructable.allowedOutside = false;
                constructable.allowedInBase = true;
                constructable.allowedOnGround = true;
                constructable.allowedOnWall = false;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = true;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;
                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<EnergyPillVendingMachineController>();

                //Apply the glass shader here because of autosort lockers for some reason doesn't like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);

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
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            return Mod.EnergyPillVendingMachineIngredients;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return new Atlas.Sprite(ImageUtils.LoadTextureFromFile(Path.Combine(_assetFolder, $"{ClassID}.png")));
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            return Mod.EnergyPillVendingMachineIngredients;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder, $"{ClassID}.png"));
        }
#endif
    }
}
