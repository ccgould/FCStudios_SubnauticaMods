using FCSCommon.Exceptions;
using FCSDeepHarvester.Configuration;
using FCSDeepHarvester.Helpers;
using FCSDeepHarvester.Logging;
using FCSDeepHarvester.Models.Prefabs;
using Harmony;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace FCSDeepHarvester
{
    public class LoadItems
    {
        // Harmony stuff
        internal static HarmonyInstance HarmonyInstance = null;
        public static GameObject FcsDeepHarvesterPrefab { get; set; }
        public static DeepHarvester DEEP_HARVESTER_PREFAB_OBJECT { get; set; }

        /// <summary>
        /// Execute to start the creation process to load the items into the game
        /// </summary>

        public static void Patch()
        {
            HarmonyInstance = HarmonyInstance.Create("com.FCStudios.FCSDeepHarvester");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            // == Get the Prefabs == //
            if (GetPrefabs())
            {
                Log.Debug("GetPrefabs");
                // === Create the Server == //
                var server = new DeepHarvester(Information.ModName, Information.ModFriendly, Information.ModDescription);
                server.Register();
                server.Patch();
                DEEP_HARVESTER_PREFAB_OBJECT = server;
            }
            else
            {
                throw new PatchTerminatedException("Error loading finding a prefab");
            }

        }


        /// <summary>
        /// Loads the prefabs from the asset bundle
        /// </summary>
        /// <returns></returns>
        private static bool GetPrefabs()
        {
            //Log.Info(Information.);

            //Log.Info(Path.Combine(Path.Combine(Environment.CurrentDirectory, "QMods"), Path.Combine("FCSPowerStorage", "fcspowerstorage-mod")));

            // == Get the server rack prefab == //
            Log.Info("Load Asset");
            var fcsDeepHarvesterPrefab = AssetHelper.Asset.LoadAsset<GameObject>("DeepHarvester");

            if (fcsDeepHarvesterPrefab != null)
            {
                Log.Info("Pass Asset");

                FcsDeepHarvesterPrefab = fcsDeepHarvesterPrefab;
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
