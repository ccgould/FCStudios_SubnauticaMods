using System.Collections.Generic;
using SMLHelper.V2.Assets;

namespace FlexibleTechFabricator
{
    public interface IFcTechFabricatorService
    {
        void AddCraftNode(Craftable item, string parentTabId);
        void AddTabNode(string tabId, string displayText, Atlas.Sprite tabSprite);
        bool HasCraftingTab(string tabId);
    }

    internal interface IFcTechFabricatorServiceInternal
    {
        void PatchFabriactor();
    }

    public class FcTechFabricatorService : IFcTechFabricatorService, IFcTechFabricatorServiceInternal
    {
        internal static readonly FcTechFabricator fcTechFabricator = new FcTechFabricator();
        internal static readonly ICollection<string> knownTabs = new List<string>();
        internal static readonly ICollection<string> knownItems = new List<string>();

        private static readonly FcTechFabricatorService singleton = new FcTechFabricatorService();

        public static IFcTechFabricatorService PublicAPI => singleton;
        internal static IFcTechFabricatorServiceInternal InternalAPI => singleton;

        private FcTechFabricatorService()
        {
        }

        public void AddCraftNode(Craftable item, string parentTabId)
        {
            if (!knownItems.Contains(item.ClassID))
            {
                fcTechFabricator.AddCraftNode(item, parentTabId);
                knownItems.Add(item.ClassID);
            }
        }

        public void AddTabNode(string tabId, string displayText, Atlas.Sprite tabSprite)
        {
            if (!knownTabs.Contains(tabId))
            {
                fcTechFabricator.AddTabNode(tabId, displayText, tabSprite);
                knownTabs.Add(tabId);
            }
        }

        public bool HasCraftingTab(string tabId)
        {
            return knownTabs.Contains(tabId);
        }

        void IFcTechFabricatorServiceInternal.PatchFabriactor()
        {
            fcTechFabricator.Patch();
        }
    }
}
