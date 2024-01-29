using FCS_AlterraHub.Core.Services;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono;
public class InfoController : MonoBehaviour
{
    [SerializeField]
    private Text _currentBiome;
    [SerializeField]
    private Text _accountName;
    [SerializeField]
    private Text _accountBalance;
    [SerializeField]
    private Text _currentBaseInfo;

    private void Awake()
    {
        InvokeRepeating(nameof(UpdateDisplay), .5f, .5f);
    }


    internal void UpdateDisplay()
    {
        if (_currentBiome == null || _accountName == null || _currentBaseInfo == null) return;

        _currentBiome.text = Player.main.GetBiomeString();

        if (!string.IsNullOrWhiteSpace(AccountService.main.GetUserName()))
            _accountName.text = AccountService.main.GetUserName();

        _accountBalance.text = $"{AccountService.main.GetAccountBalance():N0}";

        var currentBase = HabitatService.main.GetPlayersCurrentBase();
        var friendly = currentBase?.GetBaseFormatedID();
        var baseId = currentBase?.GetBaseID() ?? 0;

        if (!string.IsNullOrWhiteSpace(friendly))
        {
            SetCurrentBaseInfoText($"{friendly} | {LanguageService.BaseIDFormat(baseId.ToString("D3"))}");
        }
        else
        {
            SetCurrentBaseInfoText("N/A");
        }
    }

    private void SetCurrentBaseInfoText(string text)
    {
        _currentBaseInfo.text = $"Current Base : {text}";
    }
}
