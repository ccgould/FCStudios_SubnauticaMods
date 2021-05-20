using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono.FCSPDA.Mono;
using FCS_AlterraHub.Registration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_AlterraHub.Mono.AlterraHubDepot.Mono
{
    internal class AlterraHubDepotController: FcsDevice, IFCSSave<SaveData>
    {
        private bool _isFromSave;
        private bool _runStartUpOnEnable;
        private AlterraHubDepotEntry _savedData;
        private FCSStorage _storage;
        private GameObject _door;
        private bool _isOpen;

        private float ClosePos { get; } = 0.2897835f;
        private float OpenPos { get; } = -0.193f;
        private const float Speed = .5f;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.AlterraHubDepotTabID, Mod.ModName);
        }

        private void Update()
        {
            //TODO is container is empty turn off storage
            //_storage.enabled = !_storage.container.IsEmpty();

            if (_door == null) return;

            if (_isOpen)
            {
                if (_door.transform.localPosition.y < OpenPos)
                {
                    _door.transform.Translate(Vector3.up * Speed * DayNightCycle.main.deltaTime);
                }

                if (_door.transform.localPosition.y > OpenPos)
                {
                    _door.transform.localPosition = new Vector3(_door.transform.localPosition.x, OpenPos, _door.transform.localPosition.z);
                }
            }
            else
            {
                if (_door.transform.localPosition.y > ClosePos)
                {
                    _door.transform.Translate(-Vector3.up * Speed * DayNightCycle.main.deltaTime);
                }

                if (_door.transform.localPosition.y < ClosePos)
                {
                    _door.transform.localPosition = new Vector3(_door.transform.localPosition.x, ClosePos, _door.transform.localPosition.z);
                }
            }
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

                    _colorManager.ChangeColor(_savedData.BodyColor.Vector4ToColor(), ColorTargetMode.Both);
                }

                _runStartUpOnEnable = false;
            }
        }

        public override void Initialize()
        {
            if (IsInitialized) return;

            if (_door == null)
            {
                _door = GameObjectHelpers.FindGameObject(gameObject, "door_controller");
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, Buildables.AlterraHub.BodyMaterial);
            }

            if (_storage == null)
            {
                _storage = gameObject.GetComponent<FCSStorage>();
                _storage.SlotsAssigned = 48;
                _storage.OnContainerClosed += OnContainerClosed;
                _storage.OnContainerOpened += OnContainerOpened;
            }

            IsInitialized = true;
        }

        private void OnContainerOpened()
        {
            _isOpen = true;
        }

        private void OnContainerClosed()
        {
            _isOpen = false;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save();
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

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new AlterraHubDepotEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.BodyColor = _colorManager.GetColor().ColorToVector4();
            _savedData.BaseId = BaseId;
            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            newSaveData.AlterraHubDepotEntries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetAlterraHubDepotEntrySaveData(GetPrefabID());
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        //public void OnHandHover(GUIHand hand)
        //{
        //    var main = HandReticle.main;
        //    main.SetIcon(HandReticle.IconType.Info);
        //    main.SetInteractTextRaw("","");
            
        //    if (Input.GetKeyDown(QPatch.Configuration.PDAInfoKeyCode))
        //    {
        //        FCSPDAController.Instance.OpenEncyclopedia(TechType.Copper);
        //    }
        //}

        //public void OnHandClick(GUIHand hand)
        //{

        //}
    }
}
