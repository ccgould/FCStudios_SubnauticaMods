using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.JukeBox.Buildable;
using FCS_HomeSolutions.Mods.JukeBox.Spectrum;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.JukeBox.Mono
{
    internal  class JukeBoxController : FcsDevice, IFCSSave<SaveData>, IJukeBoxDevice
    {
        private JukeBoxDataEntry _savedData;
        private bool _isFromSave;
        private bool _runStartUpOnEnable;
        private AudioSource _audio;
        private Slider _trackSlider;
        private Text _trackName;
        private AudioLowPassFilter _lowPassFilter;
        private BaseJukeBox _baseJukeBox;
        private Slider _volumeSlider;
        private Toggle _repeatBTN;
        private float _labelScrollStart;
        private Text _volumePercentage;
        private Canvas _canvas;

        public override bool IsOperational => IsConstructed && IsInitialized;
        public bool IsJukeBox => true;
        
        public AudioSource GetAudioSource()
        {
            return _audio;
        }

        private void OnVolumeChanged(float value)
        {
            _audio.volume = value;
            _volumeSlider.SetValueWithoutNotify(value);
        }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, JukeBoxBuildable.JukeBoxTabID, Mod.ModPackID);
            _baseJukeBox = Manager.Habitat.GetComponent<BaseJukeBox>();
            RegisterDevice();
        }

        private void RegisterDevice()
        {
            _baseJukeBox.RegisterDevice(this);
            _baseJukeBox.OnLoopChanged += OnLoopChanged;
            _baseJukeBox.OnPause += Pause;
            _baseJukeBox.OnStop += Stop;
            _baseJukeBox.OnPlay += Play;
            _baseJukeBox.OnResume += Resume;
            _baseJukeBox.OnVolumeChanged += OnVolumeChanged;
            _baseJukeBox.OnTrackChanged += OnTrackChanged;
            _baseJukeBox.OnRefresh += OnRefresh;
        }

        private void Resume()
        {
            _audio.UnPause();
        }

        public void OnTrackChanged(TrackData trackData, bool isPlaying)
        {
            QuickLogger.Debug($"OnTrackChanged {trackData.AudioClip.name} : {trackData.AudioClip.length}", true);
            
            RefreshUI(trackData);

            if (isPlaying)
            {
                Play(trackData);
            }
        }

        public BaseManager GetBaseManager()
        {
            return Manager;
        }

        private void RefreshUI(TrackData trackData)
        {
            _audio.clip = trackData.AudioClip;
            _trackSlider.value = 0;
            _trackSlider.maxValue = trackData.AudioClip?.length ?? 0f;
            _trackName.text = trackData.Path ?? string.Empty;
            UpdatePositionLabel();
        }

        private void OnLoopChanged(bool value)
        {
            _repeatBTN.isOn = value;
        }

        private void Update()
        {
            if (_trackSlider == null || Manager == null || _baseJukeBox == null || _baseJukeBox.State == JukeBoxState.Pause || _canvas == null) return;
            
            LowPassFilterCheck();
            
            PlayUpdate();

            UpdatePositionLabel();

            if (_baseJukeBox.IsPowered && !_canvas.gameObject.activeSelf)
            {
                _canvas.gameObject.SetActive(true);
            }
            else if(!_baseJukeBox.IsPowered && _canvas.gameObject.activeSelf)
            {
                _canvas.gameObject.SetActive(false);
            }
        }

        private void PlayUpdate()
        {
            if (_baseJukeBox?.GetState() != JukeBoxState.Play) return;
            
            if (_baseJukeBox.HasTrackedJukeBox(this))
            {
                if (!_audio.isPlaying)
                {
                    Play(_baseJukeBox.CurrentTrack);
                }
                _audio.timeSamples = _baseJukeBox.TimeSamples;
            }
            _trackSlider.SetValueWithoutNotify(_baseJukeBox.TrackSliderValue);
        }

        private void LowPassFilterCheck()
        {
            if (_lowPassFilter != null)
            {
                _lowPassFilter.cutoffFrequency = _baseJukeBox.GetCutOffFrequency(GetConstructable());
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (_baseJukeBox == null) return;
            _baseJukeBox.UnRegisterDevice(this);
            _baseJukeBox.OnPause -= Pause;
            _baseJukeBox.OnStop -= Stop;
            _baseJukeBox.OnLoopChanged -= OnLoopChanged;
            _baseJukeBox.OnVolumeChanged -= OnVolumeChanged;
            _baseJukeBox.OnPlay -= Play;
            _baseJukeBox.OnResume -= Resume;
            _baseJukeBox.OnTrackChanged -= OnTrackChanged;
            _baseJukeBox.OnRefresh -= OnRefresh;
        }

        private void OnRefresh(TrackData obj)
        {
            RefreshUI(new TrackData());
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
                    _volumeSlider.value = _savedData.Volume;
                    _audio.volume = _savedData.Volume;
                    _volumePercentage.text = CalculateVolumePercentage();
                }

                _runStartUpOnEnable = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.JukeBoxEntrySaveData(GetPrefabID());
        }

        public override void Initialize()
        {
            if (IsInitialized) return;
            
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol,AlterraHub.BaseLightsEmissiveController);
            }

            if (_audio == null)
            {
                _audio = gameObject.GetComponentInChildren<AudioSource>();
            }

            _lowPassFilter = gameObject.GetComponentInChildren<AudioLowPassFilter>();

            var playBTN = GameObjectHelpers.FindGameObject(gameObject, "PlayBTN").GetComponent<Button>();
            playBTN.onClick.AddListener((() => _baseJukeBox.Play(this)));
            
            var stopBTN = GameObjectHelpers.FindGameObject(gameObject, "StopBTN").GetComponent<Button>();
            stopBTN.onClick.AddListener((() => _baseJukeBox.Stop(this)));
            
            var previousBTN = GameObjectHelpers.FindGameObject(gameObject, "PreviousBTN").GetComponent<Button>();
            previousBTN.onClick.AddListener(() => _baseJukeBox.PreviousTrack(this));
            
            var nextBTN = GameObjectHelpers.FindGameObject(gameObject, "NextBTN").GetComponent<Button>();
            nextBTN.onClick.AddListener(() => _baseJukeBox.NextTrack(this));

            _repeatBTN = GameObjectHelpers.FindGameObject(gameObject, "Repeat").GetComponent<Toggle>();
            _repeatBTN.onValueChanged.AddListener((value =>
            {
                _baseJukeBox.SetLoop(this, value);
                _audio.loop = value;
            }));

            var audioSpectum = gameObject.AddComponent<AudioSpectrum>();
            audioSpectum.Initialize(_audio);

            var audioSync = gameObject.AddComponent<AudioSyncColor>();
            audioSync.bias = 6;
            audioSync.timeStep = 0.15f;
            audioSync.timeToBeat = 0.05f;
            audioSync.restColor = Color.black;

            var refreshBTN = GameObjectHelpers.FindGameObject(gameObject, "RefreshBTN").GetComponent<Button>();
            refreshBTN.onClick.AddListener((() =>
            {
                _baseJukeBox.Refresh();
            }));

            _trackSlider = GameObjectHelpers.FindGameObject(gameObject, "Timeline").GetComponent<Slider>();
            _trackSlider.onValueChanged.AddListener((amount =>
            {
                if (_audio.clip == null)
                {
                    _trackSlider.SetValueWithoutNotify(0);
                    return;
                }
                _audio.time = amount;
                _baseJukeBox.SetTrackSlider(this, amount);
            }));

            _trackName = GameObjectHelpers.FindGameObject(gameObject, "TrackName").GetComponent<Text>();
            _trackName.text = string.Empty;

            _volumeSlider = GameObjectHelpers.FindGameObject(gameObject, "Volume").GetComponent<Slider>();
            _volumeSlider.SetValueWithoutNotify(_audio.volume);

            _volumePercentage = GameObjectHelpers.FindGameObject(gameObject, "VolumePercentage").GetComponent<Text>();
            _volumePercentage.text = CalculateVolumePercentage();

            _volumeSlider.onValueChanged.AddListener((value =>
            {
                if (_audio == null)
                {
                    QuickLogger.DebugError("Audio Controller is null",true);
                    return;
                }
                _baseJukeBox?.SetVolume(this,value);
                _audio.volume = value;
                _volumePercentage.text = CalculateVolumePercentage();
            }));

            _canvas = gameObject.GetComponentInChildren<Canvas>();

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseLightsEmissiveController, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseBeaconLightEmissiveController, gameObject, Color.red);
            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseBeaconLightEmissiveController, gameObject, 3f);

            IsInitialized = true;
        }
        
        private string CalculateVolumePercentage()
        {
            return $"{Mathf.FloorToInt(_audio.volume / 1 * 100)}%";
        }

        public void Play(TrackData trackData)
        {
            _audio.clip = trackData.AudioClip;
            _audio.Play();
            RefreshUI(trackData);
        }

        public void Stop()
        {
            _audio.Stop();
        }

        public void Pause()
        {
            _audio.Pause();
        }

        public void SetVolume(float value)
        {
            _volumeSlider.value = value;
        }

        public float GetAudioTime()
        {
            return _audio.time;
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
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new JukeBoxDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.Volume = _audio.volume;
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.JukeBoxDataEntries.Add(_savedData);
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public override float GetPowerUsage()
        {
            if (!GameModeUtils.RequiresPower())
            {
                return 0f;
            }

            return Time.deltaTime * 0.1f * _audio?.volume ?? 0f;
        }
        
        private void UpdatePositionLabel()
        {
            RectTransform rectTransform = _trackName.rectTransform;
            RectTransform rectTransform2 = rectTransform.parent as RectTransform;
            float preferredWidth = _trackName.preferredWidth;
            float width = rectTransform2.rect.width;
            float num = preferredWidth - width;
            if (num > 0f)
            {
                float num2 = num / 100f;
                float num3 = Trapezoid(1f, num2, 1f, num2, Time.time - _labelScrollStart);
                Vector2 anchoredPosition = rectTransform.anchoredPosition;
                anchoredPosition.x = -num * num3;
                rectTransform.anchoredPosition = anchoredPosition;
            }
        }

        public static float Trapezoid(float a, float b, float c, float d, float t, bool wrap = true)
        {
            float num = a + b + c + d;
            if (wrap)
            {
                t %= num;
            }
            return Mathf.Clamp01(Mathf.Min((b > 0f) ? ((t - a) / b) : 0f, (d > 0f) ? ((num - t) / d) : 0f));
        }
    }
}
