using System.IO;
using FCS_AlterraHub.Configuration;
using SMLHelper.V2.Utility;
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
        private FMOD_CustomLoopingEmitter _thruster;


        private void Start()
        {
            _particleSystem = gameObject.GetComponent<ParticleSystem>();
            _droneController = gameObject.GetComponentInParent<DroneController>();
            InvokeRepeating(nameof(UpdateState),1f,1f);
            _trans = gameObject.transform;


            //_thruster = gameObject.AddComponent<FMOD_CustomLoopingEmitter>();
            //var fModAsset = ScriptableObject.CreateInstance<FMODAsset>();
            //fModAsset.id = "bootster";
            //fModAsset.name = "";
            //fModAsset.path = Path.Combine(Mod.GetAssetPath(), "Audio", "booster.mp3");
            //_thruster.asset = fModAsset;
            //_thruster.restartOnPlay = true;

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
                //_thruster.Play();
            }
            else
            {
                _particleSystem.Stop(true);
                //_thruster.Stop();
            }
        }

        private bool IsUnderwater()
        {
            return _trans.position.y < Ocean.main.GetOceanLevel();
        }
    }
}