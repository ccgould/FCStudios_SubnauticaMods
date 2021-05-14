using System;
using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCS_EnergySolutions.Mods.WindSurfer.Interfaces;
using FCS_EnergySolutions.Mods.WindSurfer.Structs;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    public class PlatformController : MonoBehaviour
    {
        public GameObject[] Ports = new GameObject[4];
        public HoloGraphControl HoloGraphControl;
        private string _unitID;
        private PowerSource _powerSource;
        private WindSurferOperatorController _windSurferOperatorController;
        private FcsDevice _fcsDevice;
        private string _prefabID;

        internal WindSurferOperatorController WindSurferOperatorController
        {
            get
            {
                if (_windSurferOperatorController == null)
                {
                    _windSurferOperatorController = GetComponentInParent<PlatformController>().GetComponentInChildren<WindSurferOperatorController>();
                }

                return _windSurferOperatorController;
            }
        }

        internal ConnectedTurbineData ConnectionData { get; set; }

        public GameObject AddNewPlatForm(Transform target, int slotID,GameObject turbinePrefab)
        {
            WindSurferOperatorController.EnsureIsKinematic();
            var go = GameObject.Instantiate(turbinePrefab);
            go.transform.SetParent(target);
            go.transform.localPosition = new Vector3(11.10f, 0f, 0f);
            go.transform.SetParent(WindSurferOperatorController.TurbinesGroupLocation.transform, true);
            go.transform.localRotation = Quaternion.identity;
            var turbineController = go.GetComponent<WindSurferController>();
            turbineController.PoleState(true);
            return go;
        }

        public string GetUnitID()
        {
            if (string.IsNullOrWhiteSpace(_unitID))
            {
                _unitID = ((IPlatform)GetFcsDevice()).GetUnitID();
            }

            return _unitID;
        }

        private FcsDevice GetFcsDevice()
        {
            if (_fcsDevice == null)
            {
                _fcsDevice = gameObject.GetComponentInChildren<FcsDevice>();
            }

            return _fcsDevice;
        }

        public string GetPowerInfo()
        {
            if (GetPowerSource() == null) return "0/0";
            return $"{Mathf.RoundToInt(GetPowerSource().GetPower())}/{_powerSource.maxPower}";

        }


        public string GetPrefabID()
        {
            if (string.IsNullOrWhiteSpace(_prefabID))
            {
                _prefabID = ((IPlatform)GetFcsDevice()).GetPrefabID();
            }

            return _prefabID;
        }

        public IPowerInterface GetPowerSource()
        {
            if (_powerSource == null)
            {
                _powerSource = gameObject.GetComponentInChildren<PowerSource>();
            }

            return _powerSource;
        }
    }
}