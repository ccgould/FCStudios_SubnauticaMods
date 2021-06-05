using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_ProductionSolutions.Buildable;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.HydroponicHarvester.Mono
{
    internal class EffectsManager
    {
        private readonly HydroponicHarvesterController _controller;
        private readonly Light[] _lights;
        private FCSLightState _lightState;
        private readonly ParticleSystem[] _slot1Bubbles;
        private readonly ParticleSystem[] _slot2Bubbles;
        private readonly ParticleSystem[] _slot3Bubbles;
        private readonly ParticleSystem[] _slot1Smoke;
        private readonly ParticleSystem[] _slot2Smoke;
        private readonly ParticleSystem[] _slot3Smoke;
        private bool _isBreakerSwitched;

        internal EffectsManager(HydroponicHarvesterController controller)
        {
            _controller = controller;
            _lights = controller.gameObject.GetComponentsInChildren<Light>();
            _slot1Bubbles = GameObjectHelpers.FindGameObject(controller.gameObject, "Slot_1_BubblesFx").GetComponentsInChildren<ParticleSystem>();
            _slot2Bubbles = GameObjectHelpers.FindGameObject(controller.gameObject, "Slot_2_BubblesFx").GetComponentsInChildren<ParticleSystem>();
            _slot3Bubbles = GameObjectHelpers.FindGameObject(controller.gameObject, "Slot_3_BubblesFx").GetComponentsInChildren<ParticleSystem>();
            _slot1Smoke = GameObjectHelpers.FindGameObject(controller.gameObject, "Slot_1_SmokeFx").GetComponentsInChildren<ParticleSystem>();
            _slot2Smoke = GameObjectHelpers.FindGameObject(controller.gameObject, "Slot_2_SmokeFx").GetComponentsInChildren<ParticleSystem>();
            _slot3Smoke = GameObjectHelpers.FindGameObject(controller.gameObject, "Slot_3_SmokeFx").GetComponentsInChildren<ParticleSystem>();
        }

        internal void ToggleLightsByDistance()
        {
            if (!QPatch.Configuration.HHIsLightTriggerEnabled)
            {
                TurnOnLights();
                return;
            }

            float distance = Vector3.Distance(_controller.transform.position, Player.main.camRoot.transform.position);
            if (distance <= QPatch.Configuration.HHLightTriggerRange)
            {
                if (!_isBreakerSwitched)
                {
                    TurnOnLights();
                }
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
                    PerformOperation(_slot1Smoke, play);
                else if (type == EffectType.Bubbles) PerformOperation(_slot1Bubbles, play);
            }
            else if (slot == 1)
            {
                if (type == EffectType.Smoke)
                    PerformOperation(_slot2Smoke, play);
                else if (type == EffectType.Bubbles) PerformOperation(_slot2Bubbles, play);
            }
            else if (slot == 2)
            {
                if (type == EffectType.Smoke)
                    PerformOperation(_slot3Smoke, play);
                else if (type == EffectType.Bubbles) PerformOperation(_slot3Bubbles, play);
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

        internal void SetBreaker(bool isTripped)
        {
            _isBreakerSwitched = isTripped;
            if (isTripped)
            {
                TurnOffLights();
            }
            else
            {
                TurnOnLights();
            }
        }

        public bool GetBreakerState()
        {
            return _isBreakerSwitched;
        }
    }

    internal enum EffectType    
    {
        Smoke,
        Bubbles
    }
}