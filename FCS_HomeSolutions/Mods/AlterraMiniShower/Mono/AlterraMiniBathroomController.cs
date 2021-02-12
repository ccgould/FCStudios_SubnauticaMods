using System;
using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FMODUnity;
using rail;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.AlterraMiniShower.Mono
{
    internal class AlterraMiniBathroomController : FcsDevice , IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private AlterraMiniBathroomDataEntry _saveData;
        private DoorController _showerDoor;
        private DoorController _toiletDoor;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, "AMB", Mod.ModName);
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
            _saveData = Mod.GetAlterraMiniBathroomSaveData(id);
        }
        
        public override void Initialize()
        {
            GameObjectHelpers.FindGameObject(gameObject, "ShowerControls").EnsureComponent<ShowerController>();

            _showerDoor = GameObjectHelpers.FindGameObject(gameObject, "ShowerDoor").EnsureComponent<DoorController>();
            _showerDoor.ClosePos = -0.01301698f;
            _showerDoor.OpenPos = 1.2f;
            
            _toiletDoor = GameObjectHelpers.FindGameObject(gameObject, "ToiletDoor").EnsureComponent<DoorController>();
            _toiletDoor.ClosePos = 0.003515291f;
            _toiletDoor.OpenPos = -1.2f;

            var toiletLightController = GameObjectHelpers.FindGameObject(gameObject, "ToiletTrigger").EnsureComponent<LightController>();
            toiletLightController.TargetLight = GameObjectHelpers.FindGameObject(gameObject, "ToiletLight").GetComponent<Light>();
            
            var showerLightController = GameObjectHelpers.FindGameObject(gameObject, "ShowerTrigger").EnsureComponent<LightController>();
            showerLightController.TargetLight = GameObjectHelpers.FindGameObject(gameObject, "ShowerLight").GetComponent<Light>();
            
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject,AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }
            
            //var soundemitter = gameObject.GetComponentInChildren<FMOD_CustomEmitter>();
            
            //if (soundemitter != null)
            //{
            //    soundemitter.evt = ModelPrefab.ShowerLoop;
            //    soundemitter.Play();
            //}
            
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseEmissiveDecalsController, gameObject, new Color(0, 1, 1, 1));
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseSecondaryCol, gameObject, new Color(0.8f, 0.4933333f, 0f));
            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseEmissiveDecalsController, gameObject,  2.5f);

            _colorManager.ChangeColor(new Color(0.8f, 0.4933333f, 0f), ColorTargetMode.Secondary);

            DeleteChairDuplicates();

            IsInitialized = true;
            
            QuickLogger.Debug($"Initialized");
        }

        private void DeleteChairDuplicates()
        {
            var getChairs = GameObjectHelpers.FindGameObjects(gameObject, "StarshipChair(Clone)").ToArray();
            if (getChairs.Length > 1)
            {
                for (int i = 1; i < getChairs.Length; i++)
                {
                    Destroy(getChairs[i]);
                }
            }

            if (getChairs[0] != null)
            {
                Renderer[] renderers = getChairs[0].GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers)
                {
                    rend.enabled = false;
                }
            }
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
                _saveData = new AlterraMiniBathroomDataEntry();
            }
            _saveData.Id = id;
            _saveData.Fcs = _colorManager.GetColor().ColorToVector4();
            _saveData.Secondary = _colorManager.GetSecondaryColor().ColorToVector4();

            newSaveData.AlterraMiniBathroomEntries.Add(_saveData);
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
    }
}