using FCS_AlterraHub.Helpers;
using UnityEngine;

namespace FCS_AlterraHub.Mods.FCSDataBox.Mono
{
    internal class FCSDataBoxController : MonoBehaviour
    {
        private BlueprintHandTarget _bluePrintDataBox;
        private TechType UnlockTechType { get; set; }

        private void Start()
        {
            _bluePrintDataBox = gameObject.GetComponent<BlueprintHandTarget>();
            UnlockTechType = _bluePrintDataBox.unlockTechType;
            StartCoroutine(SpawnHelper.SpawnUWEPrefab(UWEPrefabID.DataBoxLight, transform));
            InvokeRepeating(nameof(CheckIfUnlocked),1f,1f);
        }

        private void CheckIfUnlocked()
        {
            if (KnownTech.Contains(UnlockTechType))
            {
                _bluePrintDataBox.used = true;
#if SUBNAUTICA_STABLE
                _bluePrintDataBox.Start();
#endif
                CancelInvoke(nameof(CheckIfUnlocked));
            }
        }
    }
}
