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
        internal Action<TechType,TechType,int> onItemAddedToCart;
        private PanelGroup _panelGroup;
        private AccountPageHandler _accountPageHandler;
        private Text _cartButtonNumber;
        public Action OnReturnButtonClicked { get; set; }

        private void OnDestroy()
        {
            QuickLogger.Debug("Distroying screen",true);
            _accountPageHandler = null;
            CardSystem.main.onBalanceUpdated -= OnReturnButtonClicked;
            onItemAddedToCart -= onItemAddedToCart;
            _cartDropDownManager.OnBuyAllBtnClick -= OnBuyAllBtnClick;
            _cartDropDownManager.onTotalChanged -= OnTotalChanged;

        }

        internal void Setup(AlterraHubController mono)
        {
            _mono = mono;

            if (FindAllComponents())
            {
                PowerOnDisplay();
                onItemAddedToCart += OnItemAddedToCart;
                CardSystem.main.onBalanceUpdated += OnBalanceUpdated;
            }
        }

        private void OnItemAddedToCart(TechType tech, TechType rTech,int returnAmount)
        {
            _cartDropDownManager?.AddItem(tech, rTech,returnAmount);
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

                //Body
                _panelGroup = GetDisplayBody();

                //Tabs
                GetDisplayTabs(_panelGroup);

                //Create CheckOut
                CreateCheckOutPopup(canvas);

                //Cart
                CreateCartDropDown(_homePage);

                //HomePage
                _accountPageHandler = new AccountPageHandler(_mono);

                //Return Button
                var returnBTN = GameObjectHelpers.FindGameObject(canvas, "CartReturnIcon").GetComponent<Button>();
                returnBTN.onClick.AddListener(() => { OnReturnButtonClicked?.Invoke(); });
                
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

        private void OnBalanceUpdated()
        {
            //TODO Error Happening possible subscrition issue
            if (_mainTotal != null && _mainTotal.isActiveAndEnabled)
            {
                _mainTotal.text = CardSystem.main.GetAccountBalance().ToString("n0");
            }
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

            _cartButtonNumber = GameObjectHelpers.FindGameObject(cartBTNObj, "CartAmount").GetComponentInChildren<Text>();
            var cartBTN = cartBTNObj.GetComponent<Button>();
            cartBTN.onClick.AddListener(() => { _cartDropDownManager.ToggleVisibility(); });
            
            var cartDropDown = GameObjectHelpers.FindGameObject(homePage, "CartDropDown");
            _cartDropDownManager = cartDropDown.AddComponent<CartDropDownHandler>();

            _cartDropDownManager.OnBuyAllBtnClick += OnBuyAllBtnClick;

            _cartDropDownManager.onTotalChanged += OnTotalChanged;

            _cartDropDownManager.Initialize();
        }

        private void OnTotalChanged(decimal obj)
        {
            _cartButtonNumber.text = _cartDropDownManager.GetCartCount().ToString();
        }

        private void OnBuyAllBtnClick(CartDropDownHandler obj)
        {
            _checkoutDialog.ShowDialog(_mono, _cartDropDownManager);
            _cartDropDownManager.ToggleVisibility();
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

        public override void HibernateDisplay()
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
                _cartDropDownManager.AddItem(cartItem.TechType,cartItem.ReceiveTechType, cartItem.ReturnAmount <= 0 ? 1 : cartItem.ReturnAmount);
            }
        }
    }
}