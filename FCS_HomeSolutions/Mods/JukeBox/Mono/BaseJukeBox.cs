using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.JukeBox.Mono
{
    internal class BaseJukeBox : MonoBehaviour
    {
        public float Volume;
        public bool Loop { get; set; }
        public TrackData CurrentTrack { get; set; }
        public JukeBoxState State = JukeBoxState.Stop;
        private float _audioClipLength;
        private int _currentTrack;
        private bool _wasPlaying;
        private readonly List<IJukeBoxDevice> _devices = new();
        private float _outsideAudioFilter = 100f;
        private const float DEFAULT_AUDIO_FILTER = 22000f;

        private void Update()
        {
            if (CheckIfPaused())
            {
                Pause();
                return;
            }

            if (!CheckIfBaseHasPower())
            {
                Stop();
                IsPowered = false;
                return;
            }

            IsPowered = true;

            if (_devices.Count <= 0) Reset();
            
            ResumePlayingCheck();
            
            TrackEndCheck();
        }

        public bool IsPowered { get; set; }

        private bool CheckIfBaseHasPower()
        {
            if (BaseManager == null) return false;
            if (BaseManager.GetPowerState() == PowerSystem.Status.Offline ||
                !BaseManager.HasEnoughPower(Time.deltaTime * 0.1f * Volume)) return false;

            return true;
        }
        
        private void ResumePlayingCheck()
        {
            if (_wasPlaying)
            {
                QuickLogger.Debug("Resume Playing",true);
                State = JukeBoxState.Play;
                _wasPlaying = false;
                OnResume?.Invoke();
            }
        }

        private void TrackEndCheck()
        {
            //automatically go to the next music clip ,when the current music clip is fnished
            if (_audioClipLength - 0.1f <= GetFirstJukeBox().GetSliderValue() && !Loop)
            {
                NextTrack();
            }
        }

        internal bool IsPlayerSwimming()
        {
            return Player.main.IsSwimming() || Player.main.IsUnderwaterForSwimming();
        }
        
        internal bool HasJukeBox()
        {
            return GetFirstJukeBox != null;
        }

        private bool CheckIfPaused()
        {
            var gamePaused = WorldHelpers.CheckIfPaused();

            var jukeBox = GetFirstJukeBox();

            if (jukeBox == null) return true;
            
            var audioSource = jukeBox.GetAudioSource();

            if (audioSource != null && audioSource.isPlaying && gamePaused)
            {
                _wasPlaying = true;
            }
            
            return gamePaused;
        }

        internal bool IsPairing()
        {
            return GetFirstJukeBox().IsPairing();
        }

        private IJukeBoxDevice GetFirstJukeBox()
        {
            return _devices.FirstOrDefault(x=>x.IsOperational);
        }

        private void Reset()
        {
            if(_audioClipLength == 0 && !_wasPlaying && State == JukeBoxState.Stop) return;

            QuickLogger.Debug("Resetting Base JukeBox",true);
            _audioClipLength = 0f;
            _wasPlaying = false;
            State = JukeBoxState.Stop;
        }

        internal void SetTrack(TrackData trackData)
        {
            CurrentTrack = trackData;
            _currentTrack = trackData.Index;
            _audioClipLength = trackData.AudioClip.length;
        }

        internal void Play()
        {
            if (CurrentTrack.AudioClip == null)
            {
                var trackData = JukeBox.Main.GetFirstTrack();
                SetTrack(trackData);
            }

            OnPlay?.Invoke(CurrentTrack);
            State = JukeBoxState.Play;
        }

        internal void Pause()
        {
            if (State == JukeBoxState.Pause) return;
            State = JukeBoxState.Pause;
            OnPause?.Invoke();
        }

        internal void Stop()
        {
            State = JukeBoxState.Stop;
            OnStop?.Invoke();
        }
        
        internal void SetVolume(float value)
        {
            Volume = value;
            OnVolumeChanged?.Invoke(value);
        }

        public void ForceTimeChange(float amount,AudioClip clip)
        {
            QuickLogger.Debug($"Set track slider {amount}");
            foreach (IJukeBoxDevice jukeBoxDevice in _devices)
            {
                jukeBoxDevice.ForceTimeChange(amount,clip);
            }
        }

        public void PreviousTrack()
        {
            var wasPlaying = IsPlaying;
            Stop();
            SetTrack(JukeBox.Main.GetPreviousTrack(_currentTrack));
            OnTrackChanged?.Invoke(CurrentTrack, wasPlaying);
        }

        public void NextTrack()
        {
            var wasPlaying = IsPlaying;
            Stop();
            SetTrack(JukeBox.Main.GetNextTrack(_currentTrack));
            OnTrackChanged?.Invoke(CurrentTrack, wasPlaying);
        }

        public Action<TrackData,bool> OnTrackChanged { get; set; }

        public void SetLoop(JukeBoxController device, bool value)
        {
            Loop = value;
            OnLoopChanged?.Invoke(value);
        }

        public Action<bool> OnLoopChanged { get; set; }
        public Action OnPause { get; set; }
        public Action OnStop { get; set; }
        public Action OnResume { get; set; }
        public Action<TrackData> OnPlay { get; set; }
        public Action<float> OnVolumeChanged { get; set; }
        public int TimeSamples => GetFirstJukeBox()?.GetAudioSource()?.timeSamples ?? 0;
        public bool IsPlaying => GetFirstJukeBox()?.GetAudioSource()?.isPlaying ?? false;

        public JukeBoxState GetState()
        {
            return State;
        }
        
        public void RegisterDevice(IJukeBoxDevice device)
        {
            if (_devices.Contains(device)) return;
            _devices.Add(device);
        }

        public void UnRegisterDevice(IJukeBoxDevice device)
        {
            _devices.Remove(device);
        }
        

        public string GetCurrentTrackName()
        {
            return CurrentTrack.Path ?? string.Empty;
        }   

        public void Refresh()
        {
            var wasPlaying = IsPlaying;
            JukeBox.Main.Refresh((() =>
            {
                Stop();
                OnRefresh?.Invoke(CurrentTrack);
            }));

        }

        public Action<TrackData> OnRefresh { get; set; }
        public BaseManager BaseManager;
        public float GetCutOffFrequency(Constructable constructable)
        {
            if (!constructable.IsInside() && !IsPlayerSwimming() && !Player.main.IsInSub())
            {
                return DEFAULT_AUDIO_FILTER;
            }

            if (!constructable.IsInside() && Player.main.IsInSub())
            {
                return _outsideAudioFilter;
            }

            if (constructable.IsInside() && !Player.main.IsInSub() || IsPlayerSwimming())
            {
                return _outsideAudioFilter;
            }

            return  DEFAULT_AUDIO_FILTER;
        }

        public void PlayTrack(TrackData data)
        {
            var wasPlaying = IsPlaying;
            Stop();
            SetTrack(data);
            OnTrackChanged?.Invoke(CurrentTrack, wasPlaying);
        }
    }

    internal enum JukeBoxState
    {
        None,
        Play,
        Pause,
        Stop
    }

    internal interface IJukeBoxDevice
    {
        void Play(TrackData trackData);
        void Stop();
        void Pause();
        void SetVolume(float value);
        float GetAudioTime();
        bool IsJukeBox { get; }
        bool IsOperational { get; }
        AudioSource GetAudioSource();
        void OnTrackChanged(TrackData value, bool isPlaying);
        BaseManager GetBaseManager();
        float GetPowerUsage();
        void ForceTimeChange(float amount,AudioClip clip);
        float GetSliderValue();
        bool IsPairing();
    }
}
