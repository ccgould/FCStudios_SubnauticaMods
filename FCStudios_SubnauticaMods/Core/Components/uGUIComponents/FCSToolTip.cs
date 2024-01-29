using FCS_AlterraHub.Core.Helpers;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Core.Components.uGUIComponents;
public class FCSToolTip : MonoBehaviour, ITooltip
{
    [SerializeField] private bool isDescription = false;
    [SerializeField] private string tooltip;
    [SerializeField] private string descriptionToolTip;
    [SerializeField] private TechType techType;
    [SerializeField] private float playerDistance;
    [SerializeField] private bool usePlayerDistance;
    public Func<bool> RequestPermission;
    public Func<string> ToolTipStringDelegate;

    public bool showTooltipOnDrag => true;

    void Awake() => Destroy(GetComponent<LayoutElement>());

    public void GetTooltip(TooltipData tooltip)
    {
        bool result;

        if (usePlayerDistance)
        {
            result = WorldHelpers.CheckIfPlayerInRange(gameObject, playerDistance);
        }
        else
        {
            result = RequestPermission?.Invoke() ?? false;
        }

        if (!result) return;


        if (ToolTipStringDelegate != null)
        {
            this.tooltip = ToolTipStringDelegate?.Invoke();
            var tooltipText = result ? this.tooltip : string.Empty;
            tooltip.prefix.Append(tooltipText);
        }

        if (techType != TechType.None)
        {
            if (isDescription)
            {
                InventoryItemView(tooltip.prefix, techType);
            }
            else
            {
                bool locked = !CrafterLogic.IsCraftRecipeUnlocked(techType);
                TooltipFactory.BuildTech(techType, locked, tooltip);
            }
        }
        else if (isDescription)
        {
            CustomDescripiveTooltip(tooltip.prefix);
        }
    }

    private void CustomDescripiveTooltip(StringBuilder sb)
    {
        TooltipFactory.Initialize();
        TooltipFactory.WriteTitle(sb, Language.main.Get(tooltip));
        TooltipFactory.WriteDescription(sb, Language.main.Get(descriptionToolTip));
    }

    public void InventoryItemView(StringBuilder sb, TechType techType)
    {
        TooltipFactory.Initialize();
        TooltipFactory.WriteTitle(sb, Language.main.Get(techType));
        TooltipFactory.WriteDescription(sb, Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(techType)));
    }

    public void SetTechType(TechType techType)
    {
        this.techType = techType; 
    }
}