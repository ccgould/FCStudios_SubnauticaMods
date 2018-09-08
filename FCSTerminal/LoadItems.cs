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

            //// 1) INITIALIZE HARMONY
            //HarmonyInstance = HarmonyInstance.Create("com.FCStudios.FCSTerminal");

            //// Failsafe on lockers and cargo crates deconstruction
            //var canDeconstructMethod = typeof(Constructable).GetMethod("CanDeconstruct", BindingFlags.Public | BindingFlags.Instance);
            //var canDeconstructPrefix = typeof(ConstructableFixer).GetMethod("CanDeconstruct_Prefix", BindingFlags.Public | BindingFlags.Static);
            //HarmonyInstance.Patch(canDeconstructMethod, new HarmonyMethod(canDeconstructPrefix), null);

            ServerRack01 serverRack01 = new ServerRack01();
            serverRack01.RegisterItem();
        }
    }
}
