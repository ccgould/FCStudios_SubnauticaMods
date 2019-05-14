using FCSCommon.Exceptions;
using FCSTerminal.Configuration;
using FCSTerminal.Helpers;
using FCSTerminal.Models.GameObjects;
using FCSTerminal.Models.Prefabs;
using Harmony;
using UnityEngine;

namespace FCSTerminal
{
    public static class LoadItems
    {
        // Harmony stuff
        internal static HarmonyInstance HarmonyInstance = null;

        /// <summary>
        /// The prefab for the FCS Server Rack
        /// </summary>
        public static GameObject FcsServerRackPrefab { get; set; }

        /// <summary>
        /// The server prefab
        /// </summary>
        public static GameObject ServerPrefab { get; set; }


        public static ServerPrefab SERVER_PREFAB_OBJECT { get; set; }

        public static FCSServerRack SERVER_RACK_PREFAB_OBJECT { get; set; }


        /// <summary>
        /// Patches the new objects into the game
        /// </summary>
        public static void Patch()
        {
            // 1) INITIALIZE HARMONY
            HarmonyInstance = HarmonyInstance.Create("com.FCStudios.FCSTerminal");

            // == Get the Prefabs == //
            if (GetPrefabs())
            {

                // === Create the Server == //
                var server = new ServerPrefab(Information.ModServerName, Information.ModServerFriendly, Information.ModServerDescription);
                server.RegisterItem();
                server.Patch();
                SERVER_PREFAB_OBJECT = server;


                // === Create the Server Rack == //
                var serverRack = new FCSServerRack(Information.ModRackName, Information.ModRackFriendly, Information.ModRackDescription);
                serverRack.Register();
                serverRack.Patch();
                SERVER_RACK_PREFAB_OBJECT = serverRack;

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
            var fcsPowerStoragePrefab = AssetHelper.Asset.LoadAsset<GameObject>("Server_Rack");
            if (fcsPowerStoragePrefab != null)
            {
                FcsServerRackPrefab = fcsPowerStoragePrefab;
            }
            else
            {
                return false;
            }

            var serverPrab = AssetHelper.Asset.LoadAsset<GameObject>("Server");
            if (serverPrab != null)
            {
                ServerPrefab = serverPrab;
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
