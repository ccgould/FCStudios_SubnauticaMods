using System;
using System.Collections.Generic;
using System.Linq;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCS_StorageSolutions.Mods.AlterraStorage.Mono;
using FCSCommon.Abstract;
using FCSCommon.Components;
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
        private List<DSSInventoryItem> _inventoryButtons = new List<DSSInventoryItem>();

        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }

        internal void Setup(DSSTerminalController mono)
        {
            _mono = mono;

            if (FindAllComponents())
            {
                _itemGrid.DrawPage();
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "BaseDump":
                    _mono.OpenStorage();
                    break;
            }
        }

        public override bool FindAllComponents()
        {
            try
            {
                foreach (Transform invItem in GameObjectHelpers.FindGameObject(gameObject, "Grid").transform)
                {
                    var invButton = invItem.gameObject.EnsureComponent<DSSInventoryItem>();
                    invButton.ButtonMode = InterfaceButtonMode.HoverImage;
                    invButton.OnButtonClick += OnButtonClick;
                    _inventoryButtons.Add(invButton);
                }


                var addToBaseBTNObj = InterfaceHelpers.FindGameObject(gameObject, "AddToBaseBTN");
                InterfaceHelpers.CreateButton(addToBaseBTNObj, "BaseDump",InterfaceButtonMode.Background, OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f);

                _itemGrid = _mono.gameObject.EnsureComponent<GridHelperV2>();
                _itemGrid.OnLoadDisplay += OnLoadItemsGrid;
                _itemGrid.Setup(44, gameObject, Color.gray, Color.white, OnButtonClick);

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
                if (_isBeingDestroyed) return;

                var grouped = _mono?.GetItems();

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

    internal class DSSInventoryItem : InterfaceButton
    {
        private uGUI_Icon _icon;
        private Text _amount;

        private void Initialize()
        {
            if (_icon == null)
            {
                _icon = gameObject.FindChild("Icon").EnsureComponent<uGUI_Icon>();
            }

            if (_amount == null)
            {
                _amount = gameObject.FindChild("Text").EnsureComponent<Text>();
            }
        }

        internal void Set(TechType techType, int amount)
        {
            Initialize();
            Tag = techType;
            _amount.text = amount.ToString();
            _icon.sprite = SpriteManager.Get(techType);
            Show();
        }

        internal void Reset()
        {
            Initialize();
            _amount.text = "";
            _icon.sprite = SpriteManager.Get(TechType.None);
            Tag = null;
            Hide();
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
        }

        internal void Show()
        {
            gameObject.SetActive(false);
        }
    }
}
