using FCSCommon.Utilities;
using UnityEngine;

namespace GasPodCollector.Mono.Managers
{
    internal class GaspodManager : MonoBehaviour
    {
        private GaspodCollectorController _mono;
        private const float triggerRangeSqr = 10f;
        private const float pickupRangeSqr = 30f;

        internal void Initialize(GaspodCollectorController mono)
        {
            _mono = mono;
        }

        private void Start()
        {
            InvokeRepeating(nameof(GetInactiveInRadius),1,0.5f);
            InvokeRepeating(nameof(DetectGasopod),1,0.5f);
        }

        void DetectGasopod()
        {
            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, triggerRangeSqr);

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
        
        void GetInactiveInRadius()
        {
            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, pickupRangeSqr);

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
