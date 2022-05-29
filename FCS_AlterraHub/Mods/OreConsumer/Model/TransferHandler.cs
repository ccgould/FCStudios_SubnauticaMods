using System;
using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.OreConsumer.Model
{
    internal class TransferHandler : MonoBehaviour, IFCSStorage
    {
        private bool _isInitialized;
        private Action<string> _callBack;
        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
        public DumpContainer DumpContainer { get; set; }

        internal void Initialize()
        {
            if (_isInitialized) return;

            if (DumpContainer == null)
            {
                DumpContainer = gameObject.AddComponent<DumpContainer>();
                DumpContainer.Initialize(transform, Buildables.AlterraHub.TransferMoney(),this,1,1);
            }

            _isInitialized = true;
        }

        public bool CanBeStored(int amount, TechType techType)
        {
            return techType == Mod.DebitCardTechType;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            try
            {
                var card = item.item.gameObject.GetComponent<FcsCard>()?.GetCardNumber();
                _callBack.Invoke(card);
            }
            catch (Exception e)
            {
                QuickLogger.DebugError($"Message: {e.Message} || StackTrace: {e.StackTrace}");
            }
            finally
            {
                PlayerInteractionHelper.GivePlayerItem(item);
            }
            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return CanBeStored(0, pickupable.GetTechType());
        }

        public bool IsAllowedToRemoveItems()
        {
            return false;
        }

        public Pickupable RemoveItemFromContainer(TechType techType)
        {
            return null;
        }


        public Dictionary<TechType, int> GetItemsWithin()
        {
            return null;
        }

        public bool ContainsItem(TechType techType)
        {
            return false;
        }

        public ItemsContainer ItemsContainer { get; set; }
        public int StorageCount()
        {

            return ItemsContainer?.count ?? 0;
        }

        public void OpenStorage(Action<string> withDrawMoney)
        {
            if (!_isInitialized || DumpContainer == null) return;
            _callBack = withDrawMoney;
            DumpContainer.OpenStorage();

        }
    }
}