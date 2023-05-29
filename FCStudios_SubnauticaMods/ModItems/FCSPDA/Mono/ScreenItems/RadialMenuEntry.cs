using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//#if SUBNAUTICA
//using Sprite = Atlas.Sprite;
//#endif

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.ScreenItems;

public class RadialMenuEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Text _label;
    private string _buttonName;

    [SerializeField]
    private GameObject Hover;

    [SerializeField]
    private uGUI_Icon Icon;
    private PDAPages _page;
    private FCSAlterraHubGUI _controller;


    internal void Initialize(FCSAlterraHubGUI controller, Text pLabel, Sprite pIcon, Text pageLabel, string buttonName, PDAPages page)
    {
#if SUBNAUTICA
        Icon.sprite = new Atlas.Sprite(pIcon);
#else
        Icon.sprite = pIcon;
#endif

        _controller = controller;

        _label = pageLabel;
        _label = pLabel;
        _buttonName = buttonName;
        _page = page;
    }

    private void SetLabel(string pText)
    {
        _label.text = pText;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetLabel(_buttonName);
        Select();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetLabel(string.Empty);
        Deselect();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _controller.GoToPage(_page,_page);
        Deselect();
        _label.text = string.Empty;
    }

    public void Select()
    {
        Hover.SetActive(true);
    }

    public void Deselect()
    {
        Hover.SetActive(false);
    }
}
