using FCSCommon.Exceptions;
using FCSCommon.Utilities;
using FCSTechFabricator.Mono;
using Harmony;
using Oculus.Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using FCSTechFabricator.Helpers;
using UnityEngine;

namespace FCSTechFabricator
{
    public class QPatch
    {
        #region Public Properties

        public static GameObject ColorItem { get; internal set; }

        public static GameObject SeaAlienGasTank { get; internal set; }

        public static GameObject SeaGasTank { get; internal set; }

        public static GameObject BatteryModule { get; internal set; }

        public static GameObject FocusModule { get; internal set; }

        public static GameObject SolarModule { get; internal set; }

        public static GameObject Kit { get; internal set; }

        public static AssetBundle Bundle { get; internal set; }
        public static GameObject Freon { get; internal set; }
        #endregion

        #region Internal Properties
        internal static Configuration ModConfiguration { get; set; }
        #endregion

        #region Public Methods
        public static void Patch()
        {

            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion());

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                // == Load Configuration == //
                string configJson = File.ReadAllText(Information.ConfigurationFile().Trim());

                //LoadData
                ModConfiguration = JsonConvert.DeserializeObject<Configuration>(configJson);

                if (PatchHelpers.GetPrefabs())
                {
                    FCSTechFabricatorBuildable.PatchHelper();
                    PatchHelpers.RegisterItems();
                    QuickLogger.Debug(FCSTechFabricatorBuildable.TechFabricatorCraftTreeType.ToString());

                    var harmony = HarmonyInstance.Create("com.fcstechfabricator.fcstudios");

                    harmony.PatchAll(Assembly.GetExecutingAssembly());

                    QuickLogger.Info("Finished patching");
                }
                else
                {
                    throw new PatchTerminatedException();
                }
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        #endregion

    }
}
