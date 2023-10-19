using System;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Core.Components.uGUIComponents;
public class uGUI_StorageItem : MonoBehaviour
{
    [SerializeField] private uGUI_Icon _icon;
    [SerializeField] private Text _amount;
    [SerializeField] FCSToolTip _tooltip;
    public Action<TechType> onItemClicked;

    private TechType techType;
    private string textLineOne;
    private Button _button;

    private void Awake()
    {

    }

    public void Set(TechType techType, int amount)
    {
        this.techType = techType;
        _amount.text = amount.ToString();
        _icon.sprite = SpriteManager.Get(techType);
        textLineOne = Language.main.Get(techType);
        gameObject.SetActive(true);
    }

    public void Reset()
    {
        techType = TechType.None;
        textLineOne = string.Empty;
        _amount.text = string.Empty;
        _icon.sprite = SpriteManager.Get(TechType.None);
        gameObject.SetActive(false);
    }

    public void OnClick()
    {
        onItemClicked?.Invoke(techType);
    }

    public TechType GetTechType()
    {
        return techType;
    }
}
