using FCS_AlterraHub.Enumerators;
using FCS_ProductionSolutions.Buildable;
using FCSCommon.Helpers;
using UnityEngine;

namespace FCS_ProductionSolutions.HydroponicHarvester.Mono
{
    internal class LightManager
    {
        private HydroponicHarvesterController _controller;
        private Light[] _lights;
        private FCSLightState _lightState;

        internal LightManager(HydroponicHarvesterController controller)
        {
            _controller = controller;
            _lights = controller.gameObject.GetComponentsInChildren<Light>();
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
    }
}