using FCS_AlterraHub.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.Spawnables.PaintTool.Mono.UI;
public class uGUI_ColorPickerTemplateItem : MonoBehaviour
{
    [SerializeField]private Image _primaryCircle;
    [SerializeField]private Image _secondaryCircle;
    [SerializeField]private Image _emissionCircle;
    [SerializeField]private Toggle _toggle;
    [SerializeField]private TextMeshProUGUI _text;

    private ColorTemplate _colorTemplate = new();


    public void SetColors(ColorTemplate template)
    {
        _colorTemplate = template;
        _primaryCircle.color = template.PrimaryColor;
        _secondaryCircle.color = template.SecondaryColor;
        _emissionCircle.color = template.EmissionColor;
        _text.text = template.TemplateName;
    }

    public ColorTemplate GetTemplate()
    {
        return _colorTemplate;
    }

    public void Select()
    {
        _toggle?.SetIsOnWithoutNotify(true);
    }
}
