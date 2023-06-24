using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.ModItems.Buildables.RemoteStorage.Mono;
public class uGUI_RemoteStorageItem : MonoBehaviour
{
    [SerializeField] private uGUI_Icon _icon;
    [SerializeField] private Text _amount;
    private TechType techType;
    private string textLineOne;

    internal void Set(TechType techType, int amount)
    {
        this.techType = techType;
        _amount.text = amount.ToString();
        _icon.sprite = SpriteManager.Get(techType);
        textLineOne = Language.main.Get(techType);
    }

    public void OnClick()
    {

    }
}
