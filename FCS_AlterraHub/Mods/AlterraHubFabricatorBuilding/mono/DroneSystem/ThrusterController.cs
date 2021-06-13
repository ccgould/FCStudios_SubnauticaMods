using UnityEngine;
using Valve.VR;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem
{
    internal class ThrusterController: MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private DroneController _droneController;
        internal bool isWaterSensitive = false;
        private Transform _trans;

        private void Start()
        {
            _particleSystem = gameObject.GetComponent<ParticleSystem>();
            _droneController = gameObject.GetComponentInParent<DroneController>();
            InvokeRepeating(nameof(UpdateState),1f,1f);
            _trans = gameObject.transform;
        }

        private void UpdateState()
        {
            if(_droneController == null)return;
            ChangeThrusterState(_droneController.GetState() != DroneController.DroneStates.Docked);
        }

        private void ChangeThrusterState(bool isOn)
        {
            if (isWaterSensitive)
            {
                if (IsUnderwater() && isOn)
                {
                    _particleSystem.Play(true);
                }
                else
                {
                    _particleSystem.Stop(true);
                }
                return;
            }

            if (isOn)
            {
                _particleSystem.Play(true);
            }
            else
            {
                _particleSystem.Stop(true);
            }
        }

        private bool IsUnderwater()
        {
            return _trans.position.y < Ocean.main.GetOceanLevel();
        }
    }
}