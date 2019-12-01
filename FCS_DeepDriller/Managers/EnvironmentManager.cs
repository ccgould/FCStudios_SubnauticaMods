using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FCS_DeepDriller.Managers
{
    internal class EnvironmentManager: MonoBehaviour
    {
        private float _inspectRadius;
        public bool allowedToSearch { get; set; }
        private void Inspect()
        {
            if (!allowedToSearch) return;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _inspectRadius);


            foreach (Collider hitCollider in hitColliders)
            {
                //TODO implement ExStorage
                //var controller = hitCollider.GetComponentInParent<ExStorageController>();
                //if (controller != null)
                //{
                //    Debug.Log($"Found unit: {controller.Name}");
                //}
            }
        }
    }
}
