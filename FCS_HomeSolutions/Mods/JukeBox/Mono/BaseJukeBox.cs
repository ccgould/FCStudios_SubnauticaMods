using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.JukeBox.Mono
{
    internal class BaseJukeBox : MonoBehaviour
    {
        public float TrackSliderValue { get; set; }
        public float Volume => _trackedDevice?.GetAudioSource()?.volume ?? 0f;
        public bool Loop { get; set; }
        public TrackData CurrentTrack { get; set; }
        public JukeBoxState State = JukeBoxState.Stop;
        private float _audioClipLength;
        private int _currentTrack;
        private bool _wasPlaying;
        private readonly HashSet<IJukeBoxDevice> _devices = new();
        private IJukeBoxDevice _trackedDevice;
        private float _outsideAudioFilter = 100f;
        private const float DEFAULT_AUDIO_FILTER = 22000f;

        private void Update()
        {
            if (CheckIfPaused())
            {
                Pause(_trackedDevice);
                return;
            }

            if (!CheckIfBaseHasPower())
            {
                Stop(_trackedDevice);
                IsPowered = false;
                return;
            }

            IsPowered = true;
            
            if (TrackedDeviceIsNullCheck()) return;

            TrackedDeviceOperationalCheck();

            UpdateTrackSlider();

            ResumePlayingCheck();
            
            TrackEndCheck();
        }

        public bool IsPowered { get; set; }

        private bool CheckIfBaseHasPower()
        {
            if (_trackedDevice?.GetBaseManager() == null) return false;

            if (_trackedDevice.GetBaseManager().GetPowerState() == PowerSystem.Status.Offline ||
                !_trackedDevice.GetBaseManager().HasEnoughPower(_trackedDevice.GetPowerUsage())) return false;

            return true;
        }

        private void TrackedDeviceOperationalCheck()
        {
            if (!_trackedDevice.IsOperational)
            {
                _trackedDevice = null;
            }
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
            if(_trackedDevice?.GetAudioSource()== null) return; 
            if (!IsPlaying && !Loop && State == JukeBoxState.Play)
            {
                QuickLogger.Debug($"End of track{CurrentTrack.Path}",true);
                SetTrack(JukeBox.Main.GetNextTrack(_currentTrack));
                OnPlay?.Invoke(CurrentTrack);
                QuickLogger.Debug($"Now playing {CurrentTrack.Path}", true);
            }
        }

        internal bool IsPlayerSwimming()
        {
            return Player.main.IsSwimming() || Player.main.IsUnderwaterForSwimming();
        }

        private bool TrackedDeviceIsNullCheck()
        {
            if (_trackedDevice != null) return false;

            _trackedDevice = _devices.FirstOrDefault(x => x.IsJukeBox && x.IsOperational);

            if(_devices.Count <= 0) Reset();

            return true;
        }

        private void UpdateTrackSlider()
        {
            if (_trackedDevice.IsOperational)
            {
                TrackSliderValue = _trackedDevice.GetAudioTime();
            }
        }

        private bool CheckIfPaused()
        {
            var gamePaused = WorldHelpers.CheckIfPaused();
            var audioSource = _trackedDevice?.GetAudioSource();

            if (audioSource != null && audioSource.isPlaying && gamePaused)
            {
                _wasPlaying = true;
            }
            
            return gamePaused;
        }

        private void Reset()
        {
            if(TrackSliderValue == 0 && _audioClipLength == 0 && !_wasPlaying && State == JukeBoxState.Stop) return;

            QuickLogger.Debug("Resetting Base JukeBox",true);
            TrackSliderValue = 0f;
            _audioClipLength = 0f;
            _wasPlaying = false;
            State = JukeBoxState.Stop;
        }

        private void SetTrack(TrackData trackData)
        {
            CurrentTrack = trackData;
            _currentTrack = trackData.Index;
            _audioClipLength = trackData.AudioClip.length;
        }

        internal void Play(IJukeBoxDevice device)
        {
            QuickLogger.Debug($"Play command from {device}",true);

            if (CurrentTrack.AudioClip == null)
            {
                var trackData = JukeBox.Main.GetFirstTrack();
                SetTrack(trackData);
            }

            OnPlay?.Invoke(CurrentTrack);
            State = JukeBoxState.Play;
            _trackedDevice = device;
        }


        internal void Pause(IJukeBoxDevice device)
        {
            if (State == JukeBoxState.Pause) return;
            State = JukeBoxState.Pause;
            _trackedDevice = device;
            OnPause?.Invoke();
        }

        internal void Stop(IJukeBoxDevice device)
        {
            State = JukeBoxState.Stop;
            _trackedDevice = device;
            TrackSliderValue = 0f;
            OnStop?.Invoke();
        }
        
        internal void SetVolume(IJukeBoxDevice device,float value)
        {
            OnVolumeChanged?.Invoke(value);
            _trackedDevice = device;
        }

        public void SetTrackSlider(IJukeBoxDevice device, float amount)
        {
            QuickLogger.Debug($"Set track slider {amount}");
            TrackSliderValue = amount;
            _trackedDevice = device;
        }

        public void PreviousTrack(IJukeBoxDevice device)
        {
            _trackedDevice = device;
            SetTrack(JukeBox.Main.GetPreviousTrack(_currentTrack));
            OnTrackChanged?.Invoke(CurrentTrack,IsPlaying);
        }

        public void NextTrack(IJukeBoxDevice device)
        {
            _trackedDevice = device;
            SetTrack(JukeBox.Main.GetNextTrack(_currentTrack));
            OnTrackChanged?.Invoke(CurrentTrack,IsPlaying);
        }

        public Action<TrackData,bool> OnTrackChanged { get; set; }

        public void SetLoop(JukeBoxController device, bool value)
        {
            _trackedDevice = device;
            Loop = value;
            OnLoopChanged?.Invoke(value);
        }

        public Action<bool> OnLoopChanged { get; set; }
        public Action OnPause { get; set; }
        public Action OnStop { get; set; }
        public Action OnResume { get; set; }
        public Action<TrackData> OnPlay { get; set; }
        public Action<float> OnVolumeChanged { get; set; }
        public int TimeSamples => _trackedDevice?.GetAudioSource()?.timeSamples ?? 0;
        public bool IsPlaying => _trackedDevice?.GetAudioSource()?.isPlaying ?? false;

        public JukeBoxState GetState()
        {
            return State;
        }
        
        public void RegisterDevice(IJukeBoxDevice device)
        {
            _devices.Add(device);
            if (_trackedDevice == null)
            {
                _trackedDevice = device;
            }
        }

        public void UnRegisterDevice(IJukeBoxDevice device)
        {
            _devices.Remove(device);

            if (device == _trackedDevice)
            {
                _trackedDevice = null;
            }
        }

        public AudioSource GetTrackedPlayerAudioSource()
        {
            return _trackedDevice?.GetAudioSource();
        }

        public bool HasTrackedJukeBox(IJukeBoxDevice checkIfNotDevice)
        {
            return _trackedDevice != null && _trackedDevice != checkIfNotDevice;
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
                Stop(null);
                OnRefresh?.Invoke(CurrentTrack);
            }));

        }

        public Action<TrackData> OnRefresh { get; set; }

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
    }
}
