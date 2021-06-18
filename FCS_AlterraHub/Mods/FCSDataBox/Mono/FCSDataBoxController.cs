using UnityEngine;

namespace FCS_AlterraHub.Mods.FCSDataBox.Mono
{
    internal class FCSDataBoxController : MonoBehaviour
    {
        private BlueprintHandTarget _bluePrintDataBox;
        internal TechType UnlockTechType;

        private void Initialize()
        {
            _bluePrintDataBox = gameObject.EnsureComponent<BlueprintHandTarget>();
            _bluePrintDataBox.animator = GetComponent<Animator>(); ;
            _bluePrintDataBox.animParam = "databox_take";
            _bluePrintDataBox.animParam = "databox_lookat";
            _bluePrintDataBox.unlockTechType = UnlockTechType;
        }
    }
}
