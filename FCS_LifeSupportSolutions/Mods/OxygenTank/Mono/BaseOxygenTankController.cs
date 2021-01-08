using System;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.OreConsumer;
using FCS_AlterraHub.Registration;
using FCS_LifeSupportSolutions.Buildable;
using FCS_LifeSupportSolutions.Configuration;
using FCSCommon.Controllers;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Mods.OxygenTank.Mono
{
    internal class BaseOxygenTankController : FcsDevice,IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private BaseOxygenTankEntry _savedData;
        private ParticleSystem[] _bubbles;
        private int _isRunningHash;
        private MotorHandler _fanMotor;
        private AudioManager _audioManager;
        private bool _prevPowerState;
        private OxygenTankAttachPoint _oxygenAttachPoint;
        public OxygenManager OxygenManager { get; set; }
        public override bool IsOperational => IsConstructed && Manager != null && IsInitialized;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.BaseOxygenTankTabID, Mod.ModName);
            //Manager.OnPowerStateChanged += state =>
            //{
            //    if (state != PowerSystem.Status.Normal || !Manager.HasEnoughPower(GetPowerUsage()))
            //    {
            //        UpdateAnimations(false);
            //    }
            //    else
            //    {
            //        UpdateAnimations(true);
            //    }
            //};
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
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

                    //OxygenManager.SetO2Level(_savedData.O2Level);
                    _colorManager.ChangeColor(_savedData.Body.Vector4ToColor());
                    _colorManager.ChangeColor(_savedData.SecondaryBody.Vector4ToColor(), ColorTargetMode.Secondary);
                }

                _runStartUpOnEnable = false;
            }
        }

        public override void Initialize()
        {
            //_bubbles = gameObject.GetComponentsInChildren<ParticleSystem>();

            _oxygenAttachPoint = gameObject.EnsureComponent<OxygenTankAttachPoint>();

            if (_audioManager == null)
            {
                _audioManager = new AudioManager(gameObject.EnsureComponent<FMOD_CustomLoopingEmitter>());
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial);
            }

            //if (OxygenManager == null)
            //{
            //    OxygenManager = gameObject.AddComponent<OxygenManager>();
            //    OxygenManager.Initialize(this);
            //    OxygenManager.OnOxygenUpdated += (amount, percentage) =>
            //    {
            //        _percent.text = $"{percentage:P0}";
            //        _percentBar.fillAmount = percentage;
            //    };
            //}

            //InvokeRepeating(nameof(UpdateAnimation), 1f, 1f);

            //var takeOxygenBtnObj = InterfaceHelpers.FindGameObject(gameObject, "Button");
            //InterfaceHelpers.CreateButton(takeOxygenBtnObj, "TakeBTN", InterfaceButtonMode.Background,
            //    GivePlayerOxygen, new Color(0.2784314f, 0.2784314f, 0.2784314f), new Color(0, 1, 1, 1), 5,
            //    AuxPatchers.TakeOxygen(), AuxPatchers.TakeOxygenDesc());

            //_percent = InterfaceHelpers.FindGameObject(gameObject, "percentage").GetComponent<Text>();
            //_percentBar = InterfaceHelpers.FindGameObject(gameObject, "PreLoader_Bar_Front").GetComponent<Image>();

            //MaterialHelpers.ChangeEmissionStrength(ModelPrefab.EmissiveControllerMaterial, gameObject, 5f);

            IsInitialized = true;
        }

        private void GivePlayerOxygen(string arg1, object arg2)
        {
           // OxygenManager.GivePlayerO2();
        }

        public override float GetPowerUsage()
        {
            return 0f;
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

        public override bool CanDeconstruct(out string reason)
        {
            if (_oxygenAttachPoint.HasAttachment())
            {
                reason = AuxPatchers.OxygenTankHasAttachment();
                return false;
            }
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
            else
            {
                if (_oxygenAttachPoint != null)
                    _oxygenAttachPoint.SetParent(null);
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new BaseOxygenTankEntry();
            }

            _savedData.Id = GetPrefabID();
            //_savedData.O2Level = OxygenManager.GetO2Level();
            _savedData.Body = _colorManager.GetColor().ColorToVector4();
            _savedData.SecondaryBody = _colorManager.GetSecondaryColor().ColorToVector4();
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.BaseOxygenTankEntries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetOxygenTankSaveData(GetPrefabID());
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }
    }

    internal class OxygenTankAttachPoint : MonoBehaviour, IPipeConnection
    {
        private readonly List<string>_children = new List<string>();
        private GameObject _attachPoint;
        public string parentPipeUID;
        public string rootPipeUID;
        public Vector3 parentPosition;

        private void Start()
        {
            _attachPoint = gameObject.FindChild("Cube");
        }

        public void SetParent(IPipeConnection parent)
        {
            IPipeConnection parent2 = this.GetParent();
            this.parentPipeUID = null;
            this.rootPipeUID = null;
            if (parent2 != null && parent != parent2)
            {
                parent2.RemoveChild(this);
            }

            GameObject gameObject = parent?.GetGameObject();
            if (gameObject != null && gameObject.TryGetComponent(out OxygenPipe oxygenPipe))
            {
                Vector3 attachpoint = this.GetAttachPoint();
                Vector3 vector = Vector3.Normalize(oxygenPipe.parentPosition - attachpoint);
                float magnitude = (oxygenPipe.parentPosition - attachpoint).magnitude;
                oxygenPipe.transform.position = attachpoint;
                oxygenPipe.topSection.rotation = Quaternion.LookRotation(vector, Vector3.up);
                oxygenPipe.endCap.rotation = oxygenPipe.topSection.rotation;
                oxygenPipe.bottomSection.rotation = Quaternion.LookRotation(vector, Vector3.up);
                oxygenPipe.bottomSection.position = oxygenPipe.parentPosition;
                oxygenPipe.stretchedPart.position = oxygenPipe.topSection.position + vector;
                Vector3 localScale = oxygenPipe.stretchedPart.localScale;
                localScale.z = magnitude - 2f;
                oxygenPipe.stretchedPart.localScale = localScale;
                oxygenPipe.stretchedPart.rotation = oxygenPipe.topSection.rotation;


                this.parentPipeUID = gameObject.GetComponent<UniqueIdentifier>().Id;
                this.rootPipeUID = ((oxygenPipe.GetRoot() != null) ? oxygenPipe.GetRoot().GetGameObject().GetComponent<UniqueIdentifier>().Id : null);
                this.parentPosition = oxygenPipe.GetAttachPoint();
                oxygenPipe.AddChild(this);

            }
        }

        public IPipeConnection GetParent()
        {
            UniqueIdentifier uniqueIdentifier;
            if (!string.IsNullOrEmpty(this.parentPipeUID) && UniqueIdentifier.TryGetIdentifier(this.parentPipeUID, out uniqueIdentifier))
            {
                return uniqueIdentifier.GetComponent<IPipeConnection>();
            }
            return null;
        }

        public void SetRoot(IPipeConnection root)
        {
            if (root == null)
            {
                this.rootPipeUID = null;
            }
            else
            {
                this.rootPipeUID = root.GetGameObject().GetComponent<UniqueIdentifier>().Id;
            }
        }

        public IPipeConnection GetRoot()
        {
            UniqueIdentifier uniqueIdentifier;
            if (!string.IsNullOrEmpty(this.rootPipeUID) && UniqueIdentifier.TryGetIdentifier(this.rootPipeUID, out uniqueIdentifier))
            {
                return uniqueIdentifier.GetComponent<IPipeConnection>();
            }
            return null;
        }

        public void AddChild(IPipeConnection child)
        {
            _children.Add(child.GetGameObject().GetComponent<UniqueIdentifier>().Id);
        }

        public void RemoveChild(IPipeConnection child)
        { 
            _children.Add(child.GetGameObject().GetComponent<UniqueIdentifier>().Id);
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public bool GetProvidesOxygen()
        {
            IPipeConnection parent = this.GetParent();
            if(parent != null && parent.GetProvidesOxygen())
            {
                return true;
            }
            return false;
        }

        public void Update()
        {
            if(this.parentPipeUID is null)
            {
                IPipeConnection parent = null;
                float num = 1000f;
                int num2 = UWE.Utils.OverlapSphereIntoSharedBuffer(base.transform.position, 1f, -1, QueryTriggerInteraction.UseGlobal);
                for (int i = 0; i < num2; i++)
                {
                    GameObject entityRoot = UWE.Utils.GetEntityRoot(UWE.Utils.sharedColliderBuffer[i].gameObject);
                    if (!(entityRoot == null))
                    {
                        IPipeConnection component = entityRoot.GetComponent<IPipeConnection>();
                        if (component != null && component.GetRoot() != null && this.IsInSight(component))
                        {
                            float magnitude = (entityRoot.transform.position - base.transform.position).magnitude;
                            if (magnitude < num)
                            {
                                parent = component;
                                num = magnitude;
                            }
                        }
                    }
                }
                this.SetParent(parent);
            }
        }

        private bool IsInSight(IPipeConnection parent)
        {
            bool result = true;
            Vector3 value = parent.GetAttachPoint() - this.GetAttachPoint();
            int num = UWE.Utils.RaycastIntoSharedBuffer(new Ray(this.GetAttachPoint(), Vector3.Normalize(value)), value.magnitude, -5, QueryTriggerInteraction.UseGlobal);
            for (int i = 0; i < num; i++)
            {
                if (UWE.Utils.GetEntityRoot(UWE.Utils.sharedHitBuffer[i].collider.gameObject) != parent.GetGameObject())
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        public Vector3 GetAttachPoint()
        {
            return _attachPoint.transform.position;
        }

        public void UpdateOxygen()
        {
        }

        public bool HasAttachment()
        {
            return _children.Count > 0;
        }
    }
}
