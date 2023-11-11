using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Core.Components.uGUIComponents;
public class FCSToolTip : MonoBehaviour, ITooltip
{
    [SerializeField] private string tooltip;
    [SerializeField] private TechType techType;
    public Func<bool> RequestPermission;
    public Func<string> ToolTipStringDelegate;
    [SerializeField] private bool isDescription  = false;

    public bool showTooltipOnDrag => true;

    void Awake() => Destroy(GetComponent<LayoutElement>());


    public void GetTooltip(TooltipData tooltip)
    {
        var result = RequestPermission?.Invoke() ?? false;

        if (ToolTipStringDelegate != null)
        {
            this.tooltip = ToolTipStringDelegate?.Invoke();
        }

        if (techType != TechType.None)
        {
            if (isDescription)
            {
                this.tooltip = InventoryItemView(techType);
            }
            else
            {
                bool locked = !CrafterLogic.IsCraftRecipeUnlocked(techType);
                TooltipFactory.BuildTech(techType, locked, tooltip);
            }
        }

        var tooltipText = result ? this.tooltip : string.Empty;


        tooltip.prefix.Append(tooltipText);
    }

    public static string InventoryItemView(TechType techType)
    {
        TooltipFactory.Initialize();
        StringBuilder stringBuilder = new StringBuilder();
        TooltipFactory.WriteTitle(stringBuilder, Language.main.Get(techType));
        TooltipFactory.WriteDescription(stringBuilder, Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(techType)));
        return stringBuilder.ToString();
    }
}
