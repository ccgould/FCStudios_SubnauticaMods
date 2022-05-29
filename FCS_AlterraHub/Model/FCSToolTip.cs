using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Model
{
    public class FCSToolTip : MonoBehaviour, ITooltip
    {
        public string Tooltip;
        public Func<string> ToolTipStringDelegate;
        public TechType TechType { get; set; }
        public Func<bool> RequestPermission { get; set; }
        public bool Description { get; set; } = false;
        void Awake() => Destroy(GetComponent<LayoutElement>());

#if BELOWZERO
        public bool showTooltipOnDrag => true;

        public void GetTooltip(TooltipData tooltip)
        {
            tooltip.prefix.Append(Tooltip);
        }
#else
        public void GetTooltip(out string tooltipText, List<TooltipIcon> tooltipIcons)
        {
            var result = RequestPermission?.Invoke() ?? false;
            
            if (ToolTipStringDelegate != null)
            {
                Tooltip = ToolTipStringDelegate?.Invoke();
            }

            if(TechType != TechType.None)
            {
                if (Description)
                {
                    Tooltip = InventoryItemView(TechType);
                }
                else
                {
                    bool locked = !CrafterLogic.IsCraftRecipeUnlocked(TechType);
                    TooltipFactory.BuildTech(TechType, locked, out Tooltip, tooltipIcons);
                }
            }

            tooltipText = result ? Tooltip : string.Empty;
        }
#endif

        public static string InventoryItemView(TechType techType)
        {
            TooltipFactory.Initialize();
            StringBuilder stringBuilder = new StringBuilder();
            TooltipFactory.WriteTitle(stringBuilder, Language.main.Get(techType));
            TooltipFactory.WriteDescription(stringBuilder, Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(techType)));
            return stringBuilder.ToString();
        }
    }
}
