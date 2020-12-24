using System;
using System.IO;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UnityEngine;
using ColorManager = FCS_AlterraHub.Mono.ColorManager;

namespace FCS_HomeSolutions.Spawnables
{
    internal class ObservationTankBuildable : Buildable
    {
        public override string AssetsFolder => Mod.GetAssetPath();

        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;

        public ObservationTankBuildable() : base(Mod.EmptyObservationTankClassID, Mod.EmptyObservationTankFriendly, Mod.EmptyObservationTankDescription)
        {
            OnFinishedPatching += () =>
            {
                var observationTankKit = new FCSKit(Mod.EmptyObservationTankKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                observationTankKit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Mod.EmptyObservationTankKitClassID.ToTechType(), 120, StoreCategory.Home);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.ObservationTankPrefab);

                var center = new Vector3(0f, 0.4049537f, 0f);
                var size = new Vector3(0.4143996f, 0.4694495f, 0.4182996f);

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
                constructable.allowedOnConstructables = true;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<ObservationTankController>();

                //Apply the glass shader here because of autosort lockers for some reason doesn't like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        protected override TechData GetBlueprintRecipe()
        {
            return Mod.EmptyObservationTankIngredients;
        }
    }

    internal class ObservationTankController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _isFromSave;
        private ObservationTankDataEntry _savedTankData;
        private bool _runStartUpOnEnable;
        public override bool BypassRegisterCheck => true;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DecorationItemTabId, Mod.ModName);
        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_isFromSave)
                {
                    if (_savedTankData == null)
                    {
                        ReadySaveData();
                    }

                    _colorManager.ChangeColor(_savedTankData.Body.Vector4ToColor());
                    _colorManager.ChangeColor(_savedTankData.Secondary.Vector4ToColor(), ColorTargetMode.Secondary);
                    _colorManager.ChangeColor(_savedTankData.Emission.Vector4ToColor(), ColorTargetMode.Emission);
                }

                _runStartUpOnEnable = false;
            }
        }

        public override void Initialize()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.EnsureComponent<ColorManager>();
                _colorManager.Initialize(gameObject,ModelPrefab.BodyMaterial,ModelPrefab.SecondaryMaterial,ModelPrefab.EmissionControllerMaterial);
                MaterialHelpers.ChangeEmissionStrength(ModelPrefab.EmissionControllerMaterial,gameObject,5f);
            }

            DoesTakePower = false;

            IsInitialized = true;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedTankData == null)
            {
                ReadySaveData();
            }

            _isFromSave = true;
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedTankData = Mod.GetObservationTankDataEntrySaveData(GetPrefabID());
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

                    IsInitialized = true;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedTankData == null)
            {
                _savedTankData = new ObservationTankDataEntry();
            }

            _savedTankData.Id = GetPrefabID();
            _savedTankData.Body = _colorManager.GetColor().ColorToVector4();
            _savedTankData.Secondary = _colorManager.GetSecondaryColor().ColorToVector4();
            _savedTankData.Emission = _colorManager.GetLumColor().ColorToVector4();
            QuickLogger.Debug($"Saving ID {_savedTankData.Id}");
            newSaveData.ObservationTankDataEntries.Add(_savedTankData);
        }
    }
}
