using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Helpers;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Mono;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.uGUI;
using FCSCommon.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Pages;
internal class DSSMoonPoolSettingsPage : DSSPage
{
    [SerializeField] private List<DSSFilterItemButton> _filterItemButtons = new List<DSSFilterItemButton>();
    [SerializeField] private DSSVehicleListDialog vehicleListController;
    [SerializeField] private DSSTerminalController controller;
    [SerializeField] private Toggle pullFromBaseToggle;
    [SerializeField] private DumpContainer dumpContainer;

    public override void Awake()
    {
        base.Awake();
        dumpContainer.OnDumpContainerItemSample += DumpContainer_OnDumpContainerItemSample; ;
    }

    private void DumpContainer_OnDumpContainerItemSample(Pickupable obj)
    {
        QuickLogger.Info($"Test Dump, {obj.GetTechType()}", true);
        Add(obj);
    }

    public override void Enter(object arg = null)
    {
        base.Enter(arg);
        pullFromBaseToggle.SetIsOnWithoutNotify(controller.CachedHabitatManager.PullFromDockedVehicles);
        RefreshBlackListItems();
    }

    public void OnPullFromVehicleChanged(bool v)
    {
        controller.CachedHabitatManager.PullFromDockedVehicles = v;
    }

    internal void RefreshBlackListItems()
    {
        for (int i = 0; i < 7; i++)
        {
            _filterItemButtons[i].Reset();
        }

        var techTypes = controller.CachedHabitatManager.GetDockingBlackList();

        for (int i = 0; i < techTypes.Count; i++)
        {
            _filterItemButtons[i].Set(techTypes.ElementAt(i));
        }
    }

    public void OnBlacklistBTNClicked()
    {
        dumpContainer.OpenStorage();
    }

    private bool Add(Pickupable item)
    {
        if (!controller.CachedHabitatManager.GetDockingBlackList().Contains(item.GetTechType()))
        {
            controller.CachedHabitatManager.AddDockingBlackList(item.GetTechType());
        }

        //PlayerInteractionHelper.GivePlayerItem(item);
        RefreshBlackListItems();

        return true;
    }

}
