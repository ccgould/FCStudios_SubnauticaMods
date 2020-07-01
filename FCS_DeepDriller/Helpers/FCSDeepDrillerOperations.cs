using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_DeepDriller.Mono.MK2;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;

namespace FCS_DeepDriller.Helpers
{
    internal static class FCSDeepDrillerOperations
    {
        public static bool GivePlayerItem(TechType techType, int amount = 1)
        {
            try
            {
                for (int i = 0; i < amount; i++)
                {
                    if (!Inventory.main.Pickup(techType.ToPickupable()))
                    {
                        QuickLogger.Message(LanguageHelpers.GetLanguage("InventoryFull"), true);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error<FCSDeepDrillerContainer>(e.Message);
                QuickLogger.Error<FCSDeepDrillerContainer>(e.StackTrace);
                return false;
            }
            return true;
        }

        internal static bool CanPlayerHold(TechType techType)
        {
            var size = CraftData.GetItemSize(techType);
            return Inventory.main.HasRoomFor(size.x, size.y);
        }
    }
}
