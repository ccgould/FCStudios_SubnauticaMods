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

        public bool showTooltipOnDrag => true;

        public void GetTooltip(TooltipData tooltip)
        {
            tooltip.prefix.Append(Tooltip);
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
}
