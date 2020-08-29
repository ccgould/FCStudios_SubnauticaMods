using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Patchers;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using UnityEngine;

namespace FCS_DeepDriller.Mono.MK2
{
    internal class FCSDeepDrillerTransferManager : MonoBehaviour
    {
        private FCSDeepDrillerController _mono;
        private readonly List<FCSConnectableDevice> _storagesList = new List<FCSConnectableDevice>();
        private Collider[] hitColliders;
        private bool _isAllowedToExport;

        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;
            FCSConnectableAwake_Patcher.AddEventHandlerIfMissing(OnFCSDeviceAwake);
            FCSConnectableDestroy_Patcher.AddEventHandlerIfMissing(OnFCSDeviceDestroy);
            FindExStorages();
            //InvokeRepeating(nameof(CheckInternalStorageForItems),1f,1f);
        }

        internal void CheckInternalStorageForItems()
        {
            if (_mono == null || _mono.DeepDrillerContainer == null || !_isAllowedToExport) return;

            if(_mono.DeepDrillerContainer.HasItems())
            {
                var items = _mono.DeepDrillerContainer.GetItemsWithin();

                for (int i = items.Count -1 ; i >= 0; i--)
                {
                    var curItem = items.ElementAt(i);
                    for (int j = curItem.Value - 1; j >= 0; j--)
                    {
                        var result = TransferToExStorage(curItem.Key);
                        if (result)
                        {
                            _mono.DeepDrillerContainer.OnlyRemoveItemFromContainer(curItem.Key);
                        }
                    }
                }
            }
        }

        private void FindExStorages()
        {
            hitColliders = Physics.OverlapSphere(_mono.gameObject.transform.position, QPatch.Configuration.DrillExStorageRange);
            QuickLogger.Debug($"Collider Hits: {hitColliders.Length}", true);

            foreach (var hitCollider in hitColliders)
            {
                var cDevice = hitCollider.gameObject.GetComponentInChildren<FCSConnectableDevice>();
                if (cDevice != null)
                {
                    if (cDevice.GetTechType() == Mod.ExStorageTechType() && !_storagesList.Contains(cDevice))
                    {
                        _storagesList.Add(cDevice);
                    }
                }
            }
        }

        private void OnFCSDeviceDestroy(FCSConnectableDevice obj)
        {
            _storagesList.Remove(obj);
        }

        private void OnFCSDeviceAwake(FCSConnectableDevice obj)
        {
            QuickLogger.Debug($"New Connectable Device: TechType to check {Mod.ExStorageTechType()} || Object TechType == {obj.GetTechType()}",true);
            if (_storagesList.Contains(obj) || obj.GetTechType() != Mod.ExStorageTechType()) return;
            if (!CheckIfInRange(obj)) return;
            QuickLogger.Debug("New Connectable Device in range", true);
            _storagesList.Add(obj);
        }

        private bool CheckIfInRange(FCSConnectableDevice device)
        {
            if (_mono == null || _mono.PowerManager == null || !_mono.IsConstructed || _mono.PowerManager.IsTripped()) return false;
            float distance = Vector3.Distance(_mono.gameObject.transform.position, device.gameObject.transform.position);

            QuickLogger.Debug($"Object Distance {distance}",true);

            if (distance <= QPatch.Configuration.DrillExStorageRange)
            {
                QuickLogger.Debug("Adding to Connectable device");
                return true;
            }
            return false;
        }

        internal bool TransferToExStorage(TechType item)
        {
            foreach (FCSConnectableDevice connectableDevice in _storagesList)
            {
                if (connectableDevice.CanBeStored(1, item))
                {
                    return connectableDevice.AddItemToContainer(item.ToInventoryItem(), out var reason);
                }
            }
            
            return false;
        }

        internal void Toggle()
        {
            _isAllowedToExport = !_isAllowedToExport;
            
            if (_isAllowedToExport)
            {
                CheckInternalStorageForItems();
            }

            _mono.DisplayHandler.UpdateExport(_isAllowedToExport);
        }

        internal bool IsAllowedToExport()
        {
             return _isAllowedToExport;
        }
    }
}
