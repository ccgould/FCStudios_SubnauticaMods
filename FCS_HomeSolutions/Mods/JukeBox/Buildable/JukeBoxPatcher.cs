﻿using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.JukeBox.Mono;
using FCSCommon.Utilities;
using SMLHelper.Crafting;
using SMLHelper.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.JukeBox.Buildable
{
    internal class JukeBoxBuildable : SMLHelper.Assets.Buildable
    {
        private readonly GameObject _prefab;
        private UnityEngine.Sprite _spritePlay;
        private UnityEngine.Sprite _spritePause;
        private UnityEngine.Sprite _spriteKnobHover;
        private UnityEngine.Sprite _spriteKnobNormal;
        private UnityEngine.Sprite _spriteShuffleOn;
        private UnityEngine.Sprite _spriteShuffleOff;
        private UnityEngine.Sprite[] _spritesRepeat;
        internal const string JukeBoxClassID = "FCSJukebox";
        internal const string JukeBoxFriendly = "Jukebox";
        internal const string JukeBoxDescription = "Wall-mounted Alterra Jukebox. Plays your favorite music to help you develop a relaxed attitude toward danger.";
        internal const string JukeBoxPrefabName = "Jukebox";
        internal const string JukeBoxKitClassID = "Jukebox_Kit";
        internal const string JukeBoxTabID = "JB";

        public JukeBoxBuildable() : base(JukeBoxClassID, JukeBoxFriendly, JukeBoxDescription)
        {
            _prefab = ModelPrefab.GetPrefab(JukeBoxPrefabName);
            _spritePlay = ModelPrefab.ModBundle.LoadAsset<UnityEngine.Sprite>("JukeboxIconPlay-resources.assets-4449");
            _spritePause = ModelPrefab.ModBundle.LoadAsset<UnityEngine.Sprite>("JukeboxIconPause-resources.assets-4730");
            _spriteKnobHover = ModelPrefab.ModBundle.LoadAsset<UnityEngine.Sprite>("JukeboxKnobHover");
            _spriteKnobNormal = ModelPrefab.ModBundle.LoadAsset<UnityEngine.Sprite>("JukeboxKnob");

            _spritesRepeat =  ModelPrefab.ModBundle.LoadAssetWithSubAssets<UnityEngine.Sprite>("JukeboxRepeat");


            var shuffle = ModelPrefab.ModBundle.LoadAssetWithSubAssets<UnityEngine.Sprite>("JukeboxIconShuffle");

            foreach (UnityEngine.Sprite item in shuffle)
            {
                QuickLogger.Info(item.name);
            }



            _spriteShuffleOn = shuffle[0];
            _spriteShuffleOff = shuffle[1];




            OnStartedPatching += () =>
            {
                var jukeboxKit = new FCSKit(JukeBoxKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                jukeboxKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, JukeBoxKitClassID.ToTechType(), 700000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }


        public static T GetSubAsset<T>(UnityEngine.Object[] allAssets) where T : class
        {
            for (int i = 0; i < allAssets.Length; i++)
            {
                if (allAssets[i].GetType() == typeof(T))
                {
                    return allAssets[i] as T;
                }
            }
            return null;
        }

        public static T[] GetSubAssets<T>(UnityEngine.Object[] allAssets) where T : class
        {
            List<T> ret = new List<T>();
            for (int i = 0; i < allAssets.Length; i++)
            {
                if (allAssets[i].GetType() == typeof(T))
                {
                    ret.Add(allAssets[i] as T);
                }
            }
            return ret.ToArray();
        }



        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                prefab.name = this.PrefabFileName;
                
                var center = new Vector3(0f, 0.02002776f, 0.2723739f);
                var size = new Vector3(0.8163573f, 1.504693f, 0.3991752f);

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

                var lod = prefab.AddComponent<BehaviourLOD>();
                lod.veryCloseThreshold= 20f;
                lod.closeThreshold = 20f;
                lod.farThreshold = 20f;
                lod.veryCloseThresholdSq = 400f;
                lod.closeThresholdSq = 400f;
                lod.farThresholdSq = 400f;
                lod.visibilityRendererRoot= prefab;


                prefab.AddComponent<Speaker>();

                prefab.SetActive(false);

                var volumnGO = GameObjectHelpers.FindGameObject(prefab, "Volume");
                var vpt = volumnGO.AddComponent<PointerEventTrigger>();

                var timelineGO = GameObjectHelpers.FindGameObject(prefab, "Timeline");
                var tpt = timelineGO.AddComponent<PointerEventTrigger>();

                var instance = prefab.AddComponent<JukeboxInstance>();

                instance.LOD = lod;

                var file = GameObjectHelpers.FindGameObject(prefab, "Overlay")?.GetComponent<Canvas>();

                var canvas = GameObjectHelpers.FindGameObject(prefab, "Canvas")?.GetComponent<Canvas>();
                
                var renderer = model.GetComponentInChildren<Renderer>();
                //var canvasLink = model.AddComponent<CanvasLink>();
                //canvasLink.canvases = new Canvas[] { file, canvas };
                //canvasLink.rectMasks = new RectMask2D[] { };
                //canvasLink.renderer = renderer;

                //instance.canvasLink = canvasLink;

                instance.imagePlayPause = GameObjectHelpers.FindGameObject(prefab, "PlayBTN").transform.GetChild(0).GetComponent<Image>();

                instance.imageRepeat = GameObjectHelpers.FindGameObject(prefab, "RepeatBTN").transform.GetChild(0).GetComponent<Image>();

                instance.imageShuffle = GameObjectHelpers.FindGameObject(prefab, "ShuffleBTN").transform.GetChild(0).GetComponent<Image>();

                instance.rectMask = file.gameObject.GetComponent<RectMask2D>();

                instance.textFile = GameObjectHelpers.FindGameObject(prefab, "TrackName").GetComponent<TextMeshProUGUI>();

                instance.textPosition = GameObjectHelpers.FindGameObject(prefab, "TextPosition").GetComponent<TextMeshProUGUI>();

                instance.textLength = GameObjectHelpers.FindGameObject(prefab, "TextLength").GetComponent<TextMeshProUGUI>();

                instance.textVolume = GameObjectHelpers.FindGameObject(prefab, "TextVolume").GetComponent<TextMeshProUGUI>();

                instance.imagePosition = GameObjectHelpers.FindGameObject(prefab, "TimelineBackground").GetComponent<Image>();

                instance.imagePositionKnob = GameObjectHelpers.FindGameObject(prefab, "TimeLineHandle").GetComponent<Image>();

                instance.imageVolume = GameObjectHelpers.FindGameObject(prefab, "VolumeBackground").GetComponent<Image>();

                instance.imageVolumeKnob = GameObjectHelpers.FindGameObject(prefab, "VolumeHandle").GetComponent<Image>();

                instance.imagesSpectrum = GameObjectHelpers.FindGameObject(prefab, "Spectrum").GetChildren<Image>();


                volumnGO.AddComponent<uGUI_Block>();
                instance.volumePointEventTrigger = vpt;

                timelineGO.AddComponent<uGUI_Block>();
                instance.timelinePointEventTrigger = tpt;


                instance.spritePause = _spritePause;
                instance.spritePlay = _spritePlay;
                instance.spriteKnobNormal = _spriteKnobNormal;
                instance.spriteKnobHover= _spriteKnobHover;
                instance.spriteShuffleOn = _spriteShuffleOn;
                instance.spriteShuffleOff = _spriteShuffleOff;
                instance.spritesRepeat = _spritesRepeat;

                instance.flashRenderer = renderer;

                prefab.SetActive(true);

                MaterialHelpers.ChangeEmissionStrength(string.Empty,prefab, 7f);

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
                    new Ingredient(JukeBoxKitClassID.ToTechType(), 1)
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