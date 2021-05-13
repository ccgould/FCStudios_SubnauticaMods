using System;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Objects;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    public class PlatformController : MonoBehaviour
    {
        public GameObject[] Ports = new GameObject[4];
        public HoloGraphControl HoloGraphControl;
        private string _unitID;

        internal WindSurferOperatorController WindSurferOperatorController => GetComponentInParent<PlatformController>().GetComponentInChildren<WindSurferOperatorController>();
        public Tuple<int, string, string, Vector2Int> ConnectionData { get; set; }

        public GameObject AddNewPlatForm(Transform target, int slotID,GameObject turbinePrefab)
        {
            WindSurferOperatorController.EnsureIsKinematic();
            var go = GameObject.Instantiate(turbinePrefab);
            go.transform.SetParent(target);
            go.transform.localPosition = new Vector3(11.10f, 0f, 0f);
            go.transform.SetParent(WindSurferOperatorController.Turbines.transform, true);
            go.transform.localRotation = Quaternion.identity;
            var turbineController = go.GetComponent<WindSurferController>();
            turbineController.PoleState(true);
            return go;
        }

        public string GetUnitID()
        {
            if (string.IsNullOrWhiteSpace(_unitID))
            {
                _unitID = gameObject.GetComponentInChildren<FcsDevice>().UnitID;
            }

            return _unitID;
        }
    }
}