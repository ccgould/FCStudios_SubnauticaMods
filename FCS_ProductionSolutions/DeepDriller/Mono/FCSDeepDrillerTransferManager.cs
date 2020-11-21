using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Mono;
using UnityEngine;

namespace FCS_ProductionSolutions.DeepDriller.Mono
{
    internal class FCSDeepDrillerTransferManager : MonoBehaviour
    {
        private FCSDeepDrillerController _mono;
        private readonly List<FcsDevice> _storagesList = new List<FcsDevice>();
        private Collider[] hitColliders;
        private bool _isAllowedToExport;

        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;

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
                        //var result = TransferToExStorage(curItem.Key);
                        //if (result)
                        //{
                        //    _mono.DeepDrillerContainer.OnlyRemoveItemFromContainer(curItem.Key);
                        //}
                    }
                }
            }
        }

        

        private void OnFCSDeviceDestroy(FcsDevice obj)
        {
            _storagesList.Remove(obj);
        }

        private void OnFCSDeviceAwake(FcsDevice obj)
        {
            //QuickLogger.Debug($"New Connectable Device: TechType to check {Mod.ExStorageTechType()} || Object TechType == {obj.GetTechType()}",true);
            //if (_storagesList.Contains(obj) || obj.GetTechType() != Mod.ExStorageTechType()) return;
            //if (!CheckIfInRange(obj)) return;
            //QuickLogger.Debug("New Connectable Device in range", true);
            //_storagesList.Add(obj);
        }

        private bool CheckIfInRange(FcsDevice device)
        {
            //if (_mono == null || _mono.DeepDrillerPowerManager == null || !_mono.IsConstructed || _mono.DeepDrillerPowerManager.IsTripped()) return false;
            //float distance = Vector3.Distance(_mono.gameObject.transform.position, device.gameObject.transform.position);

            //QuickLogger.Debug($"Object Distance {distance}",true);

            //if (distance <= QPatch.Configuration.DrillExStorageRange)
            //{
            //    QuickLogger.Debug("Adding to Connectable device");
            //    return true;
            //}
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
