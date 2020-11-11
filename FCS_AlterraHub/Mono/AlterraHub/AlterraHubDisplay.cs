using System;
using System.Collections.Generic;
using System.Diagnostics;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Systems;
using FCSCommon.Abstract;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    internal class AlterraHubDisplay : AIDisplay
    {
        private AlterraHubController _mono;
        private GameObject _homePage;
        private CheckOutPopupDialogWindow _checkoutDialog;
        private Text _mainTotal;
        private CartDropDownHandler _cartDropDownManager;
        internal Action<TechType,TechType> onItemAddedToCart;
        private PanelGroup _panelGroup;

        internal void Setup(AlterraHubController mono)
        {
            _mono = mono;

            if (FindAllComponents())
            {
                PowerOnDisplay();
                onItemAddedToCart += (tech, rTech) =>
                {
                    _cartDropDownManager?.AddItem(tech,rTech);
                };
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {

        }

        public override bool FindAllComponents()
        {
            try
            {
                #region Main

                var canvas = gameObject.GetComponentInChildren<Canvas>().gameObject;

                #endregion

                #region Home

                //HomePage
                _homePage = GameObjectHelpers.FindGameObject(canvas, "MainUI");

                _mainTotal = GameObjectHelpers.FindGameObject(_homePage, "CreditAmount").GetComponent<Text>();
                _mainTotal.text = CardSystem.main.GetAccountBalance().ToString("n0");
                CardSystem.main.onBalanceUpdated += amount =>
                {
                    _mainTotal.text = amount.ToString("n0");
                };

                //Body
                _panelGroup = GetDisplayBody();

                //Tabs
                GetDisplayTabs(_panelGroup);

                //Create CheckOut
                CreateCheckOutPopup(canvas);

                //Cart
                CreateCartDropDown(_homePage);

                //HomePage
                new AccountPageHandler(_mono);
                
                #endregion


                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
            }

            return false;
        }

        private void CreateCheckOutPopup(GameObject canvas)
        {
            var checkOutPopupGo = GameObjectHelpers.FindGameObject(canvas, "CheckOutPopUp");
            _checkoutDialog = checkOutPopupGo.AddComponent<CheckOutPopupDialogWindow>();
        }

        private void CreateCartDropDown(GameObject homePage)
        {
            var cartBTNObj = GameObjectHelpers.FindGameObject(homePage, "CartIcon");
            var closeBTN = GameObjectHelpers.FindGameObject(homePage, "CloseBTN").GetComponent<Button>();
            closeBTN.onClick.AddListener(() => { _cartDropDownManager.ToggleVisibility(); });

            var cartButtonNumber = GameObjectHelpers.FindGameObject(cartBTNObj, "CartAmount").GetComponentInChildren<Text>();
            var cartBTN = cartBTNObj.GetComponent<Button>();
            cartBTN.onClick.AddListener(() => { _cartDropDownManager.ToggleVisibility(); });
            
            var cartDropDown = GameObjectHelpers.FindGameObject(homePage, "CartDropDown");
            _cartDropDownManager = cartDropDown.AddComponent<CartDropDownHandler>();

            _cartDropDownManager.OnBuyAllBtnClick += handler =>
            {
                _checkoutDialog.ShowDialog(_mono,_cartDropDownManager);
                _cartDropDownManager.ToggleVisibility();
            };

            _cartDropDownManager.onTotalChanged += amount =>
            {
                cartButtonNumber.text = _cartDropDownManager.GetCartCount().ToString();
            };

            _cartDropDownManager.Initialize();
        }

        private void GetDisplayTabs(PanelGroup panelGroup)
        {
            QuickLogger.Debug("Getting Tabs");
            var tabs = GameObjectHelpers.FindGameObject(_homePage, "NavigationBar");
            var tabGroup = tabs.AddComponent<TabGroup>();
            tabGroup.tabIdle = Color.white;
            tabGroup.tabHover = Color.cyan;
            tabGroup.tabActive = Color.cyan;
            tabGroup.panelGroup = panelGroup;
            panelGroup.TabGroup = tabGroup;

            foreach (Transform child in tabs.transform)
            {
                var tabBar = GameObjectHelpers.FindGameObject(child.gameObject, "Tick");
                var tabButton = child.gameObject.AddComponent<TabButton>();
                tabButton.tabGroup = tabGroup;
                tabButton.onTabDeselected.AddListener(() => { tabBar.SetActive(false); });
                tabButton.onTabSelected.AddListener(() =>
                {
                    tabBar.SetActive(true);
                    _cartDropDownManager?.ToggleVisibility(true);
                });
                tabGroup.tabButtons = new List<TabButton> {tabButton};
                if (child.GetSiblingIndex() == 0)
                {
                    tabGroup.selectedTab = tabButton;
                }
            }

            tabGroup.Initialize();
            panelGroup.Initialize();
        }

        private PanelGroup GetDisplayBody()
        {
            var body = GameObjectHelpers.FindGameObject(_homePage, "Pages");
            var panelGroup = body.AddComponent<PanelGroup>();
            panelGroup.LinkPanels(body.GetChildren());
            return panelGroup;
        }

        public override void PowerOnDisplay()
        {
            _homePage.SetActive(true);
        }

        public override void PowerOffDisplay()
        {
            _homePage.SetActive(false);
        }

        internal PanelGroup GetPanelGroup()
        {
            return _panelGroup;
        }

        internal IEnumerable<CartItemSaveData> SaveCartItems()
        {
            return _cartDropDownManager.Save();
        }

        public void Load(AlterraHubDataEntry save)
        {
            if (save?.CartItems == null) return;

            foreach (CartItemSaveData cartItem in save.CartItems)
            {
                _cartDropDownManager.AddItem(cartItem.TechType,cartItem.ReceiveTechType);
            }
        }
    }
}