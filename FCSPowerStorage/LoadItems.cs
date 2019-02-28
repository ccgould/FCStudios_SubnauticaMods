using FCSPowerStorage.Configuration;
using FCSPowerStorage.Helpers;
using FCSPowerStorage.Model;
using Harmony;
using UnityEngine;

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

            // 1) INITIALIZE HARMONY
            HarmonyInstance = HarmonyInstance.Create("com.FCStudios.FCSPowerCell");

            Information.BatteryConfiguration = new BatteryConfiguration();

            var customBattery = new CustomBattery("FCSPowerStorage", "FCS Power Storage");
            customBattery.RegisterFCSPowerStorage();
            customBattery.Patch();


        }
    }
}
