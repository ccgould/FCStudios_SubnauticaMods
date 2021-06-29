using FCS_AlterraHub.Extensions;
using UnityEngine;

namespace FCS_AlterraHub.Structs
{
    public class EncyclopediaEntryData
    {
        public string ModPackID;
        public string AudioName;
        public string Path;
        public string TabTitle;
        public string Body;
        public string Title;
        public string ImageName;
        public bool Unlocked;
        public string UnlockedBy;
        public string Blueprint;
        public bool DestroyFragmentAfterScan;
        public bool HasFragment;
        public int TotalFragmentsToUnlock;
        public int ScanTime;
        private TechType _techType;
        public string TechTypeString { get; set; }


        public bool IsSame(TechType techType)
        {
            if (_techType == TechType.None)
            {
                _techType = TechTypeString.ToTechType();
            }

            return _techType == techType;
        }

        public override string ToString()
        {
            return $"Title: {Title} | Tab Title: {TabTitle}";
        }
    }
}