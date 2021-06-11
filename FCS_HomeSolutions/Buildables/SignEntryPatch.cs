using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Buildables
{
    internal class SignEntryPatch : Buildable
    {
        protected GameObject _prefab;
        protected Settings _settings;

        public override TechGroup GroupForPDA => _settings.GroupForPDA;
        public override TechCategory CategoryForPDA => _settings.CategoryForPDA;
        public override string AssetsFolder => Mod.GetAssetPath();

        public SignEntryPatch(string classId, string friendlyName, string description, GameObject prefab, Settings settings) : base(classId, friendlyName, description)
        {
            _prefab = prefab;
            _settings = settings;

            OnStartedPatching += () =>
            {
                QuickLogger.Debug("Patched Kit");
                var kit = new FCSKit(settings.KitClassID, friendlyName, Path.Combine(AssetsFolder, $"{classId}.png"));
                kit.Patch();
            };

            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, settings.KitClassID.ToTechType(), settings.Cost, StoreCategory.Home);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                GameObjectHelpers.AddConstructableBounds(prefab, _settings.Size, _settings.Center);

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

                constructable.allowedOutside = _settings.AllowedOutside;
                constructable.allowedInBase = _settings.AllowedInBase;
                constructable.allowedOnGround = _settings.AllowedOnGround;
                constructable.allowedOnWall = _settings.AllowedOnWall;
                constructable.rotationEnabled = _settings.RotationEnabled;
                constructable.allowedOnCeiling = _settings.AllowedOnCeiling;
                constructable.allowedInSub = _settings.AllowedInSub;
                constructable.allowedOnConstructables = _settings.AllowedOnConstructables;
                constructable.model = model;
                constructable.techType = TechType;

                prefab.AddComponent<PrefabIdentifier>().ClassId = ClassID;
                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<SignController>();

                //Apply the glass shader here because of autosort lockers for some reason doesn't like it.
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
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(_settings.KitClassID.ToTechType(),1)
                }
            };
            QuickLogger.Debug($"Created Ingredients");
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }

    internal class SignController : FcsDevice, IFCSSave<SaveData>
    {
        private Image _arrow;
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private SignDataEntry _savedData;
        private LabelController _labelController;


        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, "ASN", Mod.ModPackID);
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
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    _colorManager.ChangeColor(_savedData.Body.Vector4ToColor());
                    _arrow.transform.rotation = _savedData.ArrowDirection.Vec4ToQuaternion();
                    _labelController.SetText(_savedData.SignName);
                }

                _runStartUpOnEnable = false;
            }
        }
        
        public override void Initialize()
        {
            if (IsInitialized) return;
            
            _labelController = GameObjectHelpers.FindGameObject(gameObject, "SignLabel").AddComponent<LabelController>();
            _labelController.Initialize();

            if (_arrow == null)
            {
                _arrow = gameObject.GetComponentInChildren<Image>();
                _arrow.gameObject.AddComponent<ArrowClickHandler>();
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject,ModelPrefab.BodyMaterial);
            }

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

            if (_savedData == null)
            {
                ReadySaveData();
            }

            _isFromSave = true;
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new SignDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.Body = _colorManager.GetColor().ColorToVector4();
            _savedData.SignName = _labelController.GetText();
            _savedData.ArrowDirection = _arrow.transform.rotation.QuaternionToVec4();
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.SignDataEntries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetSignDataEntrySaveData(GetPrefabID());
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
    }

    internal class LabelController : OnScreenButton, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private NameController _nameController;
        private Text _text;
        private bool _initialized;

        internal void Initialize()
        {
            if(_initialized) return;
            _text = gameObject.GetComponent<Text>();

            if (_nameController == null)
            {
                _nameController = gameObject.AddComponent<NameController>();
                _nameController.Initialize("Submit", "Rename Sign");
                _nameController.OnLabelChanged += (s, controller) => { _text.text = s; };
                _nameController.SetCurrentName("SIGN NAME");
                _nameController.SetMaxChar(100);
            }

            TextLineOne = "Click change the sign text.";

            _initialized = true;
        }

        public override void OnPointerClick(PointerEventData pointerEventData)
        {
            _nameController.Show();
        }

        public void SetText(string signName)
        {
            Initialize();
            _text.text = signName;
            _nameController.SetCurrentName(signName);
        }

        public string GetText()
        {
            Initialize();
            return _nameController.GetCurrentName();
        }
    }

    internal class ArrowClickHandler : OnScreenButton, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Transform _transform;

        private void Start()
        {
            _transform = transform;
            TextLineOne = "Click to change the arrow direction";
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            _transform.rotation = Quaternion.Euler(_transform.rotation.eulerAngles.x, _transform.rotation.eulerAngles.y, _transform.rotation.eulerAngles.z - 90f);
        }
    }
}
