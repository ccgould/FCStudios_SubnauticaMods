using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using System;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono;
internal class uGUI_BaseManager : Page, IuGUIAdditionalPage
{
    public override PDAPages PageType => PDAPages.DevicePage;

    public event EventHandler OnMenuButtonAction;

    public event Action<PDAPages> onBackClicked;
    public event Action<FCSDevice> onSettingsClicked;

    [SerializeField]
    private BaseManagerHomePage homePage;

    public override void Enter(object arg = null)
    {
        base.Enter(arg);
        homePage.Enter();
    }

    public void Hide()
    {

    }

    public void Initialize(object obj)
    {

    }

    public override void OnBackButtonClicked()
    {

    }

    public void OnMenuButtonClicked()
    {
        OnMenuButtonAction?.Invoke(this, EventArgs.Empty);
    }
}
