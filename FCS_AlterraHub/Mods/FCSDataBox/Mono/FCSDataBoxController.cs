using UnityEngine;

namespace FCS_AlterraHub.Mods.FCSDataBox.Mono
{
    internal class FCSDataBoxController : MonoBehaviour
    {
        private BlueprintHandTarget _bluePrintDataBox;
        internal TechType UnlockTechType;

        internal void Initialize()
        {
            _bluePrintDataBox = gameObject.GetComponent<BlueprintHandTarget>();
            _bluePrintDataBox.unlockTechType = UnlockTechType;
        }
    }
}
