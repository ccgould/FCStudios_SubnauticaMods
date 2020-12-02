using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.DeepDriller.Mono
{
    internal class FCSDeepDrillerTransferManager : MonoBehaviour
    {
        private FCSDeepDrillerController _mono;
        private readonly List<FcsDevice> _storagesList = new List<FcsDevice>();
        private bool _isAllowedToExport;

        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;
            InvokeRepeating(nameof(FindAlterraStorage),0.5f,0.5f);
            InvokeRepeating(nameof(CheckInternalStorageForItems),1f,1f);
        }

        internal void CheckInternalStorageForItems()
        {
            if (_mono == null || _mono.DeepDrillerContainer == null || !_isAllowedToExport) return;

            if(_mono.DeepDrillerContainer.HasItems())
            {
                var items = _mono.DeepDrillerContainer.GetItemsWithin();

                QuickLogger.Debug($"Items within Returned: {items?.Count}",true);

                if (items == null)return;

                for (int i = items.Count -1 ; i >= 0; i--)
                {
                    var curItem = items.ElementAt(i);
                    for (int j = curItem.Value - 1; j >= 0; j--)
                    {
                        QuickLogger.Debug($"Attempting to remove: {Language.main.Get(curItem.Key)}",true);
                        var result = TransferToAlterraStorage(curItem.Key);
                        QuickLogger.Debug($"Remove result: {result}", true);
                        if (result)
                        {
                            _mono.DeepDrillerContainer.OnlyRemoveItemFromContainer(curItem.Key,true);
                        }
                    }
                }
            }
        }

        internal bool TransferToAlterraStorage(TechType item)
        {
            foreach (FcsDevice connectableDevice in _storagesList)
            {
                if (connectableDevice.CanBeStored(1,item))
                {
                    return connectableDevice.AddItemToContainer(item.ToInventoryItem());
                }

                QuickLogger.Debug("AlterraStorage Cant Hold Item", true);
            }

            return false;
        }

        private void FindAlterraStorage()
        {
            _storagesList.Clear();

            for (int i = FCSAlterraHubService.PublicAPI.GetRegisteredDevices().Count- 1; i > -1; i--)
            {
                if (FCSAlterraHubService.PublicAPI.GetRegisteredDevices().ElementAt(i).Value.UnitID.StartsWith("AS"))
                {
                    if (CheckIfInRange(FCSAlterraHubService.PublicAPI.GetRegisteredDevices().ElementAt(i).Value))
                    {
                        _storagesList.Add(FCSAlterraHubService.PublicAPI.GetRegisteredDevices().ElementAt(i).Value);
                    }
                }
            }

            if (_storagesList.Any())
            {
                _mono.DisplayHandler.RefreshAlterraStorageList();
            }
        }

        private bool CheckIfInRange(FcsDevice device)
        {
            if (_mono == null || device == null || !device.gameObject.activeSelf || _mono.DeepDrillerPowerManager == null || !_mono.IsConstructed || _mono.DeepDrillerPowerManager.IsTripped()) return false;
            float distance = Vector3.Distance(_mono.gameObject.transform.position, device.gameObject.transform.position);
            if (distance <= QPatch.DeepDrillerMk3Configuration.DrillAlterraStorageRange)
            {
                return true;
            }
            return false;
        }
        
        internal void Toggle()
        {
            _isAllowedToExport = !_isAllowedToExport;
        }

        internal bool IsAllowedToExport()
        {
             return _isAllowedToExport;
        }

        public List<FcsDevice> GetTrackedAlterraStorage()
        {
            return _storagesList;
        }
    }
}
