using FCS_AlterraHub.Mono;
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
        private CraftingOperation _craftingItem;

        public void Initialize(DSSAutoCrafterController mono)
        {
            if (_isInitialized) return;
            _mono = mono;

            _crafterBeltPath = GameObjectHelpers.FindGameObject(mono.gameObject, "Belt1WayPoints").GetChildrenT();
            _craftSlot1 = gameObject.AddComponent<CraftMachine>();
            _craftSlot1.CrafterID = 1;
            _craftSlot1.OnComplete += item =>
            {
                if(!item.IsRecursive)
                {
                    _mono.Manager.RemoveCraftingOperation(item);
                    StopOperation();
                    item.UnMount(_mono);
                    _mono.DisplayManager.Clear();
                    _mono.Manager.NotifyByID("DTC", "RefreshCraftingGrid");
                }
            };

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
        
        private void TryCraft()
        {
            if(_craftingItem == null) return;

            _craftSlot1.StartCrafting(_craftingItem, _mono);
        }

        public void StartOperation(CraftingOperation operation)
        {
            _craftingItem = operation;
            TryCraft();
        }

        public void StopOperation()
        {
            _craftingItem = null;
        }

        public void Reset(bool bypass = false)
        {
            _craftSlot1.Reset(bypass);
        }

        public bool IsRunning()
        {
            return _craftingItem != null;
        }

        public bool ItemsOnBelt()
        {
            return _itemsOnBelt > 0;
        }

        public CraftingOperation GetCraftingOperation()
        {
            return _craftingItem;
        }
    }
}