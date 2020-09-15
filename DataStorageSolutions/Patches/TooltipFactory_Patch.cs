using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Mono;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator.Objects;

namespace DataStorageSolutions.Patches
{
    internal class TooltipFactory_Patch
    {
        private static TechType _serverTechType;

        public static void GetToolTip(InventoryItem item, ref string __result)
        {
            if (!QPatch.Configuration.Config.ShowServerCustomToolTip) return;

            if (_serverTechType == TechType.None)
            {
                _serverTechType = Mod.ServerClassID.ToTechType();
            }

            if (item?.item.GetTechType() != _serverTechType) return;

            var controller = item?.item.gameObject.GetComponent<DSSServerController>();
            var isFormatted = controller?.FCSFilteredStorage?.Filters?.Count > 0;
            var sb = new StringBuilder();
            var text = Language.main.Get(_serverTechType);

            GetTitle(sb, text);
            GetDescription(sb);
            GetFilters(sb, isFormatted, controller);
            GetItems(controller, sb);

            __result = sb.ToString();
        }

        private static void GetItems(DSSServerController controller, StringBuilder sb)
        {
            var itemCount = controller.FCSFilteredStorage?.Items.Count;
            sb.AppendFormat("\n<size=20><color=#FFA500FF>{0}:</color> <color=#DDDEDEFF>{1}</color></size>", "Storage",
                $"{itemCount}/{QPatch.Configuration.Config.ServerStorageLimit}");

            if (itemCount > 0)
            {
                sb.AppendFormat("\n<size=20><color=#FFA500FF>{0}:</color>\n<color=#DDDEDEFF>{1}</color></size>", $"Items",
                    FormatData(controller.FCSFilteredStorage.Items));
            }
        }

        private static void GetFilters(StringBuilder sb, bool isFormatted, DSSServerController controller)
        {
            sb.AppendFormat("\n<size=20><color=#FFA500FF>{0}</color> <color=#DDDEDEFF>{1}</color></size>",
                AuxPatchers.FiltersCheck(), isFormatted);

            if (isFormatted)
            {
                sb.AppendFormat("\n<size=20><color=#FFA500FF>{0}:</color>\n<color=#DDDEDEFF>{1}</color></size>", $"Filters",
                    controller.FCSFilteredStorage.FormatFiltersData());
            }
        }

        private static void GetDescription(StringBuilder sb)
        {
            sb.AppendFormat("\n<size=20><color=#DDDEDEFF>{0}</color></size>", Language.main.Get("DSS_Description"));
        }

        private static void GetTitle(StringBuilder sb, string text)
        {
            sb.AppendFormat("<size=25><color=#ffffffff>{0}</color></size>", text);
        }

        private static string FormatData(HashSet<ObjectData> items)
        {

            var sb = new StringBuilder();

            var lookup = items?.Where(x => x != null).ToLookup(x => x.TechType).ToArray();

            for (int i = 0; i < lookup.Length; i++)
            {
                if (i < 4)
                {
                    if (lookup[i].All(objectData => objectData.TechType != lookup[i].Key)) continue;
                    sb.Append($"{Language.main.Get(lookup[i].Key)} x{lookup[i].Count()}");
                    sb.Append(Environment.NewLine);
                }
                else
                {
                    sb.Append($"And More.....");
                    break;
                }

            }

            return sb.ToString();
        }
    }
}
