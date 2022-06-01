using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.JukeBox.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.JukeBox.Buildable
{
    internal class JukeBoxSpeakerBuildable : SMLHelper.V2.Assets.Buildable
    {
        private readonly GameObject _prefab;
        internal const string JukeBoxSpeakerClassID = "FCSJukeboxSpeaker";
        internal const string JukeBoxSpeakerFriendly = "Jukebox Speaker";
        internal const string JukeBoxSpeakerDescription = "Wall-mounted satellite speaker for the Alterra Jukebox so you can take your relaxed attitude toward danger anywhere inside your base.";
        internal const string JukeBoxSpeakerPrefabName = "JukeBoxSpeaker";
        internal const string JukeBoxSpeakerKitClassID = "JukeboxSpeaker_Kit";
        internal const string JukeBoxSpeakerTabID = "JBS";

        public JukeBoxSpeakerBuildable() : base(JukeBoxSpeakerClassID, JukeBoxSpeakerFriendly, JukeBoxSpeakerDescription)
        {
            _prefab = ModelPrefab.GetPrefab(JukeBoxSpeakerPrefabName);
            OnStartedPatching += () =>
            {
                var jukeboxSpeakerKit = new FCSKit(JukeBoxSpeakerKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                jukeboxSpeakerKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, JukeBoxSpeakerKitClassID.ToTechType(), 100000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                prefab.name = this.PrefabFileName;
                
                var center = new Vector3(-0.003484786f, 0f, 0.1319998f);
                var size = new Vector3(0.2652141f, 0.5541678f, 0.1636116f);

                GameObjectHelpers.AddConstructableBounds(prefab,size,center);

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

                var model = prefab.FindChild("model");

                SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();
                constructable.allowedOnWall = true;
                constructable.allowedOnGround = false;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<JukeBoxSpeakerController>();
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        public override string AssetsFolder { get; } = Mod.GetAssetPath();

        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(JukeBoxSpeakerKitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
    }
}
