using System.Security.Cryptography;
using FCS_AlterraHub.Helpers;
using UnityEngine;

namespace FCS_AlterraHub.Mods.FCSDataBox.Mono
{
    internal class FCSDataBoxController : MonoBehaviour
    {
        private BlueprintHandTarget _bluePrintDataBox;
        private string _id;
        private bool _isInitalized;
        private bool _fromSave;
        private TechType UnlockTechType { get; set; }

        private void Start()
        {
            _bluePrintDataBox = gameObject.GetComponent<BlueprintHandTarget>();
            UnlockTechType = _bluePrintDataBox.unlockTechType;
            StartCoroutine(SpawnHelper.SpawnUWEPrefab(UWEPrefabID.DataBoxLight,this, transform));
            InvokeRepeating(nameof(CheckIfUnlocked),1f,1f);
        }

        private void CheckIfUnlocked()
        {
            if (KnownTech.Contains(UnlockTechType))
            {
                _bluePrintDataBox.used = true;
                _bluePrintDataBox.Start();
                CancelInvoke(nameof(CheckIfUnlocked));
            }
        }
    }
}
