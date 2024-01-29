using FCS_StorageSolutions.Models;
using FCS_StorageSolutions.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Mono;
internal class uGUI_TerminalBaseListItem : MonoBehaviour
{
    [SerializeField] private Text nameLbl;
    [SerializeField] private GameObject[] icons;
    private DSSManager dssManager;
    private DSSTerminalController terminalController;

    internal void Reset()
    {
        ResetIcons();
        nameLbl.text = string.Empty;
        gameObject.SetActive(false);
        dssManager = null;
    }

    internal void Set(KeyValuePair<string, DSSManager> dssManager, DSSTerminalController terminalController)
    {

        this.terminalController = terminalController;
        this.dssManager = dssManager.Value;
        nameLbl.text = string.IsNullOrEmpty(dssManager.Value.GetBaseFriendlyName()) ? dssManager.Value.GetBaseFormattedID() : dssManager.Value.GetBaseFriendlyName();
        SetIcon();

        gameObject.SetActive((this.dssManager.IsConnectedToNetwork() && terminalController.GetDSSManager().IsConnectedToNetwork()) || IsCurrentSub());
    }

    public void OnClick()
    {
        terminalController.ChangeFocusBase(dssManager);
    }

    private void SetIcon()
    {
        ResetIcons();

        if (dssManager.GetSubRoot().isCyclops)
        {
            icons[3].gameObject.SetActive(true);
        }
        else if (IsCurrentSub())
        {
            icons[0].gameObject.SetActive(true);
        }
        else if (dssManager.GetSubRoot().isBase)
        {
            icons[1].gameObject.SetActive(true);
        }
    }

    private bool IsCurrentSub()
    {
        return dssManager.GetSubRoot() == terminalController.CachedHabitatManager.GetSubRoot();
    }

    private void ResetIcons()
    {
        foreach (var icon in icons)
        {
            icon.gameObject.SetActive(false);
        }
    }
}
