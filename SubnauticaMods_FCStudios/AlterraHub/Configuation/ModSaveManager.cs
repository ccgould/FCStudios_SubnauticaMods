using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using FCSCommon.Utilities;
using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine;
using UWE;
using Valve.VR;


namespace FCS_AlterraHub.Configuation
{
    internal static class ModSaveManager
    {
        private static ModSaver _saveObject;
        private static SaveData _saveData;
        public static void Save()
        {
            if (!IsSaving())
            {
                QuickLogger.Debug($"=================== Saving {Main.MODNAME} ===================");
                _saveObject = new GameObject().AddComponent<ModSaver>();

                _saveData = new SaveData();

                foreach (var controller in FCSModsAPI.PublicAPI.GetRegisteredDevices())
                {

                    QuickLogger.Debug($"ModID: {FCSModsAPI.PublicAPI.GetModPackID(controller.GetTechType())} || Target: {Main.MODNAME}");

                    if (FCSModsAPI.PublicAPI.GetModPackID(controller.GetTechType()) == Main.MODNAME)
                    {
                        QuickLogger.Debug($"Saving device: {controller.UnitID}");
                        ((IFCSSave<SaveData>)controller).Save(_saveData);
                    }
                }

                _saveData.StoreManagerSaveData = StoreManager.main.Save();
                _saveData.AccountDetails = AccountService.main.SaveDetails();
                FCSPDAController.Main.Save();
                _saveData.GamePlayService = GamePlayService.Main.Save();
                SaveLoadDataService.instance.SaveData(Main.MODNAME, _saveData, false, OnSaveComplete);

                QuickLogger.Debug($"=================== Saved {Main.MODNAME} ===================");
            }
        }

        public static void OnSaveComplete()
        {
            _saveObject?.StartCoroutine(SaveCoroutine());
        }

        public static bool IsSaving()
        {
            return _saveObject != null;
        }

        private static IEnumerator SaveCoroutine()
        {
            while (SaveLoadManager.main != null && SaveLoadManager.main.isSaving)
            {
                yield return null;
            }

            Object.DestroyImmediate(_saveObject.gameObject);
            _saveObject = null;
        }

        private static SaveData GetSaveData()
        {
            return _saveData ?? new SaveData();
        }

        internal static void LoadData()
        {
            QuickLogger.Info("Loading Save Data...");
            SaveLoadDataService.instance.LoadData<SaveData>(Main.MODNAME, false, (data) =>
            {
                _saveData = data;
                QuickLogger.Info("Save Data Loaded");

                if (!AccountService.main.IsLoaded)
                {
                    QuickLogger.Info("Save Data Loaded");
                    CoroutineHost.StartCoroutine(AccountService.main.Load(_saveData.AccountDetails));
                }

                GamePlayService.Main.LoadFromSave(_saveData.GamePlayService);
                StoreManager.main.LoadSave(_saveData.StoreManagerSaveData);
                FCSPDAController.Main.LoadFromSave();
            });
        }

        internal static T GetSaveData<T>(string id) where T : new()
        {
            LoadData();

            var saveData = GetSaveData();

            if (saveData is not null && !string.IsNullOrWhiteSpace(id))
            {
                foreach (JObject entry in saveData.Data)
                {
                    var tokenIdValue = entry.Value<string>("Id");
                    if (string.IsNullOrEmpty(tokenIdValue)) continue;

                    if (tokenIdValue.Equals(id))
                    {
                        var data = entry.ToObject<T>();
                        return data;
                    }
                }
            }
            return new T();
        }
    }
}
