using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.LedLights.Mono;
using FCSCommon.Utilities;
using SMLHelper.Crafting;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
using Sprite = Atlas.Sprite;

#endif
namespace FCS_HomeSolutions.Mods.LedLights.Buildable
{
    internal class LedLightPatch : SMLHelper.Assets.Buildable
    {
        private LedLightData _data;
        public override TechGroup GroupForPDA { get; }
        public override TechCategory CategoryForPDA { get; }
        public override string AssetsFolder => Mod.GetAssetPath();

        private TechType kitTechType;

        public LedLightPatch(LedLightData data) : base(data.classId, data.friendlyName, data.description)
        {
            _data = data;
            GroupForPDA = data.groupForPda;
            CategoryForPDA = data.categoryForPDA;
            
            OnStartedPatching += () =>
            {
                var ledLightKit = new FCSKit($"{data.classId}_Kit", FriendlyName,
                    Path.Combine(AssetsFolder, $"{ClassID}.png"));
                ledLightKit.Patch();
                kitTechType = ledLightKit.TechType;

            };

            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, $"{data.classId}_Kit".ToTechType(), 33750, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_data.prefab);

                GameObjectHelpers.AddConstructableBounds(prefab, _data.size, _data.center);

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

                constructable.allowedOutside = _data.allowedOutside;
                constructable.allowedInBase = _data.allowedInBase;
                constructable.allowedOnGround = _data.allowedOnGround;
                constructable.allowedOnWall = _data.allowedOnWall;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = _data.allowedInSub;
                constructable.allowedOnConstructables = _data.allowedOnContructables;
                constructable.model = model;
                constructable.techType = TechType;

                var lw = prefab.EnsureComponent<LargeWorldEntity>();
                lw.cellLevel = LargeWorldEntity.CellLevel.Global;


                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<LedLightController>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
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
            return new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new System.Collections.Generic.List<Ingredient>()
                    {
                        new Ingredient(kitTechType, 1)
                    }
            };
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
    internal struct LedLightData
    {
        public string classId;
        public string friendlyName;
        public string description;
        public Vector3 size;
        public Vector3 center;
        public bool allowedOnWall;
        public bool allowedOutside;
        public TechGroup groupForPda;
        public TechCategory categoryForPDA;
        public bool allowedInBase;
        public bool allowedOnGround;
        public bool allowedInSub;
        public GameObject prefab;
        public bool allowedOnContructables;
    }
}
