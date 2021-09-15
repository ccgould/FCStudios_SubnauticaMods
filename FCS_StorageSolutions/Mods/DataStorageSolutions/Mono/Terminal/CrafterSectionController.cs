using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;
using WorldHelpers = FCS_AlterraHub.Helpers.WorldHelpers;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class CrafterSectionController : MonoBehaviour,IFCSDisplay
    {
        private DSSTerminalDisplayManager _mono;
        private GridHelperV2 _grid;
        private readonly List<CraftingOperationItem> _craftingOperationItem = new List<CraftingOperationItem>();
        private PaginatorController _paginatorController;
        private CraftingDialogController _craftingDialog;

        public void Initialize(DSSTerminalDisplayManager mono)
        {
            _mono = mono;

            _paginatorController = GameObjectHelpers.FindGameObject(gameObject, "Paginator").AddComponent<PaginatorController>();
            _paginatorController.Initialize(this);
           
            _grid = GameObjectHelpers.FindGameObject(gameObject, "Grid").EnsureComponent<GridHelperV2>();
            
            foreach (Transform child in _grid.transform)
            {
                var craftingOperation = child.gameObject.EnsureComponent<CraftingOperationItem>();
                craftingOperation.Initialize(this,mono);
                _craftingOperationItem.Add(craftingOperation);
            }

            _grid.OnLoadDisplay += OnLoadItemsGrid;
            _grid.Setup(28, gameObject, Color.gray, Color.white, null);

            _craftingDialog = GameObjectHelpers.FindGameObject(mono.gameObject, "CraftingDialog").AddComponent<CraftingDialogController>();
            _craftingDialog.Initialize(mono,this);

            var addCraftingBTN = GameObjectHelpers.FindGameObject(gameObject, "AddOperationBTN").GetComponent<Button>();
            
            addCraftingBTN.onClick.AddListener((() =>
            {
                _craftingDialog.Show();
            }));

            Refresh();
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                if (_mono == null) return;

                var grouped = _mono.GetController().Manager.GetBaseCraftingOperations();

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _craftingOperationItem[i].Reset();
                }

                int w = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _craftingOperationItem[w++].Set(grouped.ElementAt(i));
                }

                _grid.UpdaterPaginator(grouped.Count);
                _paginatorController.ResetCount(_grid.GetMaxPages());

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        public void GoToPage(int index)
        {
            _grid.DrawPage(index);
        }

        public void GoToPage(int index, PaginatorController sender)
        {
            _grid.DrawPage(index);
        }

        private class CraftingOperationItem : MonoBehaviour
        {
            private bool _isInitialized;
            private Text _title;
            private uGUI_Icon _icon;
            private CraftingOperation _craftingOperation;
            private readonly StringBuilder _sb = new StringBuilder();

            public void Initialize(CrafterSectionController mono, DSSTerminalDisplayManager displayManager)
            {
                if (_isInitialized) return;
                _title = gameObject.GetComponentInChildren<Text>();
                _icon = GameObjectHelpers.FindGameObject(gameObject, "Icon").AddComponent<uGUI_Icon>();

                var toolTip = gameObject.AddComponent<FCSToolTip>();
                toolTip.RequestPermission += () => WorldHelpers.CheckIfInRange(gameObject,Player.main.gameObject,2);
                toolTip.ToolTipStringDelegate += ToolTipStringDelegate;

                var deleteBtn = gameObject.GetComponentInChildren<Button>();
                deleteBtn.onClick.AddListener((() =>
                {
                    displayManager.GetController().Manager.RemoveCraftingOperation(_craftingOperation);
                    mono.Refresh();
                }));

                _isInitialized = true;
            }

            private string ToolTipStringDelegate()
            {
                _sb.Clear();

                _sb.Append($"Is Crafting: {_craftingOperation.IsBeingCrafted}");
                _sb.Append(Environment.NewLine);
                _sb.Append($"Is Recursive: {_craftingOperation.IsRecursive}");
                _sb.Append(Environment.NewLine);

                var amount = _craftingOperation.IsRecursive ? "\u221E" : _craftingOperation.Amount.ToString();
                _sb.Append($"Amount: {amount}");
                _sb.Append(Environment.NewLine);
                _sb.Append($"Is Being Crafted: {_craftingOperation.IsBeingCrafted}");
                _sb.Append(Environment.NewLine);
                return _sb.ToString();
            }


            //Continue to set up the crafting item icon amount and hover

            internal void Set(CraftingOperation craftingOperation)
            {
                _craftingOperation = craftingOperation;
                _title.text = craftingOperation.IsRecursive ? "\u221E" : craftingOperation.Amount.ToString();
                _icon.sprite = SpriteManager.Get(craftingOperation.TechType);
                gameObject.SetActive(true);
            }

            internal void Reset()
            {
                gameObject.SetActive(false);
            }
        }

        internal void Refresh()
        {
            _grid.DrawPage();
        }
    }
}