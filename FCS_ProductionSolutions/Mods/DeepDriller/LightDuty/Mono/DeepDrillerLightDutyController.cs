using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono;
using FCS_ProductionSolutions.Mods.DeepDriller.LightDuty.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.Managers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.DeepDriller.LightDuty.Mono
{
    internal class DeepDrillerLightDutyController : DrillSystem, IFCSSave<SaveData>, IHandTarget
    {
        private DeepDrillerLightDutySaveDataEntry _saveData;
        private DeepDrillerLightDutyPowerManager PowerManagerLD;
        private Material _material;
        private const float Speed = 0.1f;

        internal override bool UseOnScreenUi => true;
        public override void Initialize()
        {
            base.Initialize();

            _material = MaterialHelpers.GetMaterial(GameObjectHelpers.FindGameObject(gameObject, "CautionTrimRotor"), "fcs01_BD");

            if (PowerManagerLD == null)
            {
                PowerManagerLD = gameObject.AddComponent<DeepDrillerLightDutyPowerManager>();
                var powerRelay = gameObject.AddComponent<PowerRelay>();
                PowerManagerLD.SetPowerRelay(powerRelay);
                PowerManager = PowerManagerLD;
                PowerManagerLD.Initialize(this);
            }

            AnimationHandler = gameObject.EnsureComponent<FCSDeepDrillerAnimationHandler>();
            AnimationHandler.Initialize(new AnimationCurve(new Keyframe(0, 1.277019f), new Keyframe(1, -0.025f)));

            DeepDrillerContainer.OverrideContainerSize(48);
        }

        internal override void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDeepDrillerLightDutySaveData(id);
        }

        internal override void LoadSave()
        {
            if (_isFromSave && _saveData != null)
            {
                PowerManagerLD.LoadData(_saveData);
                DeepDrillerContainer.LoadData(_saveData.Items);
                if (_saveData.IsFocused)
                {
                    OreGenerator.SetIsFocus(_saveData.IsFocused);
                    OreGenerator.Load(_saveData.FocusOres);
                }
                OreGenerator.SetBlackListMode(_saveData.IsBlackListMode);

                _colorManager.LoadTemplate(_saveData.ColorTemplate);
                CurrentBiome = _saveData.Biome;
                OilHandler.SetOilTimeLeft(_saveData.OilTimeLeft);
                _isBreakSet = _saveData.IsBrakeSet;
                if (!string.IsNullOrWhiteSpace(_saveData.BeaconName))
                {
                    SetPingName(_saveData.BeaconName);
                }
                ToggleVisibility(_saveData.IsPingVisible);
                UpdateEmission();
            }
        }

        #region Unity Methods

        internal override void Update()
        {
            base.Update();

            if (_material != null)
            {
                float offset = Time.time * Speed;
                _material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
            }
        }

        internal override void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, DeepDrillerLightDutyBuildable.DeepDrillerLightDutyTabID, Mod.ModPackID);
            base.Start();
        }

        #endregion

        #region IProtoEventListener

        public override void Save(SaveData saveDataList, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized || _colorManager == null)
            {
                QuickLogger.Error($"Failed to save driller {GetPrefabID()}");
                return;
            }

            if (_saveData == null)
            {
                _saveData = new DeepDrillerLightDutySaveDataEntry();
            }


            QuickLogger.Message($"SaveData = {_saveData}", true);

            _saveData.Id = GetPrefabID();
            _saveData.ColorTemplate = _colorManager.SaveTemplate();

            _saveData.PowerState = PowerManager.GetPowerState();
            _saveData.PowerData = PowerManagerLD.SaveData();

            _saveData.Items = DeepDrillerContainer.SaveData();
            _saveData.Biome = CurrentBiome;
            _saveData.FocusOres = OreGenerator.GetFocusedOres();
            _saveData.IsFocused = OreGenerator.GetIsFocused();
            _saveData.IsBlackListMode = OreGenerator.GetInBlackListMode();
            _saveData.IsBrakeSet = _isBreakSet;


            _saveData.OilTimeLeft = OilHandler.GetOilTimeLeft();
            _saveData.BeaconName = _ping.GetLabel();
            _saveData.IsPingVisible = _ping.visible;
            saveDataList.DeepDrillerLightDutyEntries.Add(_saveData);

        }

        #endregion
        
        public void OnHandClick(GUIHand hand)
        {
            //IMplement ui opening on click
        }

    }
}
