using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.ModItems.Buildables.RemoteStorage.Mono;
public class uGUI_RemoteStorageItem : MonoBehaviour
{
    [SerializeField] private uGUI_Icon _icon;
    [SerializeField] private Text _amount;
    [SerializeField] private uGUI_RemoteStorage controller;

    private TechType techType;
    private string textLineOne;

   

    internal void Set(TechType techType, int amount)
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
        controller.OnInventoryButtonClicked(this);
    }

    public TechType GetTechType()
    {
        return techType;
    }
}
