using FCS_AlterraHub.Models;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.Spawnables.PaintTool.Mono.UI;
public class uGUI_ColorPickerTemplateItem : MonoBehaviour
{
    [SerializeField]private int index;
    [SerializeField]private Image _primaryCircle;
    [SerializeField]private Image _secondaryCircle;
    [SerializeField]private Image _emissionCircle;
    [SerializeField]private Toggle _toggle;

    private ColorTemplate _colorTemplate = new();
    private bool _isInitialized;

    private void Initialize()
    {
        if (_isInitialized) return;

        _isInitialized = true;
    }
    internal int GetIndex()
    {
        return index;
    }

    public void SetColors(ColorTemplate template)
    {
        Initialize();
        _colorTemplate = template;
        _primaryCircle.color = template.PrimaryColor;
        _secondaryCircle.color = template.SecondaryColor;
        _emissionCircle.color = template.EmissionColor;
    }

    public ColorTemplate GetTemplate()
    {
        return _colorTemplate;
    }

    public void Select()
    {
        QuickLogger.Debug($"Selecting index: {index}", true);
        _toggle?.SetIsOnWithoutNotify(true);
    }
}
