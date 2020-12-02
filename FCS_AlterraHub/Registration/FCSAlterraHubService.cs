using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Structs;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace FCS_AlterraHub.Registration
{
    public interface IFCSAlterraHubService
    {
        void RegisterDevice(FcsDevice device, string tabID, string packageId);
        void RemoveDeviceFromGlobal(string unitID);
        void CreateStoreEntry(TechType techType, TechType receiveTechType, decimal cost, StoreCategory category);
        Dictionary<TechType, FCSStoreEntry> GetRegisteredKits();
        TechType GetRegisteredKit(TechType techType);
        Dictionary<string, FcsDevice> GetRegisteredDevices();
        bool IsModPatched(string mod);
        void RegisterPatchedMod(string mod);
        int GetRegisterModCount(string tabID);
        bool IsTechTypeRegistered(TechType techType);
        void RegisterEncyclopediaEntry(TechType techType, List<FcsEntryData> entry);
        void AddEncyclopediaEntries(TechType techType,bool verbose);

        void UnRegisterDevice(FcsDevice miniFountainFilterController);
        Dictionary<string,FcsDevice> GetRegisteredDevicesOfId(string id);
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
        private static Dictionary<TechType, List<FcsEntryData>> _pdaEntries = new Dictionary<TechType, List<FcsEntryData>>();
        private static HashSet<TechType> _registeredTechTypes = new HashSet<TechType>();
        private List<string> _patchedMods = new List<string>();
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
        
        public void RegisterDevice(FcsDevice device, string tabID,string packageId)
        {
            var prefabID = device.GetPrefabID();

            QuickLogger.Debug($"Attempting to Register: {prefabID} : Contructed {device.IsConstructed}");

            if (string.IsNullOrWhiteSpace(prefabID) || !device.IsConstructed) return;
            
            if (!knownDevices.ContainsKey(prefabID))
            {
                var id = $"{tabID}{knownDevices.Count:D3}";
                device.UnitID = id;
                device.PackageId = packageId;
                knownDevices.Add(device.GetPrefabID(), id);
                AddToGlobalDevices(device, id);
                Mod.SaveDevices(knownDevices);
            }
            else
            {
                device.PackageId = packageId;
                device.UnitID=knownDevices[prefabID];
                AddToGlobalDevices(device, knownDevices[prefabID]);
            }

            QuickLogger.Debug($"Registering Device: {device.UnitID}");

            _registeredTechTypes.Add(device.GetTechType());

            BaseManagerSetup(device);
        }

        private static void AddToGlobalDevices(FcsDevice device, string id)
        {
            if (!GlobalDevices.ContainsKey(id))
            {
                GlobalDevices.Add(id, device);
            }
        }

        private static void BaseManagerSetup(FcsDevice device)
        {
            QuickLogger.Debug($"BMS: Device {device.UnitID}");
            if (string.IsNullOrEmpty(device.BaseId))
            {
                QuickLogger.Debug($"BMS: 1");
                var subRoot = device.gameObject.GetComponentInParent<SubRoot>();
                QuickLogger.Debug($"BMS: 2");
                if (subRoot != null)
                {
                    QuickLogger.Debug($"BMS: 3");
                    device.BaseId = subRoot.gameObject.GetComponent<PrefabIdentifier>().Id;
                    QuickLogger.Debug($"BMS: 4");
                }
                else
                {
                    QuickLogger.Error($"Failed to Setup the Base Manager for device {device.UnitID} with prefab id {device.GetPrefabID()}");
                    return;
                }
            }
            QuickLogger.Debug($"BMS: 5");
            var manager = BaseManager.FindManager(device.BaseId);
            QuickLogger.Debug($"Manager Returned: {manager?.BaseID}");
            if (manager != null)
            {
                device.Manager = manager;
                QuickLogger.Debug($"BMS: 6");
                manager.RegisterDevice(device);
                QuickLogger.Debug($"BMS: Device {device.UnitID}");
            }
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
            if (string.IsNullOrEmpty(unitID)) return;
            GlobalDevices.Remove(unitID);
        }

        public void CreateStoreEntry(TechType techType,TechType recieveTechType, decimal cost,StoreCategory category)
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

        public bool IsModPatched(string mod)
        {
            return _patchedMods.Contains(mod);
        }

        public void RegisterPatchedMod(string mod)
        {
            if(!_patchedMods.Contains(mod))
            {
                _patchedMods.Add(mod);
            }
        }

        public int GetRegisterModCount(string tabID)
        {
            return knownDevices.Count(x => x.Value.Equals(tabID));
        }

        public bool IsTechTypeRegistered(TechType techType)
        {
            return _registeredTechTypes.Contains(techType);
        }

        public void RegisterEncyclopediaEntry(TechType techType, List<FcsEntryData> entries)
        {
            LanguageHandler.SetLanguageLine($"EncyPath_fcs", "Field Creators Studios");

            if (!_pdaEntries.ContainsKey(techType))
            {
                _pdaEntries.Add(techType, entries);
            }
            else
            {
                _pdaEntries[techType] = entries;
            }
            
            foreach (FcsEntryData entryData in entries)
            {
                LanguageHandler.SetLanguageLine($"Ency_{entryData.key}", entryData.Title);
                LanguageHandler.SetLanguageLine($"EncyDesc_{entryData.key}", entryData.Description);

                PDAEncyclopediaHandler.AddCustomEntry(new PDAEncyclopedia.EntryData
                {
                    audio = entryData.audio,
                    image = entryData.image,
                    key = entryData.key,
                    nodes = entryData.nodes,
                    path = entryData.path,
                    popup = entryData.popup,
                    sound = entryData.sound,
                    timeCapsule = entryData.timeCapsule,
                    unlocked = entryData.unlocked
                });
            }
        }

        public void AddEncyclopediaEntries(TechType techType,bool verbose)
        {
            var node = new CraftNode("fcs", TreeAction.Expand)
            {
                string0 = "EncyPath_fcs",
                string1 = "Field Creators Studios"
            };
            PDAEncyclopedia.tree.AddNode(node);
            

            if (IsTechTypeRegistered(techType))
            {
                if (_pdaEntries.ContainsKey(techType))
                {
                    foreach (FcsEntryData entryData in _pdaEntries[techType])
                    {
                        PDAEncyclopedia.Add(entryData.key, verbose);
                    }
                }
            }
        }

        public void UnRegisterDevice(FcsDevice device)
        {
            knownDevices.Remove(device.GetPrefabID());
            _registeredTechTypes.Remove(device.GetTechType());
            RemoveDeviceFromGlobal(device.UnitID);
        }

        public Dictionary<string, FcsDevice> GetRegisteredDevicesOfId(string id)
        {
            return GlobalDevices.Where(x => x.Key.StartsWith(id, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
