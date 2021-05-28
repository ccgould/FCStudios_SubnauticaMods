using UnityEngine;

namespace FCS_AlterraHub.Structs
{
    public struct EncyclopediaEntryData
    {
        public string Path;
        public string TabTitle;
        public string Body;
        public string Title;
        public string ImageName;
        public bool Unlocked;
        internal Texture2D Image;
        public string UnlockedBy;

        public override string ToString()
        {
            return $"Title: {Title} | Tab Title: {TabTitle}";
        }
    }
}