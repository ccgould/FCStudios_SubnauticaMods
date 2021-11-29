using System;
using System.Text;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Spawnables;
using FCS_HomeSolutions.Spawnables;

namespace FCS_HomeSolutions.Mods.QuantumTeleporter.Patches
{
    internal class TooltipFactory_Patch
    {
        private static TechType _powerBankTechType;

        public static void GetToolTip(InventoryItem item, ref string __result)
        {

            if (_powerBankTechType == TechType.None)
            {
                _powerBankTechType = QuantumPowerBankSpawnable.PatchedTechType;
            }

            if (item?.item.GetTechType() != _powerBankTechType) return;

            var controller = item?.item.gameObject.GetComponent<QuantumPowerBankController>();
            var sb = new StringBuilder();
            var text = Language.main.Get(_powerBankTechType);

            GetTitle(sb, text);
            GetDescription(sb,controller);
            __result = sb.ToString();
        }

        private static void GetDescription(StringBuilder sb, QuantumPowerBankController controller)
        {
            sb.AppendFormat("\n<size=20><color=#DDDEDEFF>{0}</color></size>", Language.main.Get("Tooltip_QuantumPowerBank"));
            sb.Append(Environment.NewLine);
            sb.AppendFormat("\n<size=20><color=#00ff00ff>{0}/{1}</color></size>", controller.PowerManager.PowerAvailable(),3000);
        }

        private static void GetTitle(StringBuilder sb, string text)
        {
            sb.AppendFormat("<size=25><color=#ffffffff>{0}</color></size>", text);
        }
    }
}
