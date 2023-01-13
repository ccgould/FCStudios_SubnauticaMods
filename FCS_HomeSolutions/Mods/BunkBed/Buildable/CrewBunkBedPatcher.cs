using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using SMLHelper.Crafting;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.BunkBed.Buildable
{
    internal class CrewBunkBedPatcher : SMLHelper.Assets.Buildable
    {
        private readonly string _kitClassID;
        private readonly GameObject _gameObject;

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;

        internal CrewBunkBedPatcher(string classID, string friendly, string description, string kitClassID,
            string prefabName, decimal cost) : base(classID, friendly, description)
        {
            _kitClassID = kitClassID;
            _gameObject =
                FCSAssetBundlesService.PublicAPI.GetPrefabByName(prefabName,
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName);
            OnStartedPatching += () =>
            {
                var kit = new FCSKit(kitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                kit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, kitClassID.ToTechType(), cost,
                    StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }
        
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var task = new TaskResult<GameObject>();
            yield return CraftData.GetPrefabForTechTypeAsync(TechType.NarrowBed, false, task);
            try
            {
                var prefab = AddChair(task.Get());

                var mesh = GameObject.Instantiate(_gameObject);
                mesh.SetActive(false);
                ProcessBed(prefab, mesh);
                gameObject.Set(prefab);
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                gameObject.Set(null);
            }
        }

        private void ProcessBed(GameObject prefab, GameObject mesh)
        {
            prefab.name = this.PrefabFileName;

            var center = new Vector3(0f, 1.621534f, 0.2475206f);
            var size = new Vector3(2.043587f, 1.623069f, 3.731261f);
            GameObjectHelpers.AddConstructableBounds(mesh, size, center);

            var model = mesh.FindChild("model");
            model.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);

            SkyApplier skyApplier = mesh.AddComponent<SkyApplier>();
            skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
            skyApplier.anchorSky = Skies.Auto;

            //========== Allows the building animation and material colors ==========// 

            //Add constructible
            var constructable = prefab.GetComponent<Constructable>();
            constructable.allowedOnWall = false;
            constructable.allowedOnGround = true;
            constructable.allowedInSub = true;
            constructable.allowedInBase = true;
            constructable.allowedOnCeiling = false;
            constructable.allowedOutside = false;
            constructable.allowedOnConstructables = true;
            constructable.rotationEnabled = true;
            constructable.model = model;
            constructable.techType = TechType;

            mesh.transform.SetParent(prefab.transform, false);
            mesh.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            mesh.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            mesh.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            mesh.SetActive(true);

            PrefabIdentifier prefabID = prefab.GetComponent<PrefabIdentifier>();
            prefabID.ClassId = ClassID;

            prefab.EnsureComponent<TechTag>().type = TechType;

            prefab.AddComponent<BunkBedController>();
            MaterialHelpers.ChangeEmissionColor(string.Empty, mesh, Color.cyan);
            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
        }

        public override string AssetsFolder { get; } = Mod.GetAssetPath();

        public GameObject AddChair(GameObject _narrowBed)
        {
            GameObject narrowBed = GameObject.Instantiate(_narrowBed);

            foreach (Transform tr in narrowBed.transform)
            {
                var localPosition = tr.localPosition;
                localPosition = new Vector3(localPosition.x, localPosition.y + -0.2f, localPosition.z);
                tr.localPosition = localPosition;
            }

            Renderer[] renderers = narrowBed.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                rend.enabled = false;
            }

            // Add large world entity
            var lwe = narrowBed.EnsureComponent<LargeWorldEntity>();
            lwe.cellLevel = LargeWorldEntity.CellLevel.Near;

            var cb = narrowBed.GetComponentInChildren<ConstructableBounds>();
            GameObject.Destroy(cb);
            //cb.bounds.size = Vector3.zero;
            //cb.bounds.position = Vector3.zero;

            var collisions = narrowBed.GetComponentsInChildren<BoxCollider>();
            foreach (BoxCollider boxCollider in collisions)
            {
                GameObject.Destroy(boxCollider);
            }


            return narrowBed;
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new(_kitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }

    internal class BunkBedController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _fromSave;
        private bool _runStartUpOnEnable;
        private DecorationDataEntry _saveData;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, "SF", Mod.ModPackID);
        }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_saveData == null)
            {
                ReadySaveData();
            }

            if (_fromSave)
            {
                _colorManager.LoadTemplate(_saveData.ColorTemplate);
                _fromSave = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDecorationDataEntrySaveData(id);
        }

        public override void Initialize()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol,
                    string.Empty);
                _colorManager.ChangeColor(new ColorTemplate {SecondaryColor = Color.gray});
            }

            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                Mod.Save(serializer);
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new DecorationDataEntry();
            }

            _saveData.Id = id;
            _saveData.ColorTemplate = _colorManager.SaveTemplate();
            newSaveData.DecorationEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }
    }
}