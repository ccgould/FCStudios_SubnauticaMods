using FCSCommon.Utilities;

namespace FCS_DeepDriller.Configuration
{
    internal static class EquipmentConfiguration
    {
        internal static readonly string[] SlotIDs = new string[6]
        {
            "DDPowerCellCharger1",
            "DDPowerCellCharger2",
            "DDPowerCellCharger3",
            "DDPowerCellCharger4",
            "HDDAttachmentSlot1",
            "HDDAttachmentSlot2",
        };

        private static bool _addingSlots;

        public static void AddNewSlots()
        {
            if (!_addingSlots)
            {
                foreach (string slotID in SlotIDs)
                {
                    if (slotID.StartsWith("DD"))
                    {
                        Equipment.slotMapping.Add(slotID, EquipmentType.PowerCellCharger);
                        QuickLogger.Debug($"Adding slot {slotID}");
                    }

                    if (slotID.StartsWith("HDD"))
                    {
                        Equipment.slotMapping.Add(slotID, EquipmentType.Hand);
                        QuickLogger.Debug($"Adding slot {slotID}");
                    }
                }

                _addingSlots = true;
            }
        }
    }
}
