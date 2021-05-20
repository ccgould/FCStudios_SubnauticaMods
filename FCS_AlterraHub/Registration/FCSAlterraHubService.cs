using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Structs;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using QModManager.API;
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
        void AddEncyclopediaEntries(TechType techType, bool verbose);

        void UnRegisterDevice(FcsDevice device);
        Dictionary<string, FcsDevice> GetRegisteredDevicesOfId(string id);
        void RegisterTechLight(TechLight techLight);
        bool ChargeAccount(decimal amount);
        bool IsCreditAvailable(decimal amount);
        KeyValuePair<string, FcsDevice> FindDevice(string unitID);
        KeyValuePair<string, FcsDevice> FindDeviceWithPreFabID(string prefabID);
        void AddBuiltTech(TechType techType);
        int GetTechBuiltCount(TechType techType);
        void RemoveBuiltTech(TechType techType);
        TechType GetDeviceTechType(string deviceId);
        void RegisterBase(BaseManager manager);
        void UnRegisterBase(BaseManager manager);
        void CreateStoreEntry(TechType techType, TechType receiveTechType,int returnAmount, decimal cost, StoreCategory category,bool forceUnlocked = false);
    }

    internal interface IFCSAlterraHubServiceInternal
    {
    }

    public class FCSAlterraHubService : IFCSAlterraHubService, IFCSAlterraHubServiceInternal
    {
        private static readonly FCSAlterraHubService singleton = new FCSAlterraHubService();

        public static List<KnownDevice> knownDevices = new List<KnownDevice>();
        private static readonly Dictionary<string, FcsDevice> GlobalDevices = new Dictionary<string, FcsDevice>();
        private static Dictionary<TechType, FCSStoreEntry> _storeItems = new Dictionary<TechType, FCSStoreEntry>();
        private static Dictionary<TechType, List<FcsEntryData>> _pdaEntries = new Dictionary<TechType, List<FcsEntryData>>();
        private static HashSet<TechType> _registeredTechTypes = new HashSet<TechType>();
        private List<string> _patchedMods = new List<string>();
        private Dictionary<TechType, int> _globallyBuiltTech = new Dictionary<TechType, int>();
        public static IFCSAlterraHubService PublicAPI => singleton;
        internal static IFCSAlterraHubServiceInternal InternalAPI => singleton;

        private FCSAlterraHubService()
        {
            Mod.OnDevicesDataLoaded += OnDataLoaded;
        }

        private void OnDataLoaded(List<KnownDevice> obj)
        {
            if (obj != null)
                knownDevices = obj;

            //QModServices.Main.AddCriticalMessage($"Alterra Service Loaded: {knownDevices.Count} Devices");
        }

        public void RegisterDevice(FcsDevice device, string tabID, string packageId)
        {
            var prefabID = device.GetPrefabID();

            QuickLogger.Debug($"Attempting to Register: {prefabID} : Constructed {device.IsConstructed}");

            if (!device.BypassRegisterCheck)
            {
                if (string.IsNullOrWhiteSpace(prefabID) || !device.IsConstructed) return;
            }

            if (!knownDevices.Any(x => x.PrefabID.Equals(prefabID)))
            {
                QuickLogger.Debug($"Creating new ID: Known Count{knownDevices.Count}");
                var unitID = GenerateNewID(tabID, prefabID);
                device.TabID = tabID;
                device.UnitID = unitID;
                device.PackageId = packageId;
                AddToGlobalDevices(device, unitID);
                Mod.SaveDevices(knownDevices);
            }
            else
            {
                device.TabID = tabID;
                device.PackageId = packageId;
                device.UnitID = knownDevices.FirstOrDefault(x => x.PrefabID.Equals(prefabID)).ToString();
                AddToGlobalDevices(device, device.UnitID);
            }
            
            QuickLogger.Debug($"Registering Device: {device.UnitID}");

            _registeredTechTypes.Add(device.GetTechType());

            BaseManagerSetup(device);
        }

        private string GenerateNewID(string tabId, string prefabID)
        {
            var newEntry = new KnownDevice();
            var id = 0;
            if (knownDevices.Any())
            {
                id = knownDevices.Where(x => x.DeviceTabId.Equals(tabId)).DefaultIfEmpty().Max(x => x.ID);
                id++;
            }

            newEntry.ID = id;
            newEntry.PrefabID = prefabID;
            newEntry.DeviceTabId = tabId;
            knownDevices.Add(newEntry);
            return newEntry.ToString();
        }

        private static void AddToGlobalDevices(FcsDevice device, string unitID)
        {
            if (!GlobalDevices.ContainsKey(unitID))
            {
                GlobalDevices.Add(unitID, device);
            }
        }

        private static void BaseManagerSetup(FcsDevice device)
        {
            if (string.IsNullOrEmpty(device.BaseId))
            {
                var subRoot = device.gameObject.GetComponentInParent<SubRoot>();
                if (subRoot != null)
                {
                    device.BaseId = subRoot.gameObject.GetComponent<PrefabIdentifier>().Id;
                }
                else
                {
                    QuickLogger.Error($"Failed to Setup the Base Manager for device {device.UnitID} with prefab id {device.GetPrefabID()}");
                    return;
                }
            }

            var manager = BaseManager.FindManager(device.BaseId);
            if (manager != null)
            {
                device.Manager = manager;
                manager.RegisterDevice(device);
            }
        }

        private static void BaseManagerSetup(TechLight device)
        {
            string baseId = string.Empty;
            var subRoot = device.gameObject.GetComponentInParent<SubRoot>();
            if (subRoot != null)
            {
                baseId = subRoot.gameObject.GetComponent<PrefabIdentifier>().Id;
            }
            else
            {
                QuickLogger.Debug(
                    $"Failed to Setup the Base Manager for TechLight {device.gameObject.GetComponent<PrefabIdentifier>().Id}");
                return;
            }

            var manager = BaseManager.FindManager(baseId);
            if (manager != null)
            {
                manager.RegisterDevice(device);
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

        public void CreateStoreEntry(TechType techType, TechType recieveTechType, decimal cost, StoreCategory category)
        {
         CreateStoreEntry(techType,recieveTechType,1,cost,category);   
        }

        public void CreateStoreEntry(TechType techType, TechType receiveTechType, int returnAmount, decimal cost, StoreCategory category,bool forceUnlocked = false)
        {
            if (!_storeItems.ContainsKey(techType))
            {
                _storeItems.Add(techType,
                    new FCSStoreEntry
                    {
                        TechType = techType,
                        ReceiveTechType = receiveTechType,
                        Cost = cost,
                        ReturnAmount = returnAmount,
                        StoreCategory = category,
                        ForcedUnlock = forceUnlocked
                    });
            }
        }

        public Dictionary<TechType, FCSStoreEntry> GetRegisteredKits()
        {
            return _storeItems;
        }

        public TechType GetRegisteredKit(TechType techType)
        {
            return _storeItems.Any(x => x.Value.ReceiveTechType == techType)
                ? _storeItems.FirstOrDefault(x => x.Value.ReceiveTechType == techType).Key
                : TechType.None;
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
            if (!_patchedMods.Contains(mod))
            {
                _patchedMods.Add(mod);
            }
        }

        public int GetRegisterModCount(string tabID)
        {
            return knownDevices.Count(x => x.DeviceTabId.Equals(tabID));
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

        public void AddEncyclopediaEntries(TechType techType, bool verbose)
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
            foreach (KnownDevice knownDevice in knownDevices)
            {
                if (!string.IsNullOrEmpty(knownDevice.PrefabID) && knownDevice.PrefabID.Equals(device.GetPrefabID()))
                {
                    knownDevices.Remove(knownDevice);
                    _registeredTechTypes.Remove(device.GetTechType());
                    RemoveDeviceFromGlobal(device.UnitID);
                    break;
                }
            }
        }

        public Dictionary<string, FcsDevice> GetRegisteredDevicesOfId(string id)
        {
            return GlobalDevices.Where(x => x.Value.TabID.Equals(id, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public void RegisterTechLight(TechLight techLight)
        {
            BaseManagerSetup(techLight);
        }

        public bool ChargeAccount(decimal amount)
        {
            if (CardSystem.main.HasBeenRegistered())
            {
                CardSystem.main.RemoveFinances(amount);
                return true;
            }

            return false;
        }

        public bool IsCreditAvailable(decimal amount)
        {
            return CardSystem.main.HasEnough(amount);
        }

        public KeyValuePair<string, FcsDevice> FindDevice(string unitID)
        {
            return GlobalDevices.FirstOrDefault(x => x.Key.Equals(unitID, StringComparison.OrdinalIgnoreCase));
        }

        public KeyValuePair<string, FcsDevice> FindDeviceWithPreFabID(string prefabID)
        {
            return GlobalDevices.FirstOrDefault(x => x.Value.GetPrefabID().Equals(prefabID, StringComparison.OrdinalIgnoreCase));
        }

        public void AddBuiltTech(TechType techType)
        {
            if (_globallyBuiltTech.ContainsKey(techType))
            {
                _globallyBuiltTech[techType] += 1;
            }
            else
            {
                _globallyBuiltTech.Add(techType, 1);
            }
        }

        public int GetTechBuiltCount(TechType techType)
        {
            if (_globallyBuiltTech.ContainsKey(techType))
            {
                return _globallyBuiltTech[techType];
            }

            return 0;
        }

        public void RemoveBuiltTech(TechType techType)
        {
            if (_globallyBuiltTech.ContainsKey(techType))
            {
                _globallyBuiltTech[techType] -= 1;
                if (_globallyBuiltTech[techType] <= 0)
                {
                    _globallyBuiltTech.Remove(techType);
                }
            }
        }

        public TechType GetDeviceTechType(string deviceId)
        {
            return FindDevice(deviceId).Value.GetTechType();
        }

        public void RegisterBase(BaseManager manager)
        {
            QuickLogger.Debug($"Attempting to Register: {manager.BaseID}");

            if (!knownDevices.Any(x => x.PrefabID.Equals(manager.BaseID)))
            {
                manager.BaseFriendlyID = GenerateNewID("BS", manager.BaseID);
                Mod.SaveDevices(knownDevices);
            }
            else
            {
                manager.BaseFriendlyID = knownDevices.FirstOrDefault(x => x.PrefabID.Equals(manager.BaseID)).ToString();
            }

            QuickLogger.Debug($"Registering Device: {manager.BaseID}");
        }

        public void UnRegisterBase(BaseManager manager)
        {
            foreach (KnownDevice knownDevice in knownDevices)
            {
                if (!string.IsNullOrEmpty(knownDevice.PrefabID) && knownDevice.PrefabID.Equals(manager.BaseID))
                {
                    knownDevices.Remove(knownDevice);
                    break;
                }
            }
        }

    }
}