using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono.GUI;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono.GUI.Pages;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using System;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono;
internal class uGUI_BaseManager : Page, IuGUIAdditionalPage
{
    public static uGUI_BaseManager Instance;
    public override PDAPages PageType => PDAPages.DevicePage;

    public event EventHandler OnUGUIBaseManagerOpened;

    public event Action<PDAPages> onBackClicked;

    public event Action<FCSDevice> onSettingsClicked;

    [SerializeField]
    private BaseManagerSideMenu baseManagerSideMenu;

    private void Awake()
    {
        Instance = this;
    }

    public override void Enter(object arg = null)
    {
        base.Enter(arg);
        OnUGUIBaseManagerOpened?.Invoke(this, EventArgs.Empty);
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
        if (baseManagerSideMenu.IsOpen())
        {
            baseManagerSideMenu.Hide();
        }
        else
        {
            baseManagerSideMenu.Show();
        }
    }
}
