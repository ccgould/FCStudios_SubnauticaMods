using UnityEngine;

namespace FCSTerminal.Fixers
{
    public class ConstructableFixer
    {
        public static bool CanDeconstruct_Prefix(Constructable __instance, ref bool __result, out string reason)
        {
            string techTypeStr = __instance.techType.AsString();
            if (techTypeStr.StartsWith("TestObject") ||
                techTypeStr.StartsWith("DecorativeLockerClosed") ||
                techTypeStr.StartsWith("DecorativeLockerDoor") ||
                techTypeStr.StartsWith("CargoBox01_damaged") ||
                techTypeStr.StartsWith("CargoBox01a") ||
                techTypeStr.StartsWith("CargoBox01b"))
            {
                foreach (Transform tr in __instance.gameObject.transform)
                {
                    if (tr.name.StartsWith("Locker(Clone)"))
                    {
                        StorageContainer sc = tr.GetComponent<StorageContainer>();
                        if (sc != null && sc.container != null && sc.container.count > 0)
                        {
                            if (Language.main != null)
                            {
                                string notEmptyError = Language.main.Get("DeconstructNonEmptyStorageContainerError");
                                reason = (!string.IsNullOrEmpty(notEmptyError) ? notEmptyError : "Not empty!");
                            }
                            else
                                reason = "Not empty!";
                            __result = false;
                            return false;
                        }
                    }
                }
            }
            reason = null;
            return true;
        }
    }
}
