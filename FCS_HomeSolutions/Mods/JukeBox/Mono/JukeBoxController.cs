using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.JukeBox.Spectrum;
using FCSCommon.Utilities;
using RadicalLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.JukeBox.Mono
{
    internal  class JukeBoxController : FcsDevice, IFCSSave<SaveData>
    {
        private JukeBoxDataEntry _savedData;
        private bool _isFromSave;
        private bool _runStartUpOnEnable;
        private AudioSource _audio;
        private Slider _trackSlider;
        private Text _trackName;
        private bool _showSpectrum = false;
        private int _currentIndex;
        private bool _wasPlaying;
        private AudioClip _currentClip => _audio?.clip;

        //private IEnumerator SyncSources()
        //{
        //    while (true)
        //    {
        //        foreach (var slave in Manager.BaseSpeakersSources)
        //        {
        //            ((JukeBoxSpeakerController)slave).UpdateTimeSample(_audio);
        //            yield return null;
        //        }
        //    }
        //}
        
        async void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.JukeBoxTabID, Mod.ModPackID);
            await Mod.ScanForMusic();
        }

        private void Update()
        {
            if (_audio?.clip == null || _trackSlider == null || Manager == null) return;

            var gamePaused = Mathf.Approximately(Time.timeScale, 0f);

            if (_audio.isPlaying && gamePaused)
            {
                _audio.Pause();
                _wasPlaying = true;
                return;
            }

            if (gamePaused)
            {
                return;
            }

            SyncSources();


            if (_wasPlaying && !gamePaused)
            {
                _audio.Play();
                _wasPlaying = false;
            }

            if (_audio.isPlaying)
            {
                _trackSlider.SetValueWithoutNotify(_audio.time);
            }

            if (_audio.time >= _audio.clip.length && !_audio.loop)
            {
                NextTrack();
            }
        }

        private void SyncSources()
        {
            if (_audio.isPlaying)
            {
                foreach (var slave in Manager.BaseSpeakersSources)
                {
                    ((JukeBoxSpeakerController) slave).Play(_audio);
                }
            }
            else
            {
                foreach (var slave in Manager.BaseSpeakersSources)
                {
                    ((JukeBoxSpeakerController)slave).Stop();
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

                    //_colorManager.ChangeColor(_savedData.Fcs.Vector4ToColor());
                    //_colorManager.ChangeColor(_savedData.Secondary.Vector4ToColor(), ColorTargetMode.Secondary);
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
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }

            if (_audio == null)
            {
                _audio = gameObject.GetComponentInChildren<AudioSource>();
            }

            var playBTN = GameObjectHelpers.FindGameObject(gameObject, "PlayBTN").GetComponent<Button>();
            playBTN.onClick.AddListener(Play);
            
            var stopBTN = GameObjectHelpers.FindGameObject(gameObject, "StopBTN").GetComponent<Button>();
            stopBTN.onClick.AddListener(Stop);
            
            var previousBTN = GameObjectHelpers.FindGameObject(gameObject, "PreviousBTN").GetComponent<Button>();
            previousBTN.onClick.AddListener(PreviousTrack);
            
            var nextBTN = GameObjectHelpers.FindGameObject(gameObject, "NextBTN").GetComponent<Button>();
            nextBTN.onClick.AddListener(NextTrack);

            var repeatBTN = GameObjectHelpers.FindGameObject(gameObject, "Repeat").GetComponent<Toggle>();
            repeatBTN.onValueChanged.AddListener((value =>
            {
                _audio.loop = value;
            }));

            var audioSpectum = gameObject.AddComponent<AudioSpectrum>();
            audioSpectum.Initialize(_audio);

            var audioSync = gameObject.AddComponent<AudioSyncColor>();
            audioSync.bias = 6;
            audioSync.timeStep = 0.15f;
            audioSync.timeToBeat = 0.05f;
            audioSync.restColor = Color.black;
            audioSync.beatColors = new[]
            {
                Color.cyan,
                //new Color(0.04231142f,0.4341537f,0.3419144f),
                //new Color(0f,0.2581829f,0.2581829f),
            };

            var refreshBTN = GameObjectHelpers.FindGameObject(gameObject, "RefreshBTN").GetComponent<Button>();
            refreshBTN.onClick.AddListener((() =>
            {
                Purge();
                Mod.ScanForMusic();
            }));


            _trackSlider = GameObjectHelpers.FindGameObject(gameObject, "Timeline").GetComponent<Slider>();
            _trackSlider.onValueChanged.AddListener((amount =>
            {
                _audio.time = amount;
            }));

            _trackName = GameObjectHelpers.FindGameObject(gameObject, "TrackName").GetComponent<Text>();
            _trackName.text = string.Empty;

            var volumeSlider = GameObjectHelpers.FindGameObject(gameObject, "Volume").GetComponent<Slider>();
            volumeSlider.SetValueWithoutNotify(_audio.volume);


            var volumePercentage = GameObjectHelpers.FindGameObject(gameObject, "VolumePercentage").GetComponent<Text>();
            volumePercentage.text = CalculateVolumePercentage();

            volumeSlider.onValueChanged.AddListener((value =>
            {
                _audio.volume = value;
                volumePercentage.text = CalculateVolumePercentage();
            }));


            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseLightsEmissiveController, gameObject, Color.black);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseBeaconLightEmissiveController, gameObject, Color.red);
            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseBeaconLightEmissiveController, gameObject, 3f);

            IsInitialized = true;
        }

        private void NextTrack()
        {
            _currentIndex += 1;

            if (_currentIndex > Mod.Playlist.Count - 1)
            {
                _currentIndex = 0;
            }

            ChangeTrack(Mod.Playlist[_currentIndex]);
        }

        private void PreviousTrack()
        {
            _currentIndex -= 1;

            if (_currentIndex < 0)
            {
                _currentIndex = Mod.Playlist.Count - 1;
            }

            ChangeTrack(Mod.Playlist[_currentIndex]);
        }

        private string CalculateVolumePercentage()
        {
            return $"{Mathf.FloorToInt(_audio.volume / 1 * 100)}%";
        }

        private void ChangeTrack(TrackData track)
        {
            Purge();
            _audio.clip = track.AudioClip;
            _trackSlider.value = 0;
            _trackSlider.minValue = 0f;
            _trackSlider.maxValue = _audio.clip.length;
            _trackName.text = Path.GetFileNameWithoutExtension(track.Path);
            Play();
        }

        private void Purge()
        {
            if (_audio.isPlaying)
            {
                Stop();
                _audio.clip = null;
            }

            _trackName.text = string.Empty;
        }

        public void Stop()
        {
            _audio.Stop();
        }

        public void Play()
        {
            if (_audio.clip == null)
            {
                ChangeTrack(Mod.Playlist.First());
            }
            _audio.Play();
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
            _savedData.Primary = _colorManager.GetColor().ColorToVector4();
            _savedData.Secondary = _colorManager.GetSecondaryColor().ColorToVector4();
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.JukeBoxDataEntries.Add(_savedData);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }
    }
}
