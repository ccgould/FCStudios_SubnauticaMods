using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono.GUI;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCSCommon.Utilities;
using System;
using UnityEngine;
using Valve.VR;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono;

internal class uGUI_BaseManager : Page, IuGUIAdditionalPage
{
    public static uGUI_BaseManager Instance;

    public event EventHandler OnUGUIBaseManagerOpened;


    [SerializeField]
    private BaseManagerSideMenu baseManagerSideMenu;

    [SerializeField]
    private Page initialPage;

    private IFCSObject _fcsObject;
    private MenuController _menuController;

    public TechType patchedTechType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    private void Start()
    {
        _menuController = FCSPDAController.Main.GetGUI().GetMenuController();
    }

    public override void Enter(object arg = null)
    {
        QuickLogger.Debug($"BaseManager Enter Called {arg is IFCSObject}",true);
        QuickLogger.Debug($"BaseManager Enter Called {arg?.GetType()}", true);
        base.Enter(arg);

        if(arg is not null )
        {
            _fcsObject = arg as IFCSObject;
        }

        initialPage?.Enter(this);
        FCSPDAController.Main.ui.SetPDAAdditionalLabel(Language.main.Get(_fcsObject.GetTechType()));
        OnUGUIBaseManagerOpened?.Invoke(this, EventArgs.Empty);
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

    public IFCSObject GetController() => _fcsObject;

    public void PushPage(Page page)
    {
        _menuController.PushPage(page);
    }

    public void PopPage()
    {
        _menuController.PopAndPeek();
    }

}
