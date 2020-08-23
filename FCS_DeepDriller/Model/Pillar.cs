using System.Collections.Generic;
using System.Linq;
using FCS_DeepDriller.Mono.MK2;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_DeepDriller.Model
{
    internal class Pillar : MonoBehaviour
    {

        private float _length;
        private bool _isDrillTunnel;
        private Pillar[] _pillars;
        private const float MaxPillarHeight = 1000f;
        private const float ExtraHeight = 20.9f;
        private const float MinHeight = 0f;

        internal bool IsExtended { get; private set; }

        public void Instantiate(FCSDeepDrillerController mono, bool isDrillTunnel = false)
        {

            _pillars = mono.GetAllComponentsInChildren<Pillar>();
            _isDrillTunnel = isDrillTunnel;
            gameObject.SetActive(false);
            InvokeRepeating(nameof(ExtendLeg),0.5f,0.5f);
        }

        internal void ExtendLeg()
        {
            //if (IsExtended) return;

            Vector3 position = transform.position;

            Vector3 forward = transform.TransformDirection(-Vector3.up);

            float num2 = float.MaxValue;
            float num3 = float.MaxValue;
            
            if (_isDrillTunnel)
            {
                var max = _pillars?.Max(x => x.GetLength()) ?? 0;
                //QuickLogger.Debug($"Max Returned: {max}", true);
                
                if (max <= 0)
                {
                    max = MaxPillarHeight;
                }

                SetPillarLength(max);
                return;
            }


            if (Physics.Raycast(position, forward, out var hit, MaxPillarHeight))
            {

                //QuickLogger.Debug($"Pillar {gameObject.name} hit {hit.collider.name} distance {hit.distance}", true); 


                Base componentInParent = hit.collider.GetComponentInParent<Base>();

                if (componentInParent && !componentInParent.isGhost)
                {
                    num2 = Mathf.Min(num2, hit.distance);
                }
                if (hit.collider.gameObject.layer == 30)
                {
                    num3 = Mathf.Min(num3, hit.distance);
                }
            }

            if (num3 < num2)
            {
                float newLength = (num3  / 2) + ExtraHeight;
                SetPillarLength(newLength);
            }
        }

        internal float GetLength()
        {
            return _length;
        }

        internal void SetPillarLength(float length)
        {
            _length = length;
            transform.localScale =  new Vector3(1f, length, 1f);
            IsExtended = true;
            gameObject.SetActive(true);
        }
    }
}
