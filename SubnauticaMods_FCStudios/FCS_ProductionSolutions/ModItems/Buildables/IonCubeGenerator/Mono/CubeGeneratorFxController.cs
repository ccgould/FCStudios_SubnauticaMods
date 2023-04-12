﻿using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using System;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono
{
    /// <summary>
    /// Controlls the FX for the ION Cube Generator
    /// </summary>
    internal class CubeGeneratorFxController : MonoBehaviour
    {
        private IonCubeGeneratorController _controller;
        private bool _isInitialized = false;
        public float Frequency = 4f;
        public float Magnitude = 5f;
        private ParticleSystem _fx;
        private FMOD_CustomLoopingEmitter _machineSound;

        private void Awake()
        {
            _fx = gameObject.GetComponentInChildren<ParticleSystem>();
        }

        internal void Initialize(IonCubeGeneratorController controller)
        {
            _controller = controller;

            if (_machineSound == null)
            {
                _machineSound = FModHelpers.CreateCustomLoopingEmitter(gameObject, "water_filter_loop", "event:/sub/base/water_filter_loop");
            }

            _isInitialized = true;
        }


        private void Update()
        {
            if (!_isInitialized) return;
            UpdateEmission();
            UpdateSFX();
            UpdateFX();
        }

        private void UpdateSFX()
        {
            if(!_controller.IsCrafting())
            {
                if(_machineSound.playing)
                {
                    _machineSound.Stop();
                }                
            }
            else
            {
                if (!_machineSound.playing)
                {
                    _machineSound.Play();
                }
            }
        }

        private void UpdateFX()
        {
            ///TODO STOP if Power Cuts Off
            if(_controller.CoolDownPercent > 0 && _fx.isPlaying)
            {
                _fx.Stop();
            }
            else if(_controller.GenerationPercent > 0f && !_fx.isPlaying)
            {
                _fx.Play();
            }
        }

        private void UpdateEmission()
        {
            ///TODO STOP if Power Cuts Off
            float intensity = !_controller.IsCrafting() ? 5f : Mathf.Clamp(Magnitude + Mathf.Sin(Time.timeSinceLevelLoad * Frequency) * Magnitude, 1, 5f);
            MaterialHelpers.ChangeEmissionStrength(ModPrefabService.BaseSecondaryCol, gameObject, intensity);
        }
    }
}