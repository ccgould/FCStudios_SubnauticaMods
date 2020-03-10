using FCSCommon.Utilities;
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
            InvokeRepeating(nameof(GetGaspodInRadius),1,0.5f);
            InvokeRepeating(nameof(DetectGasopodInRadius),1,0.5f);
        }

        private void DetectGasopodInRadius()
        {
            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, TriggerRangeSqr);

            if (hitColliders.Length > 0)
            {
                foreach (Collider collider in hitColliders)
                {
                    var gasopod = collider.gameObject?.GetComponentInChildren<GasoPod>();
                    if (gasopod && _mono.GaspodCollectorStorage.HasSpaceAvailable())
                    {
                        QuickLogger.Debug($"In Range {collider.gameObject.name}", true);
                        
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
            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, PickupRangeSqr);

            if (hitColliders.Length > 0)
            {
                foreach (Collider collider in hitColliders)
                {
                    if (collider.gameObject.GetComponentInChildren<GasPod>() && _mono.GaspodCollectorStorage.HasSpaceAvailable())
                    {
                        QuickLogger.Debug($"In Range {collider.gameObject.name}", true);
                        _mono.GaspodCollectorStorage.AddGaspod(collider);
                    }
                }
            }
        }

    }
}
