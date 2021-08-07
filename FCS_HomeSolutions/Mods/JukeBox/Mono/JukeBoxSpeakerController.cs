using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.JukeBox.Mono
{
    internal  class JukeBoxSpeakerController : FcsDevice, IFCSSave<SaveData>
    {
        private AudioSource _audio;
        private bool _runStartUpOnEnable;

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }
                _runStartUpOnEnable = false;
            }
        }
        
        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.JukeBoxTabID, Mod.ModPackID);

            if (Manager != null)
            { 
                Manager.RegisterSpeaker(this);
            }
        }

        private void Update()
        {
            if (_audio == null) return;

            if (_audio.isPlaying && Mathf.Approximately(Time.timeScale, 0f))
            {
                _audio.Pause();
                return;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Manager?.UnRegisterSpeaker(this);
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
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseLightsEmissiveController, gameObject, Color.cyan);
        }

        internal void Play(AudioSource audio)
        {
            if (_audio.isPlaying && Mathf.Approximately(Time.timeScale, 0f)) return;
                _audio.clip = audio.clip;
            _audio.timeSamples = audio.timeSamples;
            _audio.volume = audio.volume;

            if (audio.isPlaying && !_audio.isPlaying)
            {
                _audio.Play();
            }
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {

        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {

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

        }

        public void Stop()
        {
            if(_audio == null) return;

            if(_audio.isPlaying)
                _audio?.Stop();
        }
    }
}
