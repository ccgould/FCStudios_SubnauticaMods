using System.Collections.Generic;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Configuration;
using SMLHelper.V2.Assets;
using UnityEngine;

namespace FCSTechFabricator
{
    public interface IFcTechFabricatorService
    {
        void AddCraftNode(Craftable item, string parentTabId);
#if SUBNAUTICA
        void AddTabNode(string tabId, string displayText, Atlas.Sprite tabSprite, string parentTabNodeId = null);

#elif BELOWZERO
        void AddTabNode(string tabId, string displayText, Sprite tabSprite, string parentTabNodeId = null);

#endif
        bool HasCraftingTab(string tabId);
        void RegisterDevice(FCSConnectableDevice device,string prefabID, string tabID);
    }

    internal interface IFcTechFabricatorServiceInternal
    {
        void PatchFabricator();
    }

    public class FcTechFabricatorService : IFcTechFabricatorService, IFcTechFabricatorServiceInternal
    {
        internal static readonly FcTechFabricator fcTechFabricator = new FcTechFabricator();
        internal static readonly ICollection<string> knownTabs = new List<string>();
        internal static readonly ICollection<string> knownItems = new List<string>();
        internal static Dictionary<string,string> knownDevices = new Dictionary<string,string>();

        private static readonly FcTechFabricatorService singleton = new FcTechFabricatorService();

        public static IFcTechFabricatorService PublicAPI => singleton;
        internal static IFcTechFabricatorServiceInternal InternalAPI => singleton;

        private FcTechFabricatorService()
        {
            Mod.OnDataLoaded += OnDataLoaded;
        }

        private void OnDataLoaded(Dictionary<string, string> obj)
        {
            knownDevices = obj;
        }

        public void AddCraftNode(Craftable item, string parentTabId)
        {
            if (!knownItems.Contains(item.ClassID))
            {
                fcTechFabricator.AddCraftNode(item, parentTabId);
                knownItems.Add(item.ClassID);
            }
        }

#if SUBNAUTICA
        public void AddTabNode(string tabId, string displayText, Atlas.Sprite tabSprite,string parentTabNodeId = null)
        {
            if (parentTabNodeId != null && !knownTabs.Contains(parentTabNodeId))
            {
                QuickLogger.Error($"Parent Tab {parentTabNodeId} does not exist and should be added before adding this tab {tabId} to the fabricator");
                return;
            }

            if (!knownTabs.Contains(tabId))
            {
                fcTechFabricator.AddTabNode(tabId, displayText, tabSprite,parentTabNodeId);
                knownTabs.Add(tabId);
            }
        }
#elif BELOWZERO
        public void AddTabNode(string tabId, string displayText, Sprite tabSprite, string parentTabNodeId = null)
        {
            if (parentTabNodeId != null && !knownTabs.Contains(parentTabNodeId))
            {
                QuickLogger.Error($"Parent Tab {parentTabNodeId} does not exist and should be added before adding this tab {tabId} to the fabricator");
                return;
            }

            if (!knownTabs.Contains(tabId))
            {
                fcTechFabricator.AddTabNode(tabId, displayText, tabSprite, parentTabNodeId);
                knownTabs.Add(tabId);
            }
        }
#endif

        public bool HasCraftingTab(string tabId)
        {
            return knownTabs.Contains(tabId);
        }

        public void RegisterDevice(FCSConnectableDevice device, string prefabID, string tabID)
        {
            if (!knownDevices.ContainsKey(prefabID))
            {
                var id = $"{tabID}{knownDevices.Count:D3}";
                device.UnitID = id;
                knownDevices.Add(prefabID,id);
                Mod.SaveDevices(knownDevices);
            }
            else
            {
                device.UnitID=knownDevices[prefabID];
            }
        }

        void IFcTechFabricatorServiceInternal.PatchFabricator()
        {
            fcTechFabricator.Patch();
        }
    }
}
