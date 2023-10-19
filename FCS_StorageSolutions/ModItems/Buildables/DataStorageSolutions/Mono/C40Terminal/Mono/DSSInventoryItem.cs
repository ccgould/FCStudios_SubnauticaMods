using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Mono.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Mono;
internal class uGUI_DSSInventoryItem : MonoBehaviour
{
    [SerializeField] private uGUI_Icon _icon;
    [SerializeField] private Text _amount;
    [SerializeField] private FCSToolTip _tooltip;
    [SerializeField] private TechType techType;

    private void Initialize()
    {
        if (_tooltip == null)
        {
            _tooltip = gameObject.AddComponent<FCSToolTip>();
            _tooltip.Description = true;
        }

        if (_icon == null)
        {
            _icon = gameObject.FindChild("Icon").EnsureComponent<uGUI_Icon>();
        }

        if (_amount == null)
        {
            _amount = gameObject.FindChild("Text").EnsureComponent<Text>();
        }
    }

    internal void Set(TechType techType, int amount)
    {
        Initialize();
        this.techType = techType;
        _tooltip.TechType = techType;
        _tooltip.RequestPermission += () => WorldHelpers.CheckIfInRange(gameObject, Player.main.gameObject, 1f);
        _amount.text = amount.ToString();
        _icon.sprite = SpriteManager.Get(techType);
        Show();
    }

    internal void Reset()
    {
        Initialize();
        _amount.text = "";
        _icon.sprite = SpriteManager.Get(TechType.None);
        techType = TechType.None;
        Hide();
    }

    internal void Hide()
    {
        gameObject.SetActive(false);
    }

    internal void Show()
    {
        gameObject.SetActive(true);
    }
}
