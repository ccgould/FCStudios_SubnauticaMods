using System;
using FCS_AlterraHub.Enumerators;
using FCS_ProductionSolutions.Buildable;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.HydroponicHarvester.Mono
{
    internal class EffectsManager
    {
        private HydroponicHarvesterController _controller;
        private Light[] _lights;
        private FCSLightState _lightState;
        private ParticleSystem[] _slot1_Bubbles;
        private ParticleSystem[] _slot2_Bubbles;
        private ParticleSystem[] _slot3_Bubbles;
        private ParticleSystem[] _slot1_Smoke;
        private ParticleSystem[] _slot2_Smoke;
        private ParticleSystem[] _slot3_Smoke;

        internal EffectsManager(HydroponicHarvesterController controller)
        {
            _controller = controller;
            _lights = controller.gameObject.GetComponentsInChildren<Light>();
            _slot1_Bubbles = GameObjectHelpers.FindGameObject(controller.gameObject, "Slot_1_BubblesFx").GetComponentsInChildren<ParticleSystem>();
            _slot2_Bubbles = GameObjectHelpers.FindGameObject(controller.gameObject, "Slot_2_BubblesFx").GetComponentsInChildren<ParticleSystem>();
            _slot3_Bubbles = GameObjectHelpers.FindGameObject(controller.gameObject, "Slot_3_BubblesFx").GetComponentsInChildren<ParticleSystem>();
            _slot1_Smoke = GameObjectHelpers.FindGameObject(controller.gameObject, "Slot_1_SmokeFx").GetComponentsInChildren<ParticleSystem>();
            _slot2_Smoke = GameObjectHelpers.FindGameObject(controller.gameObject, "Slot_2_SmokeFx").GetComponentsInChildren<ParticleSystem>();
            _slot3_Smoke = GameObjectHelpers.FindGameObject(controller.gameObject, "Slot_3_SmokeFx").GetComponentsInChildren<ParticleSystem>();
        }

        internal void ToggleLightsByDistance()
        {
            if (!QPatch.HarvesterConfiguration.IsLightTriggerEnabled)
            {
                TurnOnLights();
                return;
            }

            float distance = Vector3.Distance(_controller.transform.position, Player.main.camRoot.transform.position);
            if (distance <= QPatch.HarvesterConfiguration.LightTriggerRange)
            {
                TurnOnLights();
            }
            else
            {
                TurnOffLights();
            }
        }
        
        internal void TurnOnLights()
        {
            if (_lightState == FCSLightState.On) return;
            foreach (Light spotLights in _lights)
            {
                spotLights.enabled = true;
            }
            MaterialHelpers.ToggleEmission(ModelPrefab.EmissionControllerMaterial, _controller.gameObject, true);

            _lightState = FCSLightState.On;
        }

        internal void TurnOffLights()
        {
            if (_lightState == FCSLightState.Off) return;
            foreach (Light spotLights in _lights)
            {
                spotLights.enabled = false;
            }

            MaterialHelpers.ToggleEmission(ModelPrefab.EmissionControllerMaterial, _controller.gameObject, false);

            _lightState = FCSLightState.Off;
        }

        internal void ChangeEffectState(EffectType type, int slot,bool play)
        {
            QuickLogger.Debug($"Effect Type: {type}, Slot:{slot},IsToPlay: {play}",true);
            if (slot == 0)
            {
                if (type == EffectType.Smoke)
                    PerformOperation(_slot1_Smoke, play);
                else if (type == EffectType.Bubbles) PerformOperation(_slot1_Bubbles, play);
            }
            else if (slot == 1)
            {
                if (type == EffectType.Smoke)
                    PerformOperation(_slot2_Smoke, play);
                else if (type == EffectType.Bubbles) PerformOperation(_slot2_Bubbles, play);
            }
            else if (slot == 2)
            {
                if (type == EffectType.Smoke)
                    PerformOperation(_slot3_Smoke, play);
                else if (type == EffectType.Bubbles) PerformOperation(_slot3_Bubbles, play);
            }
        }

        private void PerformOperation(ParticleSystem[] particles, bool play)
        {
            if (play)
            {
                foreach (ParticleSystem system in particles)
                {
                    QuickLogger.Debug($"Playing effect",true);
                    system.Play();
                }
            }
            else
            {
                foreach (ParticleSystem system in particles)
                {
                    QuickLogger.Debug($"Stopping effect", true);
                    system.Stop();
                }
            }
        }
    }

    internal enum EffectType    
    {
        Smoke,
        Bubbles
    }
}