using System.Collections.Generic;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Model.GUI
{
    public static class EquipmentConfiguration
    {
        internal static Dictionary<string, SlotInformation> SlotIDs = new();

        private static bool _addingSlots;

        public static void AddNewSlots()
        {
            if (!_addingSlots)
            {
                foreach (KeyValuePair<string, SlotInformation> slotID in SlotIDs)
                {
                    Equipment.slotMapping.Add(slotID.Key, slotID.Value.EquipmentType);
                    QuickLogger.Debug($"Adding slot {slotID.Key}");
                }

                _addingSlots = true;
            }
        }

        public static void AddNewSlot(string id,SlotInformation equipmentType)
        {
            if (!SlotIDs.ContainsKey(id))
            {
                SlotIDs.Add(id,equipmentType);
            }
        }

        internal static void RefreshPDA()
        {
            if (Player.main == null)
            {
                QuickLogger.Debug("Player was null when trying to refresh PDA");
                return;
            }

            PDA pdaMain = Player.main.GetPDA();
            pdaMain.Open();
            pdaMain.Close();
            QuickLogger.Debug("Refreshed PDA.");
        }

        public static Vector2 GetSlotPosition(int i)
        {
            return Initialize_uGUI.GetPositionForSlot(i);
        }
    }

    public struct SlotInformation
    {
        public EquipmentType EquipmentType;
        public Vector2 Position;
    }
}

