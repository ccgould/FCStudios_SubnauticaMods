using FCS_AlterraHub.Helpers;
using UnityEngine;

namespace FCS_AlterraHub.Mods.Common.DroneSystem
{
    internal class ThrusterController: MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private DroneController _droneController;
        internal bool isWaterSensitive = false;
        private Transform _trans;
        private FMOD_CustomEmitter _audio;


        private void Start()
        {
            _audio = FModHelpers.CreateCustomLoopingEmitter(gameObject, "fire", "event:/env/background/fire");
            _particleSystem = gameObject.GetComponent<ParticleSystem>();
            _droneController = gameObject.GetComponentInParent<DroneController>();
            InvokeRepeating(nameof(UpdateState),1f,1f);
            _trans = gameObject.transform;
        }

        private void UpdateState()
        {
            if(_droneController == null)return;
            ChangeThrusterState(!_droneController.GetState()?.Name?.Equals("Idle") ?? false);
        }

        private void ChangeThrusterState(bool isOn)
        {
            if (_audio != null)
            {
                if (isOn && !_audio.playing)
                {
                    if (QPatch.Configuration.AlterraTransportDroneFxAllowed)
                    {
                        _audio.Play();
                    }
                }
                else if ((!QPatch.Configuration.AlterraTransportDroneFxAllowed || !isOn) && _audio.playing)
                {
                    _audio.Stop();
                }
            }

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
            return _trans.position.y < WorldHelpers.GetOceanDepth();
        }
    }
}