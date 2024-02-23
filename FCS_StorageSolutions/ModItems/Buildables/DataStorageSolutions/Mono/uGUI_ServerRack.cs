using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UWE;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono;

internal class uGUI_ServerRack : Page, IuGUIAdditionalPage
{
    [SerializeField] private Text _inventoryCountLbl;
    [SerializeField] private Text _deviceNameLbl;
    [SerializeField] private Transform grid;
    [SerializeField] private Transform slotTemplate;
    [SerializeField] private Image _inventoryCountPercentage;
    //Trying to set the slots by getting the slot from the controller
    private RackBase _controller;
    private List<RackSlotData> _rackSlots;
    private MenuController _menuController;

    public event Action<PDAPages> onBackClicked;
    public event Action<FCSDevice> onSettingsClicked;
    [SerializeField] private List<uGUI_RackGaugeController> rackLargeGauges;
    [SerializeField] private List<uGUI_RackGaugeController> rackGaugesSmallConfig;


    // the problem i am facing is that I need to tell this what slots to use and match the ui with those slot id



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

            _rackSlots = _controller.GetSlots();

            _deviceNameLbl.text = _controller.UnitID;

            GenerateSlots();

            var storage = _controller.GetStorage().container;
            storage.onRemoveItem += Storage_onRemoveItem;


            //Set data before screen is shown.
            RefreshDevice();
        }

        Show();

        FCSPDAController.Main.GetGUI().OnStorageButtonClicked += AlterraHubGUIOnStorageButtonClicked;
    }

    private void Storage_onRemoveItem(InventoryItem item)
    {
        RefreshDevice();
    }

    private void GenerateSlots()
    {
        QuickLogger.Debug($"Generate Slots: Count:{_rackSlots}");

        if (_rackSlots.Count > 6)
        {
            for (int i = 0; i < _rackSlots.Count; i++)
            {
                var gauge = rackLargeGauges[i];
                gauge.gameObject.SetActive(true);
                gauge.SetSlot(_rackSlots[i], _controller, i);
            }
        }
        else
        {
            for (int i = 0; i < _rackSlots.Count; i++)
            {
                var gauge = rackGaugesSmallConfig[i];
                gauge.gameObject.SetActive(true);
                gauge.SetSlot(_rackSlots[i], _controller, i);
            }

        }
    }

    private void ClearSlots()
    {
        foreach (var gauge in rackLargeGauges)
        {
            gauge.Eject();
            gauge.gameObject.SetActive(false);
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
