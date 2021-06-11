using UnityEngine;

namespace FCS_AlterraHub.Structs
{
    public struct EncyclopediaEntryData
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

        public override string ToString()
        {
            return $"Title: {Title} | Tab Title: {TabTitle}";
        }
    }
}