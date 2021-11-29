using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Model.GUI;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.ObjectPooler;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.Mods.DeepDriller.Patchers
{
    public class DeepDrillerGUIInventoryPage : DeepDrillerGUIPage
    {
        private readonly HashSet<uGUI_FCSDisplayItem> _trackedItems = new();
        private Text _itemCounter;
        private Text _paginator;
        private GridHelperPooled _inventoryGrid;
        private ObjectPooler _pooler;
        private const string InventoryPoolTag = "Inventory";

        public override void Awake()
        {
            base.Awake();

            if (_pooler == null)
            {
                _pooler = gameObject.AddComponent<ObjectPooler>();
                _pooler.AddPool(InventoryPoolTag, 12, ModelPrefab.DeepDrillerItemPrefab);
                _pooler.Initialize();
            }

            _paginator = gameObject.FindChild("Paginator").GetComponent<Text>();
            _itemCounter = gameObject.FindChild("InventoryLabel").GetComponent<Text>();

            #region Inventory Grid

            _inventoryGrid = gameObject.AddComponent<GridHelperPooled>();
            _inventoryGrid.OnLoadDisplay += OnLoadItemsGrid;
            _inventoryGrid.Setup(12, _pooler, gameObject, OnButtonClick,false);

            #endregion
        }

        private void OnButtonClick(string arg1, object tag)
        {
            if (arg1.Equals("ItemBTN"))
            {
                var item = (TechType)tag;
                Hud.GetContainer().RemoveItemFromContainer(item);
            }

        }

        private void OnLoadItemsGrid(DisplayDataPooled data)
        {
            try
            {
                if (IsBeingDestroyed) return;

                var grouped = Hud.GetItemsWithin();

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    if (CheckIfButtonIsActive(grouped.ElementAt(i).Key))
                    {
                        continue;
                    }

                    GameObject buttonPrefab = data.Pool.SpawnFromPool(InventoryPoolTag, data.ItemsGrid);
                    buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                    var itemBTN = buttonPrefab.EnsureComponent<uGUI_FCSDisplayItem>();
                    itemBTN.Initialize(grouped.ElementAt(i).Key,false,true);
                    itemBTN.UpdateAmount(1);
                    itemBTN.Subscribe((() =>
                    {
                        var techType = itemBTN.GetTechType();
                        Hud.GetContainer().RemoveItemFromContainer(techType);
                        var count = Hud.GetContainer().GetItemCount(techType);
                        if (count <= 0)
                        {
                            itemBTN.Hide();
                        }
                    }));

                    _trackedItems.Add(itemBTN);
                }
                _inventoryGrid.UpdaterPaginator(grouped.Count);
                RefreshStorageAmount();
            }
            catch (Exception e) 
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        internal void RefreshStorageAmount()
        {
            if (_itemCounter == null) return;
            _itemCounter.text = FCSDeepDrillerBuildable.InventoryStorageFormat(Hud.GetContainerTotal(), Hud.GetStorageSize());
        }

        private bool CheckIfButtonIsActive(TechType techType)
        {
            foreach (uGUI_FCSDisplayItem button in _trackedItems)
            {
                if (button.IsValidAndActive(techType))
                {
                    QuickLogger.Debug($"Button is valid: {techType} UpdatingButton", true);

                    var count = Hud.GetContainer().GetItemCount(techType);
                    if (count <= 0)
                    {
                        button.Hide();
                    }
                    else
                    {
                        button.UpdateAmount(count);
                    }

                    return true;
                }
            }

            return false;
        }
        
        public override void Show()
        {
            base.Show();
            Refresh();
        }

        public void Refresh()
        {
            _inventoryGrid.DrawPage();
        }
    }
}