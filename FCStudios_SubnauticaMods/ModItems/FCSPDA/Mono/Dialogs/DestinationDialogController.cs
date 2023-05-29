﻿using System;
using System.Collections.Generic;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs;

internal class DestinationDialogController : Page
{
    private ToggleGroup _toggleGroup;
    private Transform _list;
    private readonly List<AlterraHubDepotItemController> _toggles = new List<AlterraHubDepotItemController>();
    private CheckOutPopupDialogWindow _checkoutWindow;
    public Action OnClose { get; set; }

    public override PDAPages PageType => PDAPages.None;

    internal  void Initialize(CheckOutPopupDialogWindow checkoutWindow)
    {
        _checkoutWindow = checkoutWindow;
        var cancelBTN = gameObject.FindChild("Content").FindChild("GameObject").FindChild("CancelBTN")
            .GetComponent<Button>();
        cancelBTN.onClick.AddListener((() => { Close(true); }));

        var doneBTN = gameObject.FindChild("Content").FindChild("GameObject").FindChild("DoneBTN")
            .GetComponent<Button>();
        doneBTN.onClick.AddListener((() =>
        {
            foreach (AlterraHubDepotItemController toggle in _toggles)
            {
                if (toggle.IsChecked)
                {
                    //_checkoutWindow.SelectedDestination = toggle.Destination;
                    break;
                }
            }
            Close();
        }));

        _toggleGroup = gameObject.GetComponentInChildren<ToggleGroup>();
        _list = _toggleGroup.gameObject.transform;
    }

    private void RefreshAlterraHubDepotList()
    {
        for (var i = _toggles.Count - 1; i >= 0; i--)
        {
            AlterraHubDepotItemController item = _toggles[i];
            item.UnRegisterAndDestroy();
            _toggles.Remove(item);
        }

       // var portManagers = FindObjectsOfType<PortManager>();


        //foreach (var portManager in portManagers)
        //{
        //    if(!portManager.HasAccessPoint()) continue;
        //    var depotPrefab = GameObject.Instantiate(Buildables.AlterraHub.AlterraHubDepotItemPrefab);
        //    var controller = depotPrefab.AddComponent<AlterraHubDepotItemController>();
        //    if (controller.Initialize(portManager, _toggleGroup, _list))
        //    {
        //        _toggles.Add(controller);
        //    }
        //    else
        //    {
        //        Destroy(depotPrefab);
        //    }
        //}
    }
    
    private void Close(bool isCancel = false)
    {
        if (isCancel)
        {
            //_checkoutWindow.SelectedDestination = null;
        }
        OnClose?.Invoke();
        gameObject.SetActive(false);
    }

    internal void Open()
    {
        RefreshAlterraHubDepotList();
        gameObject.SetActive(true);
    }

    public override void OnBackButtonClicked()
    {
       FCSPDAController.Main.GetGUI().GoBackAPage();
    }
}