using System;
using FCSCommon.Utilities;
using UnityEngine;

namespace GasPodCollector.Mono.Managers
{
    internal class GaspodCollectorStorage : MonoBehaviour
    {
        private int _currentAmount;
        private int _storageLimit = QPatch.Configuration.Config.StorageLimit;
        internal Action<int> OnAmountChanged { get; set; }

        internal bool HasSpaceAvailable()
        {
            return _currentAmount < _storageLimit;
        }

        internal void AddGaspod(Collider collider)
        {
            if (_currentAmount < _storageLimit)
            {
                _currentAmount += 1;
            }

            Destroy(collider.gameObject);

            OnAmountChanged?.Invoke(_currentAmount);

            //QuickLogger.Debug($"Gaspod Collector {gameObject.GetComponent<PrefabIdentifier>().Id} has {_currentAmount} items", true);
        }

        internal void RemoveGaspod()
        {

#if SUBNAUTICA
            var itemSize = CraftData.GetItemSize(TechType.GasPod);
#elif BELOWZERO
            var itemSize = TechData.GetItemSize(techType);
#endif
            if (Inventory.main.HasRoomFor(itemSize.x, itemSize.y))
            {
                if (_currentAmount > 0)
                {
                    _currentAmount -= 1;
                    var pickup = CraftData.InstantiateFromPrefab(TechType.GasPod).GetComponent<Pickupable>();
                    Inventory.main.Pickup(pickup);
                }
            }

            OnAmountChanged?.Invoke(_currentAmount);
        }

        internal int GetStorageAmount()
        {
            return _currentAmount;
        }

        internal void SetStorageAmount(int amount)
        {
            _currentAmount = amount > _storageLimit ? _storageLimit : amount;
        }
    }
}
