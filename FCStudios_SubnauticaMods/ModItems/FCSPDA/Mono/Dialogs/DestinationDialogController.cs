using System;
using System.Collections.Generic;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs;

internal class DestinationDialogController : Page
{
    //private ToggleGroup _toggleGroup;
    //private Transform _list;
    [SerializeField] private GameObject alterraHubDepotItemPrefab;
    private readonly List<AlterraHubDepotItemController> _toggles = new List<AlterraHubDepotItemController>();
    [SerializeField] private CheckOutPopupDialogWindow _checkoutWindow;
    public Action OnClose { get; set; }

    internal  void Initialize()
    {
        //var cancelBTN = gameObject.FindChild("Content").FindChild("GameObject").FindChild("CancelBTN")
        //    .GetComponent<Button>();
        //cancelBTN.onClick.AddListener((() => { Close(true); }));

        //var doneBTN = gameObject.FindChild("Content").FindChild("GameObject").FindChild("DoneBTN")
        //    .GetComponent<Button>();



        //doneBTN.onClick.AddListener((() =>
        //{
        //    foreach (AlterraHubDepotItemController toggle in _toggles)
        //    {
        //        if (toggle.IsChecked)
        //        {
        //            //_checkoutWindow.SelectedDestination = toggle.Destination;
        //            break;
        //        }
        //    }
        //    Close();
        //}));

        //_toggleGroup = gameObject.GetComponentInChildren<ToggleGroup>();
        //_list = _toggleGroup.gameObject.transform;
    }

    public void OnDoneClick()
    {
        foreach (AlterraHubDepotItemController toggle in _toggles)
        {
            if (toggle.IsChecked)
            {
                _checkoutWindow.SelectedDestination = toggle.Destination;
                break;
            }
        }
        
        Close();
    }

    private void RefreshAlterraHubDepotList()
    {
        for (var i = _toggles.Count - 1; i >= 0; i--)
        {
            AlterraHubDepotItemController item = _toggles[i];
            item.UnRegisterAndDestroy();
            _toggles.Remove(item);
        }

        var portManagers = FindObjectsOfType<PortManager>();


        foreach (var portManager in portManagers)
        {
            if (!portManager.HasAccessPoint()) continue;
            var depotPrefab = GameObject.Instantiate(alterraHubDepotItemPrefab);
            var controller = depotPrefab.GetComponent<AlterraHubDepotItemController>();
            if (controller.Initialize(portManager))
            {
                _toggles.Add(controller);
            }
            else
            {
                Destroy(depotPrefab);
            }
        }
    }
    
    public void OnCancelBtnClicked()
    {
        _checkoutWindow.SelectedDestination = null;
        Close();
    }

    private void Close()
    {
        OnClose?.Invoke();
    }

    internal void Open()
    {
        RefreshAlterraHubDepotList();
    }
}