using System.Linq;
using FCS_ProductionSolutions.Buildable;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter
{
    internal class DSSCraftManager : MonoBehaviour
    {
        private DSSAutoCrafterController _mono;
        private CraftMachine _craftSlot1;
        private StatusController _statusController;
        private bool _startCrafting;
        private bool _isInitialized;
        private int _itemsOnBelt;
        private Transform[] _crafterBeltPath;

        public void Initialize(DSSAutoCrafterController mono)
        {
            if (_isInitialized) return;
            _mono = mono;

            _crafterBeltPath = GameObjectHelpers.FindGameObject(mono.gameObject, "Belt1WayPoints").GetChildrenT();
            _craftSlot1 = gameObject.AddComponent<CraftMachine>();
            _craftSlot1.CrafterID = 1;
            _craftSlot1.OnComplete += item =>
            {
                UpdateDisplayElements();
                if(!item.IsRecursive)
                {
                    _mono.DisplayManager.RemoveCraftingItem(item);
                    _mono.DisplayManager.Refresh();
                }

            };

            //InvokeRepeating(nameof(TryCraft),1,1);
            InvokeRepeating(nameof(UpdateDisplayElements),.1f,.1f);
            _isInitialized = true;
        }
        
        internal void SpawnItem(TechType techType)
        {
            if (_crafterBeltPath == null) return;
            //Spawn items
            var inv = GameObject.Instantiate(ModelPrefab.DSSCrafterCratePrefab);
            var follow = inv.AddComponent<DSSAutoCrafterCrateController>();
            follow.Initialize(_crafterBeltPath, _mono, techType);
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
            if (_craftSlot1 != null && _statusController != null)
            {
                if (_craftSlot1.IsOccupied)
                {
                    //_statusController.RefreshSlotOne(_craftSlot1TechType, _craftSlot1.GetComplete(), _craftSlot1.GetGoal());
                }
            }
        }

        private void TryCraft()
        {
            if(!_mono.CraftingItems.Any()) return;

            _craftSlot1.StartCrafting(_mono.CraftingItems[0], _mono);

            //foreach (CraftingOperation craftingItem in _mono.CraftingItems)
            //{
            //    if (craftingItem.IsBeingCrafted) continue;

            //    if (!_craftSlot1.IsOccupied)
            //    {

            //        continue;
            //    }
            //}
        }

        public void StartOperation()
        {
            _startCrafting = true;

            TryCraft();
        }

        public void StopOperation()
        {
            _startCrafting = false;
        }

        public void Reset(bool bypass = false)
        {
            _craftSlot1.Reset(bypass);
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