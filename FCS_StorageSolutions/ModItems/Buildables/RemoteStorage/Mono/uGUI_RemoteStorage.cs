using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UWE;

namespace FCS_StorageSolutions.ModItems.Buildables.RemoteStorage.Mono;

internal class uGUI_RemoteStorage : Page, IuGUIAdditionalPage
{
    [SerializeField] private List<uGUI_RemoteStorageItem> _inventoryButtons = new();
    [SerializeField] private Text _inventoryCountLbl;
    [SerializeField] private Text _deviceNameLbl;
    [SerializeField] private GridHelper _inventoryGrid;
    [SerializeField] private PaginatorController _paginatorController;


    private const int MAXSTORAGE = 200;

    private RemoteStorageController _controller;
    private MenuController _menuController;

    public event Action<PDAPages> onBackClicked;
    public event Action<FCSDevice> onSettingsClicked;

    public void Initialize(object obj)
    {

        _controller = (RemoteStorageController)obj;

        //Set data before screen is shown.
        RefreshDevice();

        Show();

        InvokeRepeating(nameof(RefreshDeviceName), 1f, 1f);
    }

    public override void Awake()
    {
        base.Awake();
        _paginatorController.OnPageChanged += PaginatorController_OnPageChanged;
        _inventoryGrid.OnLoadDisplay += GridHelper_OnLoadDisplay;
        RefreshDevice();

    }

    private void RefreshDevice()
    {
        if (_controller is null) return;
        _inventoryCountLbl.text = LanguageService.StorageCountFormat(_controller.GetStorage().GetCount(), MAXSTORAGE);
        _inventoryGrid.DrawPage();
    }

    private void OnDestroy()
    {
        _paginatorController.OnPageChanged -= PaginatorController_OnPageChanged;
        _inventoryGrid.OnLoadDisplay -= GridHelper_OnLoadDisplay;
    }

    private void PaginatorController_OnPageChanged(object sender, PaginatorController.OnPageChangedArgs e)
    {
        _inventoryGrid.DrawPage(e.PageIndex);
    }

    private void GridHelper_OnLoadDisplay(DisplayData data)
    {
        try
        {
            if (_controller is null) return;

            var grouped = _controller.GetStorage().container._items;

            if (grouped == null) return;

            if (data.EndPosition > grouped.Count)
            {
                data.EndPosition = grouped.Count;
            }

            for (int i = 0; i < data.MaxPerPage; i++)
            {
                _inventoryButtons[i].Reset();
            }

            int w = 0;

            for (int i = data.StartPosition; i < data.EndPosition; i++)
            {
                _inventoryButtons[w++].Set(grouped.ElementAt(i).Key, grouped.ElementAt(i).Value.items.Count);
            }

            _inventoryGrid.UpdaterPaginator(grouped.Count);
            _paginatorController.ResetCount(_inventoryGrid.GetMaxPages());
        }
        catch (Exception e)
        {
            QuickLogger.Error("Error Caught");
            QuickLogger.Error($"Error Message: {e.Message}");
            QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
        }
    }

    public void OnInventoryButtonClicked(uGUI_RemoteStorageItem uGUI_RemoteStorageItem)
    {
        var techType = uGUI_RemoteStorageItem.GetTechType();
        var size = TechDataHelpers.GetItemSize(techType);
        if (Inventory.main.HasRoomFor(size.x, size.y))
        {
            PlayerInteractionHelper.GivePlayerItem(_controller.GetStorage().RemoveItemFromContainer(techType));
            RefreshDevice();
        }
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

    public void RefreshDeviceName()
    {
        if (_controller is null) return;
        _deviceNameLbl.text = _controller.GetDeviceName();
    }

    public override void Exit()
    {
        base.Exit();
        _controller = null;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public IFCSObject GetController() => _controller;

    public void SetMenuController(MenuController menuController)
    {
        _menuController = menuController;
    }
}
