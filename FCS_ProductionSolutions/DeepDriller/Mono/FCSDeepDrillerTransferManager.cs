using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FCS_AlterraHub.Helpers;
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
        private bool _isSorting;
        private int _unsortableItems;
        private bool _sortedItem;
        public const float SortInterval = 1f;

        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;

            InvokeRepeating(nameof(FindAlterraStorage),1f,1f);
        }

        private IEnumerator Start()
        {
            while (true)
            {
                
                yield return new WaitForSeconds(Mathf.Max(0, SortInterval - (_unsortableItems / 60.0f)));
                yield return Sort();
            }
        }
        
        private IEnumerator Sort()
        {
            if (_mono?.DeepDrillerContainer == null) yield break;

            if (!_isAllowedToExport)
            {
                _isSorting = false;
                yield break;
            }

            _sortedItem = false;
            _unsortableItems = _mono.DeepDrillerContainer.GetContainerTotal();

            if (_mono.DeepDrillerContainer.IsEmpty())
            {
                _isSorting = false;
                yield break;
            }
            
            if (_storagesList.Count <= 0)
            {
                _isSorting = false;
                yield break;
            }
            
            yield return SortAnyTargets();

            if (_sortedItem)
            {
                yield break;
            }

            _isSorting = false;
        }

#if SUBNAUTICA_STABLE
        private IEnumerator SortAnyTargets()
        {
            int callsToCanAddItem = 0;
            const int CanAddItemCallThreshold = 10;
            foreach (var item in _mono.DeepDrillerContainer.GetItemsWithin())
            {
                foreach (FcsDevice target in _storagesList)
                {

                    for (int i = 0; i < item.Value; i++)
                    {
                        callsToCanAddItem++;
                        if (target.CanBeStored(1, item.Key))
                        {
                            SortItem(item.Key, target);
                            _unsortableItems--;
                            _sortedItem = true;
                            yield break;
                        }

                        if (callsToCanAddItem > CanAddItemCallThreshold)
                        {
                            callsToCanAddItem = 0;
                            goto skip;
                        }
                    }

                    skip:;
                }
            }
        }
#else
        private IEnumerator SortAnyTargets()
        {
            int callsToCanAddItem = 0;
            const int CanAddItemCallThreshold = 10;
            foreach (var item in _mono.DeepDrillerContainer.GetItemsWithin())
            {
                foreach (FcsDevice target in _storagesList)
                {

                    for (int i = 0; i < item.Value; i++)
                    {
                        callsToCanAddItem++;
                        if (target.CanBeStored(1, item.Key))
                        {
                            yield return SortItem(item.Key, target);
                            _unsortableItems--;
                            _sortedItem = true;
                            yield break;
                        }

                        if (callsToCanAddItem > CanAddItemCallThreshold)
                        {
                            callsToCanAddItem = 0;
                            goto skip;
                        }
                    }

                    skip:;
                }
            }
        }
#endif

#if SUBNAUTICA_STABLE
        private void SortItem(TechType techType, FcsDevice target)
        {
            _mono.DeepDrillerContainer.OnlyRemoveItemFromContainer(techType);
            target.AddItemToContainer(techType.ToInventoryItemLegacy());
        }
#else
        private IEnumerator SortItem(TechType techType, FcsDevice target)
        {
            _mono.DeepDrillerContainer.OnlyRemoveItemFromContainer(techType);

            TaskResult<InventoryItem> result = new TaskResult<InventoryItem>();
            yield return AsyncExtensions.ToInventoryItemLegacyAsync(techType, result);
            target.AddItemToContainer(result.Get());
            yield break;
        }
#endif

        private void FindAlterraStorage()
        {
            _storagesList.Clear();

            var foundDevices = FCSAlterraHubService.PublicAPI.GetRegisteredDevicesOfId("AS");

            for (int i = foundDevices.Count- 1; i > -1; i--)
            {
                if (WorldHelpers.CheckIfInRange(_mono, foundDevices.ElementAt(i).Value, QPatch.Configuration.DDDrillAlterraStorageRange))
                {
                    _storagesList.Add(foundDevices.ElementAt(i).Value);
                }
            }

            if (_storagesList.Any())
            {
                _mono.DisplayHandler.RefreshAlterraStorageList(_storagesList.Count);
            }
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