using UnityEngine.UI;

namespace FCS_AlterraHub.Structs
{
    public struct EncyclopediaEntryData
    {
        public int Order;
        public string Package;
        public string TabTitle;
        public string Body;
        public string Title;
        public string ImageName;
        internal Image Image;

        public override string ToString()
        {
            return $"Title: {Title} | Tab Title: {TabTitle} | Package: {Package} | Order: {Order}";
        }
    }
}