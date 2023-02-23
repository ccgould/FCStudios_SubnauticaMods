using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.JukeBox.Buildable;
using FCS_HomeSolutions.Mods.JukeBox.Spectrum;
using FCSCommon.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static JukeboxInstance;

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
        private GameObject _pairingScreen;
        private GameObject _overlay;
        private GameObject _musicList;
        private GameObject _mainScreen;
        private GameObject _boot;
        private GameObject _musicListContent;
        private bool _isPairing;

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
            _volumePercentage.text = CalculateVolumePercentage();

        }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, JukeBoxBuildable.JukeBoxTabID, Mod.ModPackID);
            _baseJukeBox = Manager.Habitat.GetComponent<BaseJukeBox>();
            RegisterDevice();
            if (_isFromSave)
            {
                _baseJukeBox.Volume = _savedData.Volume;
            }
            else
            {
                _volumeSlider.value = _baseJukeBox.Volume;
                _volumePercentage.text = CalculateVolumePercentage();
            }
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
            if (_audio.clip == null) return;
            _audio.UnPause();
        }

        public void OnTrackChanged(TrackData trackData, bool isPlaying)
        {
            if (_isPairing) return;
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
            _trackSlider.SetValueWithoutNotify(0);
            _trackSlider.maxValue = trackData.AudioClip?.length ?? 0f;
            SetLabel(trackData.Path ?? string.Empty);
            UpdatePositionLabel();
        }

        private void SetLabel(string text)
        {
            _trackName.text = text;
            RectTransform rectTransform = _trackName.rectTransform;
            RectTransform rectTransform2 = rectTransform.parent as RectTransform;
            float preferredWidth = _trackName.preferredWidth;
            float width = rectTransform2.rect.width;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
            float num = preferredWidth - width;
            if (num > 0f)
            {
                this._labelScrollStart = Time.time;
                this.UpdatePositionLabel();
            }
            else
            {
                Vector2 anchoredPosition = rectTransform.anchoredPosition;
                anchoredPosition.x = 0.5f * -num;
                rectTransform.anchoredPosition = anchoredPosition;
            }
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

            _trackSlider.SetValueWithoutNotify(_audio.time);


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
            if (_baseJukeBox?.GetState() != JukeBoxState.Play || _isPairing) return;


            if (!_audio.isPlaying)
            {
                QuickLogger.Debug($"Time Samples: {_baseJukeBox.TimeSamples}", true);

                Play(_baseJukeBox.CurrentTrack);
                _audio.timeSamples = _baseJukeBox.TimeSamples;
            }
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
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol,string.Empty);
            }

            if (_audio == null)
            {
                _audio = gameObject.GetComponentInChildren<AudioSource>();
            }

            _lowPassFilter = gameObject.GetComponentInChildren<AudioLowPassFilter>();

            var playBTN = GameObjectHelpers.FindGameObject(gameObject, "PlayBTN").GetComponent<Button>();
            playBTN.onClick.AddListener((() => _baseJukeBox.Play()));
            
            var stopBTN = GameObjectHelpers.FindGameObject(gameObject, "StopBTN").GetComponent<Button>();
            stopBTN.onClick.AddListener((() => _baseJukeBox.Stop()));
            
            var previousBTN = GameObjectHelpers.FindGameObject(gameObject, "PreviousBTN").GetComponent<Button>();
            previousBTN.onClick.AddListener(() => _baseJukeBox.PreviousTrack());
            
            var nextBTN = GameObjectHelpers.FindGameObject(gameObject, "NextBTN").GetComponent<Button>();
            nextBTN.onClick.AddListener(() => _baseJukeBox.NextTrack());

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

            var playListBTN = GameObjectHelpers.FindGameObject(gameObject, "PlayListBTN").GetComponent<Button>();
            playListBTN.onClick.AddListener((() =>
            {
                GotoPage(JukeboxPages.MusicList);
            }));

            var backBTN = GameObjectHelpers.FindGameObject(gameObject, "BackBTN").GetComponent<Button>();
            backBTN.onClick.AddListener((() =>
            {
                GotoPage(JukeboxPages.Home);
            }));


            _trackSlider = GameObjectHelpers.FindGameObject(gameObject, "Timeline").GetComponent<Slider>();
            _trackSlider.onValueChanged.AddListener((amount =>
            {
                if (_audio.clip == null)
                {
                    _trackSlider.SetValueWithoutNotify(0);
                    return;
                }
                _baseJukeBox.ForceTimeChange(amount, _audio.clip);
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
                _baseJukeBox?.SetVolume(value);
                _audio.volume = value;
                _volumePercentage.text = CalculateVolumePercentage();
            }));

            _canvas = gameObject.GetComponentInChildren<Canvas>(true);

            _canvas = gameObject.GetComponentInChildren<Canvas>(true);

            _pairingScreen = GameObjectHelpers.FindGameObject(_canvas.gameObject, "PairingScreen");
            _overlay = GameObjectHelpers.FindGameObject(_canvas.gameObject, "Overlay");
            _musicList = GameObjectHelpers.FindGameObject(_canvas.gameObject, "MusicList");
            _mainScreen = GameObjectHelpers.FindGameObject(_canvas.gameObject, "MainScreen");
            _boot = GameObjectHelpers.FindGameObject(_canvas.gameObject, "Boot");
            _musicListContent = GameObjectHelpers.FindGameObject(_musicList, "Content");

            LoadMusicListScreen();


            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.red);
            MaterialHelpers.ChangeEmissionStrength(string.Empty, gameObject, 3f);

            StartCoroutine(PairJukeBox());


            IsInitialized = true;
        }

        private IEnumerator PairJukeBox()
        {
            _isPairing = true;
            Play(ModelPrefab.PoweringOnClip);
            GotoPage(JukeboxPages.PoweringOn);
            yield return new WaitForSeconds(5);
            Play(ModelPrefab.ReadyToPairClip);
            GotoPage(JukeboxPages.Pairing);
            yield return new WaitForSeconds(5);
            Play(ModelPrefab.PairedClip);
            yield return new WaitForSeconds(5);
            GotoPage(JukeboxPages.Home);
            _isPairing = false;
            yield break;

        }

        private void GotoPage(JukeboxPages page)
        {
            _mainScreen.SetActive(false);
            _overlay.SetActive(false);
            _pairingScreen.SetActive(false);
            _musicList.SetActive(false);


            switch (page)
            {
                case JukeboxPages.PoweringOn:
                    _canvas.gameObject.SetActive(true);
                    break;
                case JukeboxPages.Pairing:
                    _pairingScreen.SetActive(true);
                    _boot.SetActive(false);
                    break;
                case JukeboxPages.Home:
                    _mainScreen.SetActive(true);
                    _overlay.SetActive(true);
                    break;
                case JukeboxPages.MusicList:
                    _musicList.SetActive(true);
                    _overlay.SetActive(true);
                    LoadMusicListScreen();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(page), page, null);
            }
        }

        private void LoadMusicListScreen()
        {
            foreach (Transform childTransform in _musicListContent.transform)
            {
                Destroy(childTransform.gameObject);
            }

            foreach (TrackData data in JukeBox.Playlist)
            {
                var depotPrefab = GameObject.Instantiate(ModelPrefab.JukeboxMusicItemPrefab);
                var controller = depotPrefab.AddComponent<MusicListItemController>();
                //controller.Set(data, _baseJukeBox, this);
                controller.transform.SetParent(_musicListContent.transform, false);
            }
        }


        private string CalculateVolumePercentage()
        {
            return $"{Mathf.FloorToInt(_audio.volume / 1 * 100)}%";
        }

        public void Play(TrackData trackData)
        {
            if (!IsOperational || _isPairing) return;
            _audio.clip = trackData.AudioClip;
            _trackSlider.SetValueWithoutNotify(0);
            _audio.Play();
            RefreshUI(trackData);
        }

        public void Play(AudioClip clip)
        {
            if (_audio.isPlaying) return;
            _audio.clip = clip;
            _audio.Play();
        }

        public void Stop()
        {
            _audio.Stop();
            _audio.time = 0;
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
            if (!UWEHelpers.RequiresPower())
            {
                return 0f;
            }

            return Time.deltaTime * 0.1f * _audio?.volume ?? 0f;
        }

        public void ForceTimeChange(float amount, AudioClip clip)
        {
            _audio.time = Mathf.Min(amount, clip.length - 0.01f);
        }

        public float GetSliderValue()
        {
            return _trackSlider.value;
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
            return Mathf.Clamp01(Mathf.Min(b > 0f ? (t - a) / b : 0f, d > 0f ? (num - t) / d : 0f));
        }

        public bool IsPairing()
        {
            return _isPairing;
        }
    }

    
}
