using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;

namespace FCS_AlterraHub.Registration
{
    public interface IFCSAlterraHubService
    {
        void RegisterDevice(FcsDevice device, string tabID);
        void RemoveDeviceFromGlobal(string unitID);
        void CreateStoreEntry(TechType techType, TechType receiveTechType, float cost, StoreCategory category);
        Dictionary<TechType, FCSStoreEntry> GetRegisteredKits();
        TechType GetRegisteredKit(TechType techType);
        Dictionary<string, FcsDevice> GetRegisteredDevices();
    }

    internal interface IFCSAlterraHubServiceInternal
    {
        
    }

    public class FCSAlterraHubService : IFCSAlterraHubService, IFCSAlterraHubServiceInternal
    {
        private static readonly FCSAlterraHubService singleton = new FCSAlterraHubService();

        public static Dictionary<string,string> knownDevices = new Dictionary<string,string>();
        private static readonly Dictionary<string, FcsDevice> GlobalDevices = new Dictionary<string,FcsDevice>();
        private static Dictionary<TechType, FCSStoreEntry> _storeItems = new Dictionary<TechType, FCSStoreEntry>();
        public static IFCSAlterraHubService PublicAPI => singleton;
        internal static IFCSAlterraHubServiceInternal InternalAPI => singleton;

        private FCSAlterraHubService()
        {
            Mod.OnDevicesDataLoaded += OnDataLoaded;
        }

        private void OnDataLoaded(Dictionary<string, string> obj)
        {
            knownDevices = obj;
        }
        
        public void RegisterDevice(FcsDevice device, string tabID)
        {
            var prefabID = device.GetPrefabID();

            if (string.IsNullOrWhiteSpace(prefabID)) return;
            
            if (!knownDevices.ContainsKey(prefabID))
            {
                var id = $"{tabID}{knownDevices.Count:D3}";
                device.UnitID = id;
                knownDevices.Add(device.GetPrefabID(), id);
                AddToGlobalDevices(device, prefabID, id);
                Mod.SaveDevices(knownDevices);
            }
            else
            {
                device.UnitID=knownDevices[prefabID];
                AddToGlobalDevices(device, prefabID, knownDevices[prefabID]);
            }

            BaseManagerSetup(device);
        }

        private static void AddToGlobalDevices(FcsDevice device, string prefabId, string id)
        {
            if (!GlobalDevices.ContainsKey(prefabId))
            {
                GlobalDevices.Add(id, device);
            }
        }

        private static void BaseManagerSetup(FcsDevice device)
        {
            if (string.IsNullOrEmpty(device.BaseId))
            {
                var subRoot = device.gameObject.GetComponentInParent<SubRoot>();
                if(subRoot != null)
                {
                    device.BaseId = subRoot.gameObject.GetComponent<PrefabIdentifier>().Id;
                }
            }

            var manager = BaseManager.FindManager(device.BaseId);
            QuickLogger.Debug($"Manager Returned: {manager?.BaseID}");
            device.Manager = manager;
            manager.RegisterDevice(device);
        }

        public void RegisterNewCard(string prefabID)
        {
            CardSystem.main.GenerateNewCard(prefabID);
        }

        public void UnRegisterCard(string cardNumber)
        {
            CardSystem.main.DeleteCard(cardNumber);
        }

        public void RemoveDeviceFromGlobal(string unitID)
        {
            GlobalDevices.Remove(unitID);
        }

        public void CreateStoreEntry(TechType techType,TechType recieveTechType,float cost,StoreCategory category)
        {
            if (!_storeItems.ContainsKey(techType))
            {
                _storeItems.Add(techType,new FCSStoreEntry{TechType = techType, ReceiveTechType = recieveTechType, Cost = cost, StoreCategory = category});
            }
        }

        public Dictionary<TechType,FCSStoreEntry> GetRegisteredKits()
        {
            return _storeItems;
        }

        public TechType GetRegisteredKit(TechType techType)
        {
            return _storeItems.Any(x=>x.Value.ReceiveTechType == techType) ? _storeItems.FirstOrDefault(x => x.Value.ReceiveTechType == techType).Key : TechType.None;
        }

        public Dictionary<string, FcsDevice> GetRegisteredDevices()
        {
            return GlobalDevices;
        }
    }

    public struct FCSStoreEntry
    {
        public TechType TechType { get; set; }
        public float Cost { get; set; }
        public StoreCategory StoreCategory { get; set; }
        public TechType ReceiveTechType { get; set; }
    }
}
