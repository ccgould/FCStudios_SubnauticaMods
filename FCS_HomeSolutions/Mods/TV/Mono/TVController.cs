using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.TV.Mono
{
    internal class TVController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private DecorationDataEntry _savedData;
        private bool _isOn = true;
        private Canvas _canvas;
        private ToggleGroup _toggleGroup;
        private VideoSectionController _videoSection;
        private HomeSectionController _homeSection;
        private Toggle _videoToggle;
        private Toggle _homeToggle;
        private Text _clock;
        private Text _volumeText;
        private float _timer;
        private bool _forceOn;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DecorationItemTabId, Mod.ModPackID);
            Manager.OnPowerStateChanged += OnPowerStateChanged;
        }

        private void Update()
        {
            if (_clock != null)
                _clock.text = WorldHelpers.GetGameTimeFormat();

            if (GetIsOn())
            {
                if (_volumeText != null && _volumeText.gameObject.activeSelf)
                {
                    _timer += DayNightCycle.main.deltaTime;

                    if (_timer >= 3f)
                    {
                        if (!_videoSection.IsMuted())
                        {
                            _volumeText.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        private void OnPowerStateChanged(PowerSystem.Status obj)
        {
            if(_forceOn) return;
            if (obj == PowerSystem.Status.Offline)
            {
                TurnOff();
            }
            else
            {
                if (_isOn)
                {
                    TurnOn();
                }

            }
        }

        public override void OnDestroy()
        {
            if (Manager != null)
            {
                Manager.OnPowerStateChanged -= OnPowerStateChanged;
            }
            base.OnDestroy();
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

                    _colorManager.LoadTemplate(_savedData.ColorTemplate);
                    
                    _videoSection.ChangeVolume(_savedData.Volume, _savedData.Volume > 0f);
                    
                    QuickLogger.Debug($"Is On from save: {_savedData.IsOn}",true);
                    if (!_savedData.IsOn)
                    {
                        TurnOff();
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(_savedData.Video))
                        {
                            ChangePage("VideoBTN");
                           _videoSection.LoadChannel(_savedData.Video);
                           _forceOn = true;
                            TurnOn();
                        }
                    }
                    
                }
                _runStartUpOnEnable = false;
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new DecorationDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.Volume = _videoSection.GetVolume();
            _savedData.Video = _videoSection.GetCurrentChannel();
            _savedData.IsOn = _isOn;
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.DecorationEntries.Add(_savedData);
        }

        public override void Initialize()
        {
            _toggleGroup = gameObject.GetComponentInChildren<ToggleGroup>(true);

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol);
            }
            
            if (_canvas == null)
            {
                _canvas = gameObject.GetComponentInChildren<Canvas>(true);

                _videoSection = GameObjectHelpers.FindGameObject(gameObject, "VideoSelection").AddComponent<VideoSectionController>();
                _videoSection.Initialize(this);
                _homeSection = GameObjectHelpers.FindGameObject(gameObject, "Home").AddComponent<HomeSectionController>();
                
                _homeToggle = GameObjectHelpers.FindGameObject(gameObject, "HomeBTN").GetComponent<Toggle>();
                _homeToggle.onValueChanged.AddListener((value =>
                {
                    ChangePage("HomeBTN");
                }));

                _videoToggle = GameObjectHelpers.FindGameObject(gameObject, "VideoBTN").GetComponent<Toggle>();
                
                _videoToggle.onValueChanged.AddListener((value =>
                {
                    ChangePage("VideoBTN");
                }));

                if (_clock == null)
                {
                    _clock = GameObjectHelpers.FindGameObject(gameObject, "Clock").GetComponent<Text>();
                }

                _volumeText = GameObjectHelpers.FindGameObject(gameObject, "VolumeLBL").GetComponent<Text>();

                TurnOn();
            } 
            
            IsInitialized = true;
        }

        private void ChangePage(string name)
        {
            switch (name)
            {
                case "VideoBTN":
                    _videoSection.Show();
                    _homeSection.Hide();
                    break;
                case "HomeBTN":
                    _videoSection.Hide();
                    _homeSection.Show();
                    break;
            }
        }

        private void TurnOff()
        {
            QuickLogger.Debug("Turn Off TV", true);
            if (!_isOn && !_homeToggle.isOn && !_canvas.gameObject.activeSelf) return;
            _isOn = false;
            _homeToggle.isOn = true;
            _forceOn = false;
            _videoSection.Hide();
            _canvas.gameObject.SetActive(false);
        }

        private void TurnOn()
        {
            QuickLogger.Debug($"Turn On TV {_canvas.gameObject.activeSelf}", true);
            if (_isOn && _homeToggle.isOn && _canvas.gameObject.activeSelf) return;
            _isOn = true;
            _homeToggle.isOn = true;
            _homeSection.Show();
            _canvas.gameObject.SetActive(true);
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
        
        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetDecorationDataEntrySaveData(GetPrefabID());
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

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public override void OnHandHover(GUIHand hand)
        {
            OnHandHover(hand,"SmartTVs");

            var data = new string[]
            {
                AuxPatchers.TVFormat(QPatch.Configuration.SmartTelevisionsToggleTv.ToString()),
                AuxPatchers.TVFormatContinued(
                    QPatch.Configuration.SmartTelevisionsVolumeUp.ToString(),
                    QPatch.Configuration.SmartTelevisionsVolumeDown.ToString())
            };

            data.HandHoverPDAHelperEx("SmartTVs");

            if (Input.GetKeyDown(QPatch.Configuration.SmartTelevisionsToggleTv))
            {
                if (_isOn)
                {
                    TurnOff();
                }
                else
                {
                    TurnOn();
                }
            }

            if (Input.GetKeyDown(QPatch.Configuration.SmartTelevisionsVolumeUp))
            {
                _videoSection.VolumeUp();
            }

            if (Input.GetKeyDown(QPatch.Configuration.SmartTelevisionsVolumeDown))
            {
                _videoSection.VolumeDown();
            }
        }

        public void OnHandClick(GUIHand hand)
        {

        }

        internal bool GetIsOn()
        {
            return _isOn;
        }

        public void SetVolumeText(string value)
        {
            _volumeText.text = value;
            _volumeText.gameObject.SetActive(true);
        }
    }
}