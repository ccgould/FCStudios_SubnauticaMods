using System.Collections.Generic;
using System.IO;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace FCS_HomeSolutions.Mods.TV.Mono
{
    internal class TVController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private DecorationDataEntry _savedData;
        private AudioSource _audio;
        private VideoPlayer _video;
        private bool _isOn = true;
        private Canvas _canvas;
        private Text _volumeText;
        private float _timer;
        private List<string>_videoLocations;
        private int _currentChannel;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DecorationItemTabId, Mod.ModName);
            Manager.OnPowerStateChanged += OnPowerStateChanged;
        }

        private void OnPowerStateChanged(PowerSystem.Status obj)
        {
            if (obj != PowerSystem.Status.Normal)
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

                    _colorManager.ChangeColor(_savedData.Fcs.Vector4ToColor());
                    ChangeVolume(_savedData.Volume, _savedData.Volume > 0f);

                    if (_videoLocations.Count - 1 >= _savedData.Channel)
                    {
                        _currentChannel = _savedData.Channel;
                        ForceChannel();
                    }

                    if (!_savedData.IsOn)
                    {
                        TurnOff();
                        _isOn = false;
                    }
                    
                }

                _runStartUpOnEnable = false;
            }
        }

        private void ChangeVolume(float volume, bool isVolumeUp = true)
        {
            _audio.volume = volume;
            var amount = Mathf.RoundToInt(volume * 100).ToString();
            if (isVolumeUp)
            {
                _volumeText.text = amount;
            }
            else
            {
                _volumeText.text = volume <= 0f ? "MUTE" : amount;
            }
            _volumeText.gameObject.SetActive(true);
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
            _savedData.Fcs = _colorManager.GetColor().ColorToVector4();
            _savedData.Volume = _audio.volume;
            _savedData.Channel = _currentChannel;
            _savedData.IsOn = _isOn;
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.DecorationEntries.Add(_savedData);
        }

        private void ForceChannel()
        {
            if(_videoLocations.Count <= 0 && _videoLocations[_currentChannel] ==null) return;
            _video.url = _videoLocations[_currentChannel];
            _video.Play();
        }

        public override void Initialize()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
            }

            if (_audio == null)
            {
                _audio = gameObject.GetComponentInChildren<AudioSource>();
            }

            if (_video == null)
            {
                LoadVideoLocations();
                _canvas = gameObject.GetComponentInChildren<Canvas>();
                var rawImage = gameObject.GetComponentInChildren<RawImage>();
                _video = gameObject.GetComponentInChildren<VideoPlayer>();

                _video.url = _videoLocations[0];
                var renderTexture = new RenderTexture(1920, 1080, 24);
                renderTexture.Create();
                _video.targetTexture = renderTexture;
                rawImage.texture = renderTexture;
            } 
            
            _volumeText = gameObject.GetComponentInChildren<Text>(true);
            
            IsInitialized = true;
        }

        private void Update()
        {
            if(_video != null)
                _video.playbackSpeed = Mathf.Approximately(DayNightCycle.main.deltaTime,0f) ? 0 : 1;

            if (_isOn)
            {
                if (_volumeText != null && _volumeText.gameObject.activeSelf)
                {
                    _timer += DayNightCycle.main.deltaTime;
                    if (_timer >= 3f)
                    {
                        if (!Mathf.Approximately(_audio.volume, 0))
                        {
                            _volumeText.gameObject.SetActive(false);
                        }
                        
                    }
                }
            }

        }

        private void LoadVideoLocations()
        {
            if (_videoLocations == null)
            {
                _videoLocations = new List<string>();
            }

            var path = Path.Combine(Mod.GetAssetPath(), "Videos");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var videos = Directory.GetFiles(path, "*.mp4");

            foreach (string filePath in videos)
            {
                if (File.Exists(filePath) && !_videoLocations.Contains(filePath))
                {
                    _videoLocations.Add(filePath);
                }
            }

            QuickLogger.Debug($"Loaded: {_videoLocations.Count}.",true);
        }

        private void Play()
        {
            _video.Play();
        }

        private void Pause()
        {
            _video.Pause();
        }

        private void Stop()
        {
            _video.Stop();
        }

        private void VolumeUp()
        {
            if (!_isOn) return;
            _timer = 0f;
            ChangeVolume(Mathf.Clamp(_audio.volume + 0.1f, 0, 1f));
        }

        private void VolumeDown()
        {
            if(!_isOn) return;
            _timer = 0f;
            ChangeVolume(Mathf.Clamp(_audio.volume - 0.1f, 0, 1f),false);
        }

        private void TurnOff()
        {
            _isOn = false;
            _audio.mute = true;
            _canvas.gameObject.SetActive(false);
        }

        private void TurnOn()
        {
            _isOn = true;
            _audio.mute = false;
            _canvas.gameObject.SetActive(true);
        }

        private void ChannelUp()
        {
            _currentChannel++;

            if (_currentChannel >= _videoLocations.Count)
            {
                _currentChannel = _currentChannel % _videoLocations.Count;
            }

            ForceChannel();
        }

        private void ChannelDown()
        {
            _currentChannel--;

            if (_currentChannel == -1)
            {
                _currentChannel = _videoLocations.Count - 1;
            }

            ForceChannel();
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

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;

            main.SetUseTextRaw(AuxPatchers.TVFormat(QPatch.TelevisionConfiguration.ToggleTv.ToString()),
                AuxPatchers.TVFormatContinued(
                    QPatch.TelevisionConfiguration.VolumeUp.ToString(),
                    QPatch.TelevisionConfiguration.VolumeDown.ToString(),
                    QPatch.TelevisionConfiguration.ChannelUp.ToString(),
                    QPatch.TelevisionConfiguration.ChannelDown.ToString()));

            main.SetIcon(HandReticle.IconType.Default);

            if (Input.GetKeyDown(QPatch.TelevisionConfiguration.ToggleTv))
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

            if (Input.GetKeyDown(QPatch.TelevisionConfiguration.VolumeUp))
            {
                VolumeUp();
            }

            if (Input.GetKeyDown(QPatch.TelevisionConfiguration.VolumeDown))
            {
                VolumeDown();
            }

            if (Input.GetKeyDown(QPatch.TelevisionConfiguration.ChannelDown))
            {
                ChannelDown();
            }

            if (Input.GetKeyDown(QPatch.TelevisionConfiguration.ChannelUp))
            {
                ChannelUp();
            }
        }

        public void OnHandClick(GUIHand hand)
        {

        }
    }
}