
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Managers;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Pages;
internal class DSSTerminalConfigurationPage : DSSPage
{
    private DSSPageController _pageController;

    public override void Awake()
    {
        base.Awake();
        _pageController = gameObject.GetComponentInParent<DSSPageController>();
    }
    public void PushPage(DSSPage page)
    {
        _pageController.PushPage(page);
    }
}
