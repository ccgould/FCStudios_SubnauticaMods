using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCSCommon.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents;
public class uGUI_PDANavigationController : MonoBehaviour
{
    [SerializeField] private Text pageLabel;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject settingButton;
    [SerializeField] private GameObject infoButton;
    [SerializeField] private GameObject storageButton;
    private IuGUIAdditionalPage _page;
    private Page _additionalPageAsPage;
    private FCSDevice _IFCSObjectAsFCSDevice;
    internal Action<IFCSObject> onSettingsClicked;
    private string _customLabel;

    private void Awake()
    {
        InvokeRepeating(nameof(UpdateLabel),1f, 1f);
    }

    private void UpdateLabel()
    {
        if (_page?.GetController() is null || Time.time == 0 || !isActiveAndEnabled) return;

        switch (_additionalPageAsPage.NavigationLabelState)
        {
            case NavigationLabelState.None:
                pageLabel.text = string.Empty;
                break;
            case NavigationLabelState.DeviceName:
                pageLabel.text = Language.main.Get(_page.GetController().GetTechType());
                break;
            case NavigationLabelState.FriendlyName:
                if(_IFCSObjectAsFCSDevice is not null)
                {
                    pageLabel.text = Language.main.Get(_IFCSObjectAsFCSDevice.FriendlyName);
                }
                else
                {
                    pageLabel.text = Language.main.Get(_page.GetController().GetTechType());
                }

                break;
            case NavigationLabelState.Custom:
                pageLabel.text = _customLabel;
                break;
        }
    }

    public void OnBackButtonClicked()
    {
        FCSPDAController.Main.GetGUI().GoBackAPage();
    }

    public void OnSettingsButtonClicked()
    {
        onSettingsClicked?.Invoke(_page.GetController());
    }

    public void OnInfoButtonClicked()
    {
        FCSPDAController.Main.GetGUI().OnInfoButtonClicked?.Invoke(_page.GetController().GetTechType());
    }

    public void OnStorageButtonClicked()
    {
        QuickLogger.Debug("PDA Navigation on Storage Button Clicked" ,true);
        FCSPDAController.Main.GetGUI().OnStorageButtonClicked?.Invoke();
    }

    public void RegisterPage(IuGUIAdditionalPage page)
    {
        this._page = page;
        this._additionalPageAsPage = page as Page;
        this._IFCSObjectAsFCSDevice = page.GetController() as FCSDevice;
    }

    public void SetNavigationButtons(Page currentPage)
    {
        this.backButton.SetActive(currentPage.ShowBackButton());
        this.storageButton.SetActive(currentPage.ShowStorageButton());
        this.settingButton.SetActive(currentPage.ShowSettingsButton());
        this.infoButton.SetActive(currentPage.ShowInfoButton());
        this.pageLabel.gameObject.SetActive(currentPage.ShowLabel());
    }

    internal void SetLabel(string value)
    {
        _customLabel = value; 
    }

    public enum NavigationLabelState
    {
        None = 0,
        DeviceName = 1,
        FriendlyName = 2,
        Custom = 3
    }
}
