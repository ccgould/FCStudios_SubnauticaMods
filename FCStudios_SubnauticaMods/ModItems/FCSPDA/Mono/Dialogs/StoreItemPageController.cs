﻿using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.ScreenItems;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

internal class StoreItemPageController : Page
{
    [SerializeField]
    private FCSAlterraHubGUI _gui;
    protected override bool showHud => false;

    public override PDAPages PageType => PDAPages.StorePage;

    [SerializeField]
    private GameObject _storePageGrid;
    private Dictionary<StoreCategory, List<StoreItem>> _storeItems = new();
    [SerializeField]
    private Text _storeLabel;
    [SerializeField]
    private Text _cartAmountLabel;
    [SerializeField]
    private Text _cartTotalLabel;

    private void Awake()
    {
        LoadStore();
    }     

    private void UpdateCartValues()
    {
        QuickLogger.Debug($"{_cartAmountLabel is null} || {StoreManager.main is null} || {CartDropDownHandler.main is null}",true);
        QuickLogger.Debug("1");
        _cartAmountLabel.text = $"Cart Amount: {StoreManager.main.GetCartTotal(CartDropDownHandler.main.GetShipmentInfo())}";
        
        QuickLogger.Debug("2");
        _cartTotalLabel.text = $"Cart Total: {StoreManager.main.GetCartCount(CartDropDownHandler.main.GetShipmentInfo()):n0}";
    }

    public override void Enter(object arg)
    {
        base.Enter(arg);
        LoadStorePage((PDAPages)arg);
        UpdateCartValues();
    }

    private void LoadStore()
    {

        foreach (StoreCategory category in Enum.GetValues(typeof(StoreCategory)))
        {
            foreach (var storeItem in FCSModsAPI.PublicAPI.GetRegisteredKits())
            {
                if (storeItem.Value.StoreCategory != category) continue;
                QuickLogger.Debug($"Trying to add Store Item  {Language.main.Get(storeItem.Key)}");

                var item = StoreInventoryService.CreateStoreItem(storeItem.Value, AddToCartCallBack, IsInUse);

                if (_storeItems.ContainsKey(category))
                {
                    _storeItems[category].Add(item);
                }
                else
                {
                    _storeItems.Add(category, new List<StoreItem> { item });
                }

                item.gameObject.transform.SetParent(_storePageGrid.transform, false);

                QuickLogger.Debug($"Added Store Item  {Language.main.Get(storeItem.Key)} with category to Panel: {storeItem.Value.StoreCategory}:");
            }

            //foreach (FCSStoreEntry storeItem in Main.Configuration.AdditionalStoreItems)
            //{
            //    if (storeItem.StoreCategory != category) continue;

            //    QuickLogger.Debug($"Trying to add Store Item  {Language.main.Get(storeItem.TechType)}");

            //    var item = StoreInventorySystem.CreateStoreItem(storeItem, AddToCartCallBack, IsInUse);
            //    if (_storeItems.ContainsKey(category))
            //    {
            //        _storeItems[category].Add(item);
            //    }
            //    else
            //    {
            //        _storeItems.Add(category, new List<StoreItem> { item });
            //    }

            //    item.gameObject.transform.SetParent(_storePageGrid.transform, false);

            //    QuickLogger.Debug($"Added Store Item  {Language.main.Get(storeItem.TechType)} with category to Panel: {storeItem.StoreCategory}:");
            //}
        }
    }

    private void LoadStorePage(PDAPages pages)
    {
        StoreCategory category = StoreCategory.None;

        switch (pages)
        {
            case PDAPages.HomeSolutions:
                category = StoreCategory.Home;
                _storeLabel.text = "Home Solutions";
                break;
            case PDAPages.LifeSolutions:
                category = StoreCategory.LifeSupport;
                _storeLabel.text = "Life Solutions";
                break;
            case PDAPages.EnergySolutions:
                category = StoreCategory.Energy;
                _storeLabel.text = "Energy Solutions";
                break;
            case PDAPages.ProductionSolutions:
                category = StoreCategory.Production;
                _storeLabel.text = "Production Solutions";
                break;
            case PDAPages.StorageSolutions:
                category = StoreCategory.Storage;
                _storeLabel.text = "Storage Solutions";
                break;
            case PDAPages.VehicleSolutions:
                category = StoreCategory.Vehicles;
                _storeLabel.text = "Vehicle Solutions";
                break;
            case PDAPages.MiscSolutions:
                category = StoreCategory.Misc;
                _storeLabel.text = "Misc";
                break;
            case PDAPages.AlterraHub:
                category = StoreCategory.AlterraHub;
                _storeLabel.text = "Alterra Hub";
                break;
        }
        foreach (var storeItem in _storeItems)
        {
            if (storeItem.Key == category)
            {
                foreach (StoreItem item in storeItem.Value)
                {
                    item.Show();
                }
            }
            else
            {
                foreach (StoreItem item in storeItem.Value)
                {
                    item.Hide();
                }
            }
        }
    }

    private bool IsInUse()
    {
        return _gui.IsOpen;
    }

    private void AddToCartCallBack(TechType techType, TechType receiveTechType, int returnAmount)
    {
        CartDropDownHandler.main.AddItem(techType, receiveTechType, returnAmount);
        UpdateCartValues();
    }

    public override void OnBackButtonClicked()
    {
        _gui.GoBackAPage();
    }
}