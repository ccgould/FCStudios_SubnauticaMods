using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Core.Components.uGUIComponents;

[RequireComponent(typeof(Selectable))]
[DisallowMultipleComponent]
public class uGUI_PlayerDistanceDisable : MonoBehaviour, ITooltip, ILocalizationCheckable
{
    private Selectable _selectable;
    [SerializeField] private float range = 5f;
    [SerializeField] private string defaultText = "OpenStorage";
    [SerializeField] private string errorText = "AHB_ToFarFromDevice";
    [SerializeField] private bool translate = true;
    private string text;

    public bool showTooltipOnDrag => false;

    void ITooltip.GetTooltip(TooltipData data)
    {
        data.prefix.Append(this.translate ? Language.main.Get(this.text) : this.text);
    }

    public string CompileTimeCheck(ILanguage language)
    {
        if (!this.translate)
        {
            return null;
        }
        return language.CheckKey(this.errorText, true);
    }

    private void Awake()
    {
        _selectable = GetComponent<Selectable>();
    }
    public void Start()
    {
        InvokeRepeating(nameof(Check), 1f, 1f);
    }

    private void Check()
    {
        var device = FCSPDAController.Main.GetGUI().GetCurrentDevice();
        if (device is not null && !WorldHelpers.CheckIfInRange(Player.mainObject, ((FCSDevice)device).gameObject, range))
        {
            _selectable.interactable = false;
            text = errorText;
        }
        else
        {
            _selectable.interactable = true;
            text = defaultText;
        }
    }
}
