using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Model
{
    public class FCSToolTip : MonoBehaviour, ITooltip
    {
        public string Tooltip;
        public Func<string> ToolTipStringDelegate;
        public Func<bool> RequestPermission { get; set; }

        void Awake() => Destroy(GetComponent<LayoutElement>());

#if BELOWZERO
        public bool showTooltipOnDrag => true;

        public void GetTooltip(TooltipData tooltip)
        {
            tooltip.prefix.Append(Tooltip);
        }
#else
        public void GetTooltip(out string tooltipText, List<TooltipIcon> _)
        {
            var result = RequestPermission?.Invoke() ?? false;
            if (ToolTipStringDelegate != null)
            {
                Tooltip = ToolTipStringDelegate?.Invoke();
            }
            tooltipText = result ? Tooltip : string.Empty;
        }
#endif
    }
}
