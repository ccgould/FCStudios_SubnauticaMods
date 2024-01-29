using FCS_AlterraHub.Core.Helpers;
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
        _tooltip.RequestPermission += OnRequestPermission;
    }

    private bool OnRequestPermission()
    {
        return WorldHelpers.CheckIfPlayerInRange(gameObject, 3f);
    }

    public void Set(TechType techType, int amount = 0)
    {
        gameObject.SetActive(true);
        this.techType = techType;

        if(_amount is not null)
        {
            _amount.text = amount.ToString();
        }

        _icon.sprite = SpriteManager.Get(techType);
        textLineOne = Language.main.Get(techType);
        _tooltip.SetTechType(techType);
    }

    public void Reset()
    {
        if(gameObject.activeSelf)
        {
            techType = TechType.None;            
            textLineOne = string.Empty;

            if(_amount is not null)
            {
                _amount.text = string.Empty;
            }

            _icon.sprite = SpriteManager.Get(TechType.None);
            gameObject.SetActive(false);
        }
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
