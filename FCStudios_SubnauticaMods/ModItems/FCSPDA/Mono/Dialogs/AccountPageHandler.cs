using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.Spawnables.DebitCard.Spawnable;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

internal class AccountPageHandler : Page
{
    [SerializeField]
    private Text _userNameLBL;
    [SerializeField]
    private CreateAccountDialogPage _createAccountDialog;
    [SerializeField]
    private Text _requestButtonText;
    [SerializeField]
    private FCSAlterraHubGUI _mono;
    [SerializeField]
    private Text _sliderText;
    [SerializeField]
    private Slider _slider;
    [SerializeField]
    private Toggle _deductionToggle;

    public override void Awake()
    {              
        base.Awake();

        CreateWelcomePage();
    }    

    public void OnDeductionToggleChanged(bool value)
    {
        GamePlayService.Main.SetAutomaticDebitDeduction(value);
    }

    public void OnSliderValueChanged(float value)
    {
        var rate = value * 10f;
        _sliderText.text = $"{Mathf.CeilToInt(rate)}%";
        GamePlayService.Main.SetRate(rate);
    }

    private void CreateWelcomePage()
    {
        _userNameLBL.text = AccountService.main.GetUserName();
        UpdateRequestBTN(AccountService.main.HasBeenRegistered());
    }

    public void OnRequestCardButtonClicked()
    {
        if (AccountService.main.HasBeenRegistered())
        {
            //Check if player has any FiberMesh and Magniette
            if (!PlayerHasIngredients())
            {
                _mono.ShowMessage(string.Format(LanguageService.CardRequirementsMessageFormat(),
                    LanguageService.GetLanguage(TechType.FiberMesh), LanguageService.GetLanguage(TechType.Magnetite)));
                return;
            }

            PlayerInteractionHelper.GivePlayerItem(DebitCardSpawnable.PatchedTechType);
            RemoveItemsFromContainer();
        }
        else
        {
            _createAccountDialog.Show();
        }
    }

    internal void UpdateRequestBTN(bool accountReg)
    {
        _requestButtonText.text = accountReg ? LanguageService.RequestNewCard() : LanguageService.CreateNewAccount();
    }

    private static void RemoveItemsFromContainer()
    {
        var fiberMesh = Inventory.main.container.RemoveItem(TechType.FiberMesh);
        Object.Destroy(fiberMesh.gameObject);
        var magnetite = Inventory.main.container.RemoveItem(TechType.Magnetite);
        Object.Destroy(magnetite.gameObject);
    }

    private bool PlayerHasIngredients()
    {
        var container = Inventory.main.container;
        return container.Contains(TechType.FiberMesh) && container.Contains(TechType.Magnetite);
    }

    internal void Refresh()
    {
        _slider.SetValueWithoutNotify(GamePlayService.Main.GetRate() / 10f);
        _sliderText.text = $"{Mathf.CeilToInt(GamePlayService.Main.GetRate())}%";
        _deductionToggle.SetIsOnWithoutNotify(GamePlayService.Main.GetAutomaticDebitDeduction());
    }

    public override void Enter(object arg = null)
    {
        base.Enter();
        UpdateRequestBTN(AccountService.main.HasBeenRegistered());
    }
    public void Close()
    {

    }
}