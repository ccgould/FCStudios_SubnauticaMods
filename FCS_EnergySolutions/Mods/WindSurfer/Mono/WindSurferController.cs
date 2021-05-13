using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.OreConsumer;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UWE;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    internal class WindSurferController : FcsDevice, IFCSSave<SaveData>, IPlatform
    {
        private MotorHandler _motor;
        private bool _fromSave;
        private WindSurferDataEntry _savedData;
        private double _timeStartGrowth;
        private bool _allowedToExtend;
        private GameObject _pole3;
        private GameObject _pole2;
        private GameObject _pole1;
        private float maxProgress = 1f;
        private float _pole1Max = 3.71f;
        private float _pole2Max = 4.7f;
        private float _pole3Max = 5.2f;
        private AnimationCurve _pole1AnimationCurve;
        private AnimationCurve _pole2AnimationCurve;
        private AnimationCurve _pole3AnimationCurve;
        public PlatformController PlatformController => _platformController ?? (_platformController = GetComponent<PlatformController>());

        private Light[] _lights;
        private PlatformController _platformController;
        public override bool BypassRegisterCheck { get; } = true;

        private void Start()
        {
            _pole1AnimationCurve = new AnimationCurve(new Keyframe(0, 1.149f), new Keyframe(1, _pole1Max));
            _pole2AnimationCurve = new AnimationCurve(new Keyframe(0, 1.841746f), new Keyframe(1, _pole2Max));
            _pole3AnimationCurve = new AnimationCurve(new Keyframe(0, 1.397793f), new Keyframe(1, _pole3Max));
            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseEmissiveDecals, gameObject, 4f);
            GetUnitID();
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
                TryMoveToPosition();
                _fromSave = false;
            }
        }

        internal void TryMoveToPosition()
        {
            ReadySaveData();
            transform.position = _savedData.Position.ToVector3();
            transform.rotation = _savedData.Rotation.Vec4ToQuaternion();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Mod.OnLightsEnabledToggle -= OnLightsEnabledToggle;
        }

        private void Update()
        {
            if (_allowedToExtend)
            {
                if (!_allowedToExtend) return;
                float progress = this.GetProgress();
                this.SetPosition(_pole1.transform, progress, _pole1AnimationCurve, _pole1Max);
                this.SetPosition(_pole2.transform, progress, _pole2AnimationCurve, _pole2Max);
                this.SetPosition(_pole3.transform, progress, _pole3AnimationCurve, _pole3Max);
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

        internal void PoleState(bool isExtended)
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
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _savedData = Mod.GetWindSurferSaveData(id);
        }

        public override void Initialize()
        {

            _lights = gameObject.GetComponentsInChildren<Light>();

            Mod.OnLightsEnabledToggle += OnLightsEnabledToggle;


            if (_motor == null)
            {

                _pole1 = GameObjectHelpers.FindGameObject(gameObject, "Pole1");
                _pole2 = GameObjectHelpers.FindGameObject(gameObject, "Pole2");
                _pole3 = GameObjectHelpers.FindGameObject(gameObject, "Pole3");
                _motor = _pole1.EnsureComponent<MotorHandler>();
                _motor.Initialize(200);
                _motor.StopMotor();
            }

            CreateLadders();

            IsInitialized = true;
        }

        private void OnLightsEnabledToggle(bool value)
        {
            if (_lights != null)
            {
                foreach (Light light in _lights)
                {
                    light.gameObject.SetActive(value);
                }
            }
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

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized) return;

            if (_savedData == null)
            {
                _savedData = new WindSurferDataEntry();
            }

            _savedData.Id = GetPrefabID();

            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            //_savedData.Body = _colorManager.GetColor().ColorToVector4();
            _savedData.BaseId = BaseId;
            _savedData.Position = transform.position.ToVec3();
            _savedData.Rotation = transform.rotation.QuaternionToVec4();
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

        public string GetUnitID()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.WindSurferTabID, Mod.ModName);
            return UnitID;
        }
    }
}
