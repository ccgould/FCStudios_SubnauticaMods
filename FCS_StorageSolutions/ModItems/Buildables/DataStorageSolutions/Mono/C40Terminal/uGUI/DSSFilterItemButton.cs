using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Mono;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Pages;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.uGUI;
internal class DSSFilterItemButton : MonoBehaviour
{
    [SerializeField] private DSSTerminalController controller;
    [SerializeField] private Text text;
    [SerializeField] private DSSMoonPoolSettingsPage moonPoolSettingsPage;

    private TechType _techType;

    internal void Set(TechType techType)
    {
        _techType = techType;
        text.text = Language.main.Get(techType);
        gameObject.SetActive(true);
    }

    public void OnDeleteBTNClick()
    {
        controller.CachedHabitatManager.RemoveDockingBlackList(_techType);
        Reset();
    }

    internal void Reset()
    {
        gameObject.SetActive(false);
        text.text = string.Empty;
        //moonPoolSettingsPage.RefreshBlackListItems();
    }
}
