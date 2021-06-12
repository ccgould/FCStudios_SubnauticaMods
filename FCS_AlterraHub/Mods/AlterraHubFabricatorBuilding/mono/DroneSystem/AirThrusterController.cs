using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem
{
    internal class AirThrusterController: MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private DroneController _droneController;

        private void Start()
        {
            _particleSystem = gameObject.GetComponent<ParticleSystem>();
            _droneController = gameObject.GetComponentInParent<DroneController>();
            InvokeRepeating(nameof(UpdateState),1f,1f);
        }

        private void UpdateState()
        {
            if(_droneController == null)return;
            ChangeThrusterState(_droneController.GetState() != DroneController.DroneStates.Docked);
        }

        private void ChangeThrusterState(bool isOn)
        {
            if (isOn)
            {
                _particleSystem.Play();
            }
            else
            {
                _particleSystem.Stop();
            }
        }
    }
}