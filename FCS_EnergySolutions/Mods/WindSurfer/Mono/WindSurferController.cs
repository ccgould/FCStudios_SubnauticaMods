using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods;
using FCS_AlterraHub.Mods.OreConsumer.Model;
using FCS_AlterraHub.Objects;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.Mods.TelepowerPylon.Buildable;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    internal class WindSurferController : WindSurferPlatformBase
    {
        private MotorHandler _motor;
        private bool _fromSave;
        private WindSurferDataEntry _savedData;
        private double _timeStartGrowth;
        private bool _allowedToExtend;
        private float maxProgress = 1f;
        private float _pole1Max = 6.79f;
        private float _pole2Max = 4.7f;
        private float _pole3Max = 5.2f;
        private AnimationCurve _pole1AnimationCurve;
        private AnimationCurve _pole2AnimationCurve;
        private AnimationCurve _pole3AnimationCurve;

        public override WindSurferPowerController PowerController { get; set; }
        public override PlatformController PlatformController => _platformController ?? (_platformController = GetComponent<PlatformController>());
        private PlatformController _platformController;
        private Transform _pole1Trans;
        private Transform _pole2Trans;
        private Transform _pole3Trans;
        public override bool BypassRegisterCheck { get; } = true;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.WindSurferTabID, Mod.ModPackID);

            _pole1AnimationCurve = new AnimationCurve(new Keyframe(0, 4.42813f), new Keyframe(1, _pole1Max));
            _pole2AnimationCurve = new AnimationCurve(new Keyframe(0, 1.841746f), new Keyframe(1, _pole2Max));
            _pole3AnimationCurve = new AnimationCurve(new Keyframe(0, 1.397793f), new Keyframe(1, _pole3Max));
            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, gameObject, 4f);
            Manager.NotifyByID(TelepowerPylonBuildable.TelepowerPylonTabID, "PylonBuilt");
        }

        private void OnEnable()
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            if (_savedData == null)
            {
                ReadySaveData();
            }

            if (_fromSave)
            {
                //_colorManager.ChangeColor(_saveData.Body.Vector4ToColor());
                //_colorManager.ChangeColor(_saveData.SecondaryBody.Vector4ToColor(), ColorTargetMode.Secondary);
                if (_savedData.PoleState != null)
                {
                    _pole1Trans.localPosition = new Vector3(_pole1Trans.localPosition.x, _savedData.PoleState.X, _pole1Trans.localPosition.z);
                    _pole2Trans.localPosition = new Vector3(_pole2Trans.localPosition.x, _savedData.PoleState.Y, _pole1Trans.localPosition.z);
                    _pole3Trans.localPosition = new Vector3(_pole3Trans.localPosition.x, _savedData.PoleState.Z, _pole1Trans.localPosition.z);
                }

                _motor.SpeedByPass(_savedData.Speed);
                _fromSave = false;
            }
        }

        public override void TryMoveToPosition()
        {
            ReadySaveData();
            transform.position = _savedData.Position.ToVector3();
            transform.rotation = _savedData.Rotation.Vec4ToQuaternion();
        }


        private void Update()
        {
            if (_allowedToExtend)
            {
                float progress = this.GetProgress();
                if (!_allowedToExtend || Mathf.Approximately( progress, 1)) return;
                
                this.SetPosition(_pole1Trans, progress, _pole1AnimationCurve, _pole1Max);
                this.SetPosition(_pole2Trans, progress, _pole2AnimationCurve, _pole2Max);
                this.SetPosition(_pole3Trans, progress, _pole3AnimationCurve, _pole3Max);
            }
        }

        private float GetProgress()
        {
            if (this._timeStartGrowth == -1f)
            {
                this.SetProgress(0f);
                return 0f;
            }
            return Mathf.Clamp((float)(DayNightCycle.main.timePassed - (double)this._timeStartGrowth) / 30, 0f, this.maxProgress);
        }

        private void SetProgress(float progress)
        {
            progress = Mathf.Clamp(progress, 0f, this.maxProgress);
            this._timeStartGrowth = DayNightCycle.main.timePassedAsFloat - 30 * progress;
        }

        private void SetPosition(Transform tr, float progress,AnimationCurve curve,float max)
        {
            float y = Mathf.Clamp(curve.Evaluate(progress), 0, max);
            tr.localPosition = new Vector3(tr.localPosition.x, y, tr.localPosition.z);
        }

        public override void PoleState(bool isExtended)
        {
            if (isExtended)
            {
                if (_timeStartGrowth <= 0)
                {
                    _timeStartGrowth = DayNightCycle.main.timePassed;
                }

                _motor.StartMotor();
                _allowedToExtend = true;
            }
            else
            {
                _motor.StopMotor();
                _allowedToExtend = false;
            }
        }

        private void ReadySaveData()
        {
            try
            {
                QuickLogger.Debug("In OnProtoDeserialize");
                var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
                var id = prefabIdentifier?.Id ?? string.Empty;
                _savedData = Mod.GetWindSurferSaveData(id);
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
            }
        }

        public override void Initialize()
        {

            if (_motor == null)
            {

                _pole1Trans = GameObjectHelpers.FindGameObject(gameObject, "Pole1").transform;
                _pole2Trans = GameObjectHelpers.FindGameObject(gameObject, "Pole2").transform;
                _pole3Trans = GameObjectHelpers.FindGameObject(gameObject, "Pole3").transform;

                _motor = _pole1Trans.gameObject.EnsureComponent<MotorHandler>();
                _motor.Initialize(200);
                _motor.StopMotor();
            }

            if (PowerController == null)
            {
                PowerController = gameObject.EnsureComponent<WindSurferPowerController>();
                PowerController.Initialize(this);
            }

            CreateLadders();

            IsInitialized = true;
        }
        
        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.WindSurferFriendlyName}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.WindSurferFriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = String.Empty;
            return false;
        }

        public override void OnConstructedChanged(bool constructed)
        {

        }

        public override void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized) return;

            if (_savedData == null)
            {
                _savedData = new WindSurferDataEntry();
            }

            _savedData.Id = GetPrefabID();

            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            //_savedData.Body = _colorManager.SaveTemplate();
            _savedData.BaseId = BaseId;
            _savedData.Position = transform.position.ToVec3();
            _savedData.Rotation = transform.rotation.QuaternionToVec4();
            _savedData.StoredPower = PowerController.GetStoredPower();
            _savedData.PoleState = new Vec3(_pole1Trans.localPosition.y, _pole2Trans.localPosition.y,_pole3Trans.localPosition.y);
            _savedData.Speed = _motor.GetRPM();
            newSaveData.WindSurferEntries.Add(_savedData);
        }

        private void CreateLadders()
        {
            var t01 = GameObjectHelpers.FindGameObject(gameObject, "T01").EnsureComponent<LadderController>();
            t01.Set(GameObjectHelpers.FindGameObject(gameObject, "T01_Top"));

            var t02 = GameObjectHelpers.FindGameObject(gameObject, "T02").EnsureComponent<LadderController>();
            t02.Set(GameObjectHelpers.FindGameObject(gameObject, "T02_Top"));

            var t03 = GameObjectHelpers.FindGameObject(gameObject, "T03").EnsureComponent<LadderController>();
            t03.Set(GameObjectHelpers.FindGameObject(gameObject, "T03_Top"));

            var t04 = GameObjectHelpers.FindGameObject(gameObject, "T04").EnsureComponent<LadderController>();
            t04.Set(GameObjectHelpers.FindGameObject(gameObject, "T04_Top"));
        }
    }
}
