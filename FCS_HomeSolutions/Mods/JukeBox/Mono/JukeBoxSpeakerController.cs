﻿using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.JukeBox.Buildable;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.JukeBox.Mono
{
    internal  class JukeBoxSpeakerController : FcsDevice, IFCSSave<SaveData>
    {
        protected AudioSource _audio;
        private bool _runStartUpOnEnable;
        private AudioLowPassFilter _lowPassFilter;
        private BaseJukeBox _baseJukeBox;
        private JukeBoxDataEntry _savedData;
        private bool _isFromSave;
        private Constructable _constructable;
        protected bool BypassAudioVolumeSync;
        protected bool GetVolumeFromSave;
        protected bool _mutedByHand;

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

                    if (GetVolumeFromSave)
                    {
                        _audio.volume = _savedData.Volume;
                        _audio.mute = _savedData.IsMuted;
                    }
                    _mutedByHand = _savedData.IsMutedByHand;

                }

                _runStartUpOnEnable = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.JukeBoxEntrySaveData(GetPrefabID());
        }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, JukeBoxBuildable.JukeBoxTabID, Mod.ModPackID);
            _baseJukeBox = Manager.Habitat.GetComponent<BaseJukeBox>();
            _baseJukeBox.OnPlay += Play;
            _baseJukeBox.OnPause += Pause;
            _baseJukeBox.OnStop += Stop;
            _baseJukeBox.OnResume += Resume;
            _baseJukeBox.OnVolumeChanged += OnVolumeChanged;
            _baseJukeBox.OnTrackChanged += OnTrackChanged;

            if (_baseJukeBox.IsPlaying)
            {
                Play(_baseJukeBox.CurrentTrack);
                _audio.volume = _baseJukeBox.Volume;
            }
            InvokeRepeating(nameof(EnsurePlaying),1f,1f);
        }

        private void EnsurePlaying()
        {
            if (!_audio.isPlaying && _baseJukeBox.IsPlaying)
            {
                _audio.Play();
            }

            if (!BypassAudioVolumeSync)
            {
                _audio.volume = _baseJukeBox.Volume;
            }

            //_audio.volume = _baseJukeBox.Volume;
        }

        private void Resume()
        {
            _audio.UnPause();
        }

        private void OnTrackChanged(TrackData trackData, bool isPlaying)
        {
            if (isPlaying)
            {
                Play(trackData);
            }
            else
            {
                _audio.clip = trackData.AudioClip;
            }
        }

        private void Play(TrackData obj)
        {
            if (_audio == null || obj.AudioClip == null) return;
            _audio.clip = obj.AudioClip;
            _audio.time = Mathf.Min(0, obj.AudioClip.length - 0.01f);
            _audio.Play();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if(_baseJukeBox == null) return;
            _baseJukeBox.OnPause -= Pause;
            _baseJukeBox.OnStop -= Stop;
            _baseJukeBox.OnPlay -= Play;
            _baseJukeBox.OnResume -= Resume;
            _baseJukeBox.OnVolumeChanged -= OnVolumeChanged;
            _baseJukeBox.OnTrackChanged -= OnTrackChanged;
        }

        private void OnVolumeChanged(float value)
        {
            if (!BypassAudioVolumeSync)
            {
                _audio.volume = value;
            }
        }

        private void Stop()
        {
            _audio.Stop();
        }

        private void Pause()
        {
            _audio.Pause();
        }

        private void Update()
        {
            if (_audio == null || _baseJukeBox == null) return;
            CheckIfMuting();
            UpdateTimeSample();
            LowPassFilterCheck();
        }

        internal virtual void CheckIfMuting()
        {
            if (!_baseJukeBox.HasJukeBox() || _baseJukeBox.IsPairing())
            {
                Mute();
            }
            else
            {
                UnMute();
            }
        }

        internal void Mute()
        {
            _audio.mute = true;
        }

        internal void UnMute()
        {
            _audio.mute = false;
        }

        private void UpdateTimeSample()
        {
            if(_baseJukeBox == null) return;
            _audio.timeSamples = _baseJukeBox.TimeSamples;
        }

        private void LowPassFilterCheck()
        {
            if (_lowPassFilter != null && _baseJukeBox != null)
            {
                _lowPassFilter.cutoffFrequency = _baseJukeBox.GetCutOffFrequency(GetConstructable());
            }
        }

        public override void Initialize()
        {
            if (_audio == null)
            {
                _audio = gameObject.GetComponentInChildren<AudioSource>();
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol, AlterraHub.BaseLightsEmissiveController);
            }

            _constructable = gameObject.GetComponentInChildren<Constructable>();

            _lowPassFilter = gameObject.GetComponentInChildren<AudioLowPassFilter>();


            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseLightsEmissiveController, gameObject, Color.cyan);

            IsInitialized = true;
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

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new JukeBoxDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.Volume = _audio.volume;
            _savedData.IsMuted = _audio.mute;
            _savedData.IsMutedByHand = _mutedByHand;
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.JukeBoxDataEntries.Add(_savedData);
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }
    }
}
