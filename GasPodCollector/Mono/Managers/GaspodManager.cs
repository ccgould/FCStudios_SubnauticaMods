using System.Linq;
using FCSCommon.Utilities;
using GasPodCollector.Configuration;
using UnityEngine;

namespace GasPodCollector.Mono.Managers
{
    internal class GaspodManager : MonoBehaviour
    {
        private GaspodCollectorController _mono;
        private const float TriggerRangeSqr = 10f;
        private const float PickupRangeSqr = 30f;

        internal void Initialize(GaspodCollectorController mono)
        {
            _mono = mono;
        }

        private void Start()
        {
            InvokeRepeating(nameof(DetectPlayerInRadius), 1, 0.5f);
            InvokeRepeating(nameof(GetGaspodInRadius),1,0.5f);
            InvokeRepeating(nameof(DetectGasopodInRadius),1,0.5f);
        }

        private void DetectPlayerInRadius()
        {
            if (_mono == null || _mono.PowerManager == null) return;
            if (!_mono.PowerManager.HasPower()) return;

            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, TriggerRangeSqr);

            if (hitColliders.Length > 0)
            {
                var player = hitColliders.SingleOrDefault(x => x.gameObject.name.ToLower().StartsWith("player"));

                Mod.ProtectPlayer = player;
            }
        }

        //private void DestroyAllGasPods()
        //{
        //    if (_mono == null || _mono.PowerManager == null) return;

        //    if (!_mono.PowerManager.HasPower()) return;

        //    Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, PickupRangeSqr);

        //    if (hitColliders.Length > 0)
        //    {
        //        foreach (Collider collider in hitColliders)
        //        {
        //            if (collider.gameObject.GetComponentInChildren<GasPod>() && _mono.GaspodCollectorStorage.HasSpaceAvailable())
        //            {
        //                Destroy(collider.gameObject);
        //            }
        //        }
        //    }
        //}

        private void AddGasPod(Collider collider)
        {
            _mono.GaspodCollectorStorage.AddGaspod(collider);
        }

        private void DetectGasopodInRadius()
        {
            if(_mono == null || _mono.PowerManager == null) return;
            if (!_mono.PowerManager.HasPower()) return;

            if(Mod.ProtectPlayer) return;

            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, TriggerRangeSqr);

            if (hitColliders.Length > 0)
            {
                foreach (Collider collider in hitColliders)
                {
                    var gasopod = collider.gameObject?.GetComponentInChildren<GasoPod>();
                    if (gasopod && _mono.GaspodCollectorStorage.HasSpaceAvailable())
                    {
                        if (gasopod.timeLastGasPodDrop + gasopod.minTimeBetweenPayloads <= Time.time)
                        {
                            gasopod.timeLastGasPodDrop = Time.time;
                            gasopod.DropGasPods();
                        }
                    }
                }
            }
        }
        
        private void GetGaspodInRadius()
        {
            if (_mono == null || _mono.PowerManager == null) return;

            if (!_mono.PowerManager.HasPower()) return;

            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, PickupRangeSqr);

            if (hitColliders.Length > 0)
            {
                foreach (Collider collider in hitColliders)
                {
                    var gaspod = collider.gameObject.GetComponentInChildren<GasPod>();

                    if(!gaspod) continue;

                    if ( _mono.GaspodCollectorStorage.HasSpaceAvailable())
                    {
                        AddGasPod(collider);
                    }
                    else
                    {
                        if (Mod.ProtectPlayer)
                        {
                            gaspod.damagePerSecond = 0;
                        }
                    }
                }
            }
        }
    }
}
