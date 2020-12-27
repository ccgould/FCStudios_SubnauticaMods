using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Abstract;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class DSSTerminalDisplayManager : AIDisplay
    {
        private DSSTerminalController _mono;
        private GridHelperV2 _itemGrid;
        private bool _isBeingDestroyed;
        private readonly List<DSSInventoryItem> _inventoryButtons = new List<DSSInventoryItem>();
        private Text _baseName;
        private Text _serverAmount;
        private Text _rackCountAmount;
        private Text _totalItemsAmount;
        private BaseManager _currentBase;

        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }

        internal void Setup(DSSTerminalController mono)
        {
            _mono = mono;

            if (FindAllComponents())
            {
                _currentBase = _mono.Manager;
                InvokeRepeating(nameof(UpdateDisplay), .5f, .5f);
            }
        }

        private void UpdateDisplay()
        {
            var serverCount = _currentBase?.BaseServers.Count ?? 0;
            var fcsCount = _currentBase?.BaseFcsStorage.Sum(x => x.GetMaxStorage()) ?? 0;
            var lockersCount = _currentBase?.BaseStorageLockers.Sum(x => x.height * x.width) ?? 0;
            var item = _currentBase?.GetTotal() ?? 0;
            _itemGrid?.DrawPage();
            _baseName.text = _currentBase?.GetBaseName();
            _rackCountAmount.text = AuxPatchers.RackCountFormat(_currentBase?.BaseRacks.Count ?? 0);
            _serverAmount.text = AuxPatchers.ServerCountFormat(_currentBase?.BaseServers.Count ?? 0);
            _totalItemsAmount.text = AuxPatchers.TotalItemsFormat(item, serverCount * 48);
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "BaseDump":
                    _currentBase.OpenBaseStorage();
                    break;
                case "InventoryBTN":
                    var techType = (TechType)tag;
                    if (!PlayerInteractionHelper.CanPlayerHold(techType)) return;
                    var result = _currentBase.TakeItem(techType);
                    if (result != null)
                    {
                        UpdateDisplay();
                        PlayerInteractionHelper.GivePlayerItem(result);
                    }
                    break;
                case "RenameBTN":
                    _currentBase.ChangeBaseName();
                    break;
                case "BaseBTN":
                    _currentBase = (BaseManager) tag;
                    Refresh();
                    break;
            }
        }

        internal void Refresh()
        {
            _itemGrid.DrawPage();
        }

        public override bool FindAllComponents()
        {
            try
            {
                foreach (Transform invItem in GameObjectHelpers.FindGameObject(gameObject, "Grid").transform)
                {
                    var invButton = invItem.gameObject.EnsureComponent<DSSInventoryItem>();
                    invButton.ButtonMode = InterfaceButtonMode.HoverImage;
                    invButton.BtnName = "InventoryBTN";
                    invButton.OnButtonClick += OnButtonClick;
                    _inventoryButtons.Add(invButton);
                }

                var addToBaseBTNObj = InterfaceHelpers.FindGameObject(gameObject, "AddToBaseBTN");
                InterfaceHelpers.CreateButton(addToBaseBTNObj, "BaseDump", InterfaceButtonMode.Background, OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f);

                var renameBTNObj = GameObjectHelpers.FindGameObject(gameObject, "RenameBTN");
                InterfaceHelpers.CreateButton(renameBTNObj, "RenameBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f, AuxPatchers.Rename(), AuxPatchers.RenameDesc());

                var networkBTN = GameObjectHelpers.FindGameObject(gameObject, "NetworkBTN").EnsureComponent<NetworkDialogController>();
                networkBTN.Initialize(_mono.Manager,gameObject,this);

                _itemGrid = _mono.gameObject.EnsureComponent<GridHelperV2>();
                _itemGrid.OnLoadDisplay += OnLoadItemsGrid;
                _itemGrid.Setup(44, gameObject, Color.gray, Color.white, OnButtonClick);



                _baseName = GameObjectHelpers.FindGameObject(gameObject, "BaseName").GetComponent<Text>();
                _serverAmount = GameObjectHelpers.FindGameObject(gameObject, "ServerCount").GetComponent<Text>();
                _rackCountAmount = GameObjectHelpers.FindGameObject(gameObject, "RackCount").GetComponent<Text>();
                _totalItemsAmount = GameObjectHelpers.FindGameObject(gameObject, "TotalItems").GetComponent<Text>();

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Error(e.Source);
                return false;
            }

            return true;
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed || _mono == null) return;
                var grouped = _currentBase.GetItemsWithin();
                if (grouped == null) return;
                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }
                for (int i = data.EndPosition; i < data.MaxPerPage - 1; i++)
                {
                    _inventoryButtons[i].Reset();
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _inventoryButtons[i].Set(grouped.ElementAt(i).Key, grouped.ElementAt(i).Value);
                }

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }
    }
}
