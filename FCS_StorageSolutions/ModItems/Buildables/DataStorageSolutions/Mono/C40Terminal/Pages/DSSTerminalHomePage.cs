using FCS_AlterraHub.Core.Components.uGUIComponents;
using FCS_AlterraHub.Core.Helpers;
using FCS_StorageSolutions.Models.Enumerator;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Managers;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Mono;
using FCS_StorageSolutions.ModItems.Buildables.RemoteStorage.Buildable;
using System;
using System.Text;
using UnityEngine;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Pages;
internal class DSSTerminalHomePage : DSSPage
{
    private DSSPageController _pageController;
    [SerializeField]private FCSToolTip fcsToolTip;
    private StringBuilder _sb = new StringBuilder();
    private DSSTerminalController _sender;

    public override void Enter(object arg = null)
    {
        base.Enter(arg);

        _sender = arg as DSSTerminalController;

        if(fcsToolTip is null)
        {
            fcsToolTip = gameObject.GetComponentInChildren<FCSToolTip>();
        }

        fcsToolTip.RequestPermission += RequestPermission;

        fcsToolTip.ToolTipStringDelegate += ToolTipStringDelegate;
    }

    private bool RequestPermission()
    {
        return WorldHelpers.CheckIfPlayerInRange(_sender, 3f);
    }

    public override void Awake()
    {
        base.Awake();
        _pageController = gameObject.GetComponentInParent<DSSPageController>();


    }
    public void PushPage(DSSPage page)
    {
        _pageController.PushPage(page);
    }

    private string ToolTipStringDelegate()
    {
        _sb.Clear();
        _sb.AppendFormat("\n<size=20><color=#FFA500FF>{0}:</color></size>", "Additional Storage Information");
        _sb.AppendFormat("\n<size=20><color=#FFA500FF>{0}:</color> <color=#DDDEDEFF>{1}</color></size>", "Storage Lockers", $"{_sender.GetDSSManager().GetDeviceItemTotal(TechType.Locker) + _sender.GetDSSManager().GetDeviceItemTotal(TechType.SmallLocker)} items.");
        _sb.AppendFormat("\n<size=20><color=#FFA500FF>{0}:</color> <color=#DDDEDEFF>{1}</color></size>", RemoteStorageBuildable.PatchedTechType.AsString(), $"{_sender.GetDSSManager().GetDeviceItemTotal(RemoteStorageBuildable.PatchedTechType)} items.");
        return _sb.ToString();
    }
        
    private void OnDestroy()
    {
        fcsToolTip.ToolTipStringDelegate += ToolTipStringDelegate;
    }
}
