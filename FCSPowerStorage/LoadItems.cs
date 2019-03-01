using FCSPowerStorage.Model;
using Harmony;

namespace FCSPowerStorage
{
    public class LoadItems
    {
        // Harmony stuff
        internal static HarmonyInstance HarmonyInstance = null;


        /// <summary>
        /// Execute to start the creation process to load the items into the game
        /// </summary>
        public static void Patch()
        {
            HarmonyInstance = HarmonyInstance.Create("com.FCStudios.FCSPowerCell");

            var customBattery = new CustomBattery("FCSPowerStorage", "FCS Power Storage");
            customBattery.RegisterFCSPowerStorage();
            customBattery.Patch();
        }
    }
}
