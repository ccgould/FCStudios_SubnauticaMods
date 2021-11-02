using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.AutoCrafter.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Buildable
{
    internal class AutoCrafterPatch : SMLHelper.V2.Assets.Buildable
    {
        private readonly GameObject _prefab;

        internal const string AutoCrafterTabID = "ACU";
        internal const string AutoCrafterFriendlyName = "Auto Crafter";
        private const string AutoCrafterClassName = "AutoCrafter";
        private const string AutoCrafterPrefabName = "AutoCraftMachine";
        private static string AutoCrafterKitClassID => $"{AutoCrafterClassName}_Kit";
        private const string AutoCrafterDescription = "Avoid long hours in front of the Fabricator. Queue up a list of multiple items or just keep yourself automatically stocked on an important one.";

        public override TechGroup GroupForPDA => TechGroup.Miscellaneous;
        public override TechCategory CategoryForPDA => TechCategory.Misc;
        public override string AssetsFolder => Mod.GetAssetPath();

        public AutoCrafterPatch() : base(AutoCrafterClassName, AutoCrafterFriendlyName, AutoCrafterDescription)
        {

            _prefab = ModelPrefab.GetPrefab(AutoCrafterPrefabName);

            OnFinishedPatching += () =>
            {
                var AutoCrafterKit = new FCSKit(AutoCrafterKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                AutoCrafterKit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, AutoCrafterKitClassID.ToTechType(), 236250, StoreCategory.Production);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                var center = new Vector3(0f, 1.33581f, 0.1292717f);
                var size = new Vector3(1.87915f, 2.327191f, 1.775631f);


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

                var lw = prefab.AddComponent<LargeWorldEntity>();
                lw.cellLevel = LargeWorldEntity.CellLevel.Global;

                prefab.AddComponent<TechTag>().type = TechType;
                var craftMachine = prefab.AddComponent<CraftMachine>();
                var controller = prefab.AddComponent<AutoCrafterController>();
                craftMachine.Crafter = controller;
                controller.Storage =  UWEHelpers.CreateStorageContainer(prefab, null, ClassID, string.Empty, 4, 8);

                //Apply the glass shader here because of autosort lockers for some reason doesn't like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
                MaterialHelpers.ApplyShaderToMaterial(prefab, "_ConveyorBelt");

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData
            {
                craftAmount = 1,
                Ingredients =
                {
                    new Ingredient(AutoCrafterKitClassID.ToTechType(), 1),
                }
            };
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}