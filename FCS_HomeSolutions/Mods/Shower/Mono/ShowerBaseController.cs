using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.Shower.Buildable;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Shower.Mono
{
    internal class ShowerBaseController : FcsDevice , IFCSSave<SaveData>, IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private ShowerDataEntry _saveData;
        private DoorController _showerDoor;


        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, ShowerBuildable.ShowerTabID, Mod.ModPackID);
        }

        public override float GetPowerUsage()
        {
            return 0.01f;
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
                _colorManager.ChangeColor(_saveData.Fcs.Vector4ToColor());
                _colorManager.ChangeColor(_saveData.Secondary.Vector4ToColor(), ColorTargetMode.Secondary);
                _fromSave = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetShowerSaveData(id);
        }
        
        public override void Initialize()
        {
            GameObjectHelpers.FindGameObject(gameObject, "ShowerControls").EnsureComponent<ShowerController>();

            _showerDoor = GameObjectHelpers.FindGameObject(gameObject, "DoorAnimController").EnsureComponent<DoorController>();
            _showerDoor.ClosePos = 0f;
            _showerDoor.OpenPos = 161.112f;
            
            var showerLightController = GameObjectHelpers.FindGameObject(gameObject, "ShowerTrigger").EnsureComponent<LightController>();
            showerLightController.TargetLight = GameObjectHelpers.FindGameObject(gameObject, "ShowerLight").GetComponent<Light>();
            
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject,AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }
            
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, new Color(0, 1, 1, 1));
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseSecondaryCol, gameObject, new Color(0.8f, 0.4933333f, 0f));
            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseDecalsEmissiveController, gameObject,  2.5f);

            _colorManager.ChangeColor(new Color(0.8f, 0.4933333f, 0f), ColorTargetMode.Secondary);
            
            IsInitialized = true;
            
            QuickLogger.Debug($"Initialized");
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.AlienChefFriendly}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.AlienChefFriendly}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new ShowerDataEntry();
            }
            _saveData.Id = id;
            _saveData.Fcs = _colorManager.GetColor().ColorToVector4();
            _saveData.Secondary = _colorManager.GetSecondaryColor().ColorToVector4();

            newSaveData.ShowerEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
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

        public override void OnHandHover(GUIHand hand)
        {
            if(!IsInitialized || !IsConstructed) return;
            base.OnHandHover(hand);

            var data = new[]
            {
                $"{AlterraHub.PowerPerMinute(GetPowerUsage() * 60)}"
            };
            data.HandHoverPDAHelperEx(GetTechType());
        }

        public void OnHandClick(GUIHand hand)
        {
            
        }
    }
}