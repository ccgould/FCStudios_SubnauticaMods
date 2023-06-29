using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
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
    private bool _retrieveNameFromDevice = true;
    internal Action<IFCSObject> onSettingsClicked;

    private void Awake()
    {
        InvokeRepeating(nameof(UpdateLabel),1f, 1f);
    }

    private void UpdateLabel()
    {
        if (!_retrieveNameFromDevice || _page?.GetController() is null) return;
        pageLabel.text = Language.main.Get(_page.GetController().GetTechType());
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

    public void RegisterPage(IuGUIAdditionalPage page)
    {
        this._page = page;
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
        _retrieveNameFromDevice = false;
        pageLabel.text = value;
    }
}
