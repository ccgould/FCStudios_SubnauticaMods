﻿using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
using FCSCommon.Utilities;
using rail;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UWE;

namespace FCS_StorageSolutions.ModItems.Buildables.RemoteStorage.Mono;

internal class uGUI_ServerRack : Page, IuGUIAdditionalPage
{
    //[SerializeField] private List<uGUI_RemoteStorageItem> _inventoryButtons = new();
    [SerializeField] private Text _inventoryCountLbl;
    [SerializeField] private Text _deviceNameLbl;
    [SerializeField] private Transform grid;
    [SerializeField] private Transform slotTemplate;
    [SerializeField] private Image _inventoryCountPercentage;
    //[SerializeField] private GridHelper _inventoryGrid;
    //[SerializeField] private PaginatorController _paginatorController;


    private RackBase _controller;
    private int _rackSlots;
    private MenuController _menuController;

    public event Action<PDAPages> onBackClicked;
    public event Action<FCSDevice> onSettingsClicked;
    public List<RackGaugeController> rackGauges = new();


    

    public override void Awake()
    {
        base.Awake();
        RefreshDevice();
    }

    private void Start()
    {
        _menuController = FCSPDAController.Main.GetGUI().GetMenuController();
    }    


    private void RefreshDevice()
    {
        if (_controller is null) return;

        _inventoryCountLbl.text = LanguageService.StorageCountFormat(CalculateStorageTotal(), CalculateMaxStorage());
        float percentage = (float)CalculateStorageTotal() / CalculateMaxStorage();

        _inventoryCountPercentage.fillAmount = percentage;
    }

    private int CalculateStorageTotal()
    {
        int total = 0;
        foreach (var item in _controller.GetStorage().container)
        {
            var server = item.item.gameObject.GetComponentInChildren<DSSServerController>();

            if (server is not null)
            {
                total += server.GetStorageTotal();
            }
        }

        return total;
    }

    private int CalculateMaxStorage()
    {
        int maxStorage = 0;
        foreach (var item in _controller.GetStorage().container)
        {
            var server = item.item.gameObject.GetComponentInChildren<DSSServerController>();

            if (server is not null)
            {
                maxStorage += server.GetMaxStorage();
            }
        }

        return maxStorage;
    }

    private void OnDestroy()
    {

    }


    public void OpenDumpContainer()
    {
        CoroutineHost.StartCoroutine(OpenStorage());
    }

    public IEnumerator OpenStorage()
    {
        QuickLogger.Debug($"Storage Button Clicked", true);

        //Close FCSPDA so in game pda can open with storage
        FCSPDAController.Main.Close();

        QuickLogger.Debug($"Closing FCS PDA", true);

        QuickLogger.Debug("Attempting to open the In Game PDA", true);
        Player main = Player.main;
        PDA pda = main.GetPDA();

        while (pda != null && pda.isInUse || pda.isOpen)
        {
            QuickLogger.Debug("Waiting for In Game PDA Settings to reset", true);
            yield return null;
        }

        QuickLogger.Debug("Gettings Reset", true);
        _controller.OpenDumpContainer();

        yield break;
    }

    public override void Enter(object arg = null)
    {
        base.Enter(arg);
        QuickLogger.Debug("Server Rack Enter", true);
        
        if (arg is not null)
        {
            _controller = (RackBase)arg;

            _rackSlots = _controller.GetMaxStorage();

            _deviceNameLbl.text = _controller.UnitID;

            GenerateSlots();

            InjectServers();

            //Set data before screen is shown.
            RefreshDevice();
        }
              
        Show();

        FCSPDAController.Main.GetGUI().OnStorageButtonClicked += AlterraHubGUIOnStorageButtonClicked;
    }

    private void InjectServers()
    {
        if(_controller is not null)
        {
            var storage = _controller.GetStorage().container;

            storage.onRemoveItem += Storage_onRemoveItem;

            for (int i = 0; i < storage.count; i++)
            {
                var server = storage.ElementAt(i);
                rackGauges[i].SetServer(server);
            }
        }
    }

    private void Storage_onRemoveItem(InventoryItem item)
    {
        RefreshDevice();
    }

    private void GenerateSlots()
    {
        QuickLogger.Debug($"Generate Slots: Count:{_rackSlots}");

        for (int i = 0; i < _rackSlots; i++)
        {
            QuickLogger.Debug($"Generating slot {i}");
            var instance = Instantiate(slotTemplate, grid, false);
            var controller = instance.GetComponent<RackGaugeController>();
            controller.Initalize(i,_controller);
            rackGauges.Add(controller);
            instance.gameObject.SetActive(true);
            QuickLogger.Debug($"Done Generating slot {i}");
        }
    }

    public override void Exit()
    {
        base.Exit();

        var storage = _controller.GetStorage().container;

        storage.onRemoveItem -= Storage_onRemoveItem;

        FCSPDAController.Main.GetGUI().OnStorageButtonClicked -= AlterraHubGUIOnStorageButtonClicked;
        ClearSlots();
    }

    private void ClearSlots()
    {
        foreach (var slot in rackGauges)
        {
            Destroy(slot.gameObject);
        }

        rackGauges.Clear();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public IFCSObject GetController() => _controller;

    private void AlterraHubGUIOnStorageButtonClicked()
    {
        OpenDumpContainer();
    }
}