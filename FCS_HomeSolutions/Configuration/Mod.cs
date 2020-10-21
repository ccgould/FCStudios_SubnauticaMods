using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_AlterraHomeSolutions.Configuration
{
    internal static class Mod
    {
        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private static List<PaintToolController> _registeredPaintTool;
        
        internal const string ModName = "HomeSolutions";
        internal const string ModFriendlyName = "Alterra Home Solutions";
        internal const string ModBundleName = "fcsalterrahomesolutions";
        internal const string PaintToolClassID = "PaintTool";
        internal const string PaintToolFriendly = "Alterra Paint Tool";
        internal const string PaintToolDescription = "Change the color of FCStudios mods";
        internal const string PaintToolPrefabName = "PaintTool";
        internal const string PaintToolKitClassID = "PaintTool_Kit";
        
        internal static Action<SaveData> OnDataLoaded { get; set; }

        internal static string SaveDataFilename => $"{ModName}SaveData.json";

        internal static string GetModDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        internal static string GetAssetPath()
        {
            return Path.Combine(GetModDirectory(), "Assets");
        }

        internal static string GetSaveFileDirectory()
        {
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModName);
        }

        private static void OnSaveComplete()
        {

        }
        
        internal static void Save()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var controllers = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IFCSSave<SaveData>>();

                foreach (PaintToolController controller in _registeredPaintTool)
                {
                    controller.Save(newSaveData);
                }

                foreach (var controller in controllers)
                {
                    controller.Save(newSaveData);
                }
                _saveData = newSaveData;

                ModUtils.Save<SaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
            }
        }

        internal static void LoadData()
        {
            QuickLogger.Info("Loading Save Data...");
            ModUtils.LoadSaveData<SaveData>(SaveDataFilename, GetSaveFileDirectory(), (data) =>
            {
                _saveData = data;
                QuickLogger.Info("Save Data Loaded");
                OnDataLoaded?.Invoke(_saveData);
            });
        }

        internal static bool IsSaving()
        {
            return _saveObject != null;
        }

        internal static SaveData GetSaveData()
        {
            return _saveData ?? new SaveData();
        }

        public static DecorationDataEntry GetDecorationDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.DecorationEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new DecorationDataEntry{ Id = id };
        }

        internal static PaintToolDataEntry GetPaintToolDataEntrySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.PaintToolEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new PaintToolDataEntry() { Id = id };
        }

        internal static void RegisterPaintTool(PaintToolController paintTool)
        {
            if (_registeredPaintTool == null)
            {
                QuickLogger.Debug($"Registered Paint tool: {paintTool.GetPrefabID()}");
                _registeredPaintTool = new List<PaintToolController>();
            }

            if (!_registeredPaintTool.Contains(paintTool))
            {
                _registeredPaintTool.Add(paintTool);
            }
                
        }

        internal static void UnRegisterPaintTool(PaintToolController paintTool)
        {
            if (_registeredPaintTool == null)
            {
                _registeredPaintTool = new List<PaintToolController>();
            }

            if (_registeredPaintTool.Contains(paintTool))
            {
                QuickLogger.Debug($"UnRegistered Paint tool: {paintTool.GetPrefabID()}");
                _registeredPaintTool.Remove(paintTool);
            }

        }
    }
}
