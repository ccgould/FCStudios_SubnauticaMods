using System;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCSCommon.Extensions;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.AutoCrafter
{
    internal class DSSCraftManager : MonoBehaviour
    {
        private DSSAutoCrafterController _mono;
        private CraftMachine _craftSlot2;
        private CraftMachine _craftSlot1;
        private StatusController _statusController;
        private bool _startCrafting;
        private bool _isInitialized;
        private TechType _craftSlot1TechType;
        private TechType _craftSlot2TechType;
        private Transform[] _crafterBeltPath;
        private Transform[] _crafter2BeltPath;
        private int _itemsOnBelt;

        public void Initialize(DSSAutoCrafterController mono)
        {
            if (_isInitialized) return;
            _mono = mono;

            _crafterBeltPath = mono.gameObject.FindChild("Belt1WayPoints").GetChildrenT();
            _craftSlot1 = gameObject.AddComponent<CraftMachine>();
            _craftSlot1.CrafterID = 1;
            _craftSlot1.OnComplete += item =>
            {
                UpdateDisplayElements();
                if(!item.IsRecurring)
                {
                    _mono.DisplayManager.RemoveCraftingItem(item);
                    _mono.DisplayManager.Refresh();
                }

            };

            _crafter2BeltPath = mono.gameObject.FindChild("Belt2WayPoints").GetChildrenT();
            _craftSlot2 = gameObject.AddComponent<CraftMachine>();
            _craftSlot2.CrafterID = 2;
            _craftSlot2.OnComplete += item =>
            {
                UpdateDisplayElements();
                if (!item.IsRecurring)
                {
                    _mono.DisplayManager.RemoveCraftingItem(item);
                    _mono.DisplayManager.Refresh();
                }
            };
            InvokeRepeating(nameof(TryCraft),1,1);
            InvokeRepeating(nameof(UpdateDisplayElements),.1f,.1f);
            _isInitialized = true;
        }

        internal void SpawnItem(TechType techType,int id)
        {
            var path = id == 1 ? _crafterBeltPath : _crafter2BeltPath;

            //Spawn items
            var inv = GameObject.Instantiate(ModelPrefab.DSSCrafterCratePrefab);
            var follow = inv.gameObject.AddComponent<DSSAutoCrafterCrateController>();
            follow.TechType = techType;
            follow.Initialize(path, _mono);
            follow.OnPathComplete += OnPathComplete;
            _itemsOnBelt++;
        }

        private void OnPathComplete()
        {
            if (_itemsOnBelt == 0) return;
            _itemsOnBelt -= 1;
            if (_itemsOnBelt < 0) _itemsOnBelt = 0;
        }

        private void UpdateDisplayElements()
        {
            if (_craftSlot1 != null && _craftSlot2 != null && _statusController != null)
            {
                if (_craftSlot1.IsOccupied)
                {
                    _statusController.RefreshSlotOne(_craftSlot1TechType, _craftSlot1.GetComplete(), _craftSlot1.GetGoal());
    
                }

                if (_craftSlot2.IsOccupied)
                {
                    _statusController.RefreshSlotTwo(_craftSlot2TechType, _craftSlot2.GetComplete(), _craftSlot2.GetGoal());
                }
            }
        }

        private void TryCraft()
        {
            if(!_startCrafting) return;

            foreach (CraftingItem craftingItem in _mono.CraftingItems)
            {
                if (craftingItem.IsBeingCrafted) continue;

                if (!_craftSlot1.IsOccupied)
                {
                    _craftSlot1TechType = craftingItem.TechType;
                    _craftSlot1.StartCrafting(craftingItem,_mono);
                    continue;
                }

                if (!_craftSlot2.IsOccupied)
                {
                    _craftSlot2TechType = craftingItem.TechType;
                    _craftSlot2.StartCrafting(craftingItem,_mono);
                }
            }
        }

        public void StartOperation(StatusController statusController)
        {
            if (_statusController == null)
            {
                _statusController = statusController;
            }

            _startCrafting = true;
        }

        public void StopOperation()
        {
            _startCrafting = false;
        }

        public void Reset(bool bypass = false)
        {
            _craftSlot1.Reset(bypass);
            _craftSlot2.Reset(bypass);
        }

        public bool IsRunning()
        {
            return _startCrafting;
        }

        public bool ItemsOnBelt()
        {
            return _itemsOnBelt > 0;
        }
    }
}