
using System.Reflection;
using FCSTerminal.Fixers;
using FCSTerminal.Items;
using Harmony;

namespace FCSTerminal
{
    public static class LoadItems
    {
        // Harmony stuff
        internal static HarmonyInstance HarmonyInstance = null;

        public static void Patch()
        {


            // 1) INITIALIZE HARMONY
            HarmonyInstance = HarmonyInstance.Create("com.FCStudios.FCSTerminal");

            ServerRack serverRack = new ServerRack("ServerRack", "FCS Server Rack", "This is a server rack for the FCS Terminal Mod");
            serverRack.Main();
            serverRack.Patch();

            FCSTerminalItem FCSTerminal = new FCSTerminalItem("FCSTerminal", "FCS Terminal Screen", "This is the terminal for the FCS Terminal Mod");
            FCSTerminal.Register();
            FCSTerminal.Patch();

            // Fix cargo crates items-containers
            var onProtoDeserializeObjectTreeMethod = typeof(StorageContainer).GetMethod("OnProtoDeserializeObjectTree", BindingFlags.Public | BindingFlags.Instance);
            var onProtoDeserializeObjectTreePostfix = typeof(StorageContainerFixer).GetMethod("OnProtoDeserializeObjectTree_Postfix", BindingFlags.Public | BindingFlags.Static);
            HarmonyInstance.Patch(onProtoDeserializeObjectTreeMethod, null, new HarmonyMethod(onProtoDeserializeObjectTreePostfix));
            // Failsafe on lockers and cargo crates deconstruction
            var canDeconstructMethod = typeof(Constructable).GetMethod("CanDeconstruct", BindingFlags.Public | BindingFlags.Instance);
            var canDeconstructPrefix = typeof(ConstructableFixer).GetMethod("CanDeconstruct_Prefix", BindingFlags.Public | BindingFlags.Static);
            HarmonyInstance.Patch(canDeconstructMethod, new HarmonyMethod(canDeconstructPrefix), null);

            //ServerRack01 serverRack01 = new ServerRack01();
            //serverRack01.RegisterItem();


        }
    }
}
