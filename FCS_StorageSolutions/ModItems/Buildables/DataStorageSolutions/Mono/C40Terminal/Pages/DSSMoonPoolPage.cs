using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Managers;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
using System;
using UnityEngine;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Pages;
internal class DSSMoonPoolPage : DSSPage
{
    private DSSPageController _pageController;
    [SerializeField] private DSSVehicleSectionPage _vehicleSelecitonPage;

    public Action<string> IPCMessage { get; internal set; }

    public override void Awake()
    {
        base.Awake();
        _pageController = gameObject.GetComponentInParent<DSSPageController>();
    }
    public void PushPage(DSSPage page)
    {
        _pageController.PushPage(page);
    }

    internal void ShowVehicleContainers(Vehicle vehicle)
    {
        _pageController.PushPage(_vehicleSelecitonPage,vehicle);
    }
}
