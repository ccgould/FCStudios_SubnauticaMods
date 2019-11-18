using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FCSCommon.Interfaces;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSCommon.Abstract
{
    //public abstract class ModAB<TSaveData> where TSaveData : ISaveData, new()   
    //{
    //    private static ModSaver _saveObject;
    //    private static TSaveData _saveData;

    //    public abstract string ClassID { get;}
    //    public abstract string ModName { get; }
    //    public abstract string ModFriendlyName { get; }
    //    public abstract string ModDescription { get; }
    //    public virtual string SaveDataFilename => $"{ModName}SaveData.json";
    //    public abstract string GameObjectName { get; }
    //    public string ModFolderName => $"FCS_{ModName}";
    //    public abstract string BundleName { get; }

    //    #region Save

    //    public virtual void SaveMod<TController>() where TController : FCSController
    //    {
    //        if (!IsSaving())
    //        {
    //            _saveObject = new GameObject().AddComponent<ModSaver>();

    //            TSaveData newSaveData = new TSaveData();

    //            var drills = GameObject.FindObjectsOfType<TController>();

    //            foreach (var drill in drills)
    //            {
    //                drill.Save(newSaveData);
    //            }

    //            _saveData = newSaveData;

    //            ModUtils.Save<TSaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
    //        }
    //    }

    //    public virtual void LoadData()
    //    {
    //        QuickLogger.Info("Loading Save Data...");
    //        ModUtils.LoadSaveData<TSaveData>(SaveDataFilename, GetSaveFileDirectory(), (data) =>
    //        {
    //            _saveData = data;
    //            QuickLogger.Info("Save Data Loaded");
    //            OnDataLoaded?.Invoke(_saveData);
    //        });
    //    }

    //    public static TSaveData GetSaveData()
    //    {
    //        return _saveData != null ? _saveData : new TSaveData();
    //    }

    //    //public virtual ISaveDataEntry GetSaveData(string id)
    //    //{
    //    //    try
    //    //    {
    //    //        LoadData();

    //    //        var saveData = GetSaveData();

    //    //        foreach (var entry in saveData.Entries)
    //    //        {
    //    //            if (entry.Id == id)
    //    //            {
    //    //                return entry;
    //    //            }
    //    //        }
    //    //    }
    //    //    catch (Exception e)
    //    //    {
    //    //        QuickLogger.Error(e.Message);
    //    //    }
    //    //    return new ISaveDataEntry() { Id = id };
    //    //}
    //    #endregion

    //    public virtual void OnSaveComplete()
    //    {
    //        _saveObject.StartCoroutine(SaveCoroutine());
    //    }

    //    private static IEnumerator SaveCoroutine()
    //    {
    //        while (SaveLoadManager.main != null && SaveLoadManager.main.isSaving)
    //        {
    //            yield return null;
    //        }
    //        GameObject.DestroyImmediate(_saveObject.gameObject);
    //        _saveObject = null;
    //    }

    //    public static bool IsSaving()
    //    {
    //        return _saveObject != null;
    //    }

    //    /// <summary>
    //    /// The location of the QMods directory
    //    /// </summary>
    //    public static string QMODFOLDER { get; } = Path.Combine(Environment.CurrentDirectory, "QMods");

    //    public virtual string MODFOLDERLOCATION => GetModPath();

    //    public static Action<TSaveData> OnDataLoaded { get; private set; }

    //    private static string GetQModsPath()
    //    {
    //        return Path.Combine(Environment.CurrentDirectory, "QMods");
    //    }

    //    public virtual string GetModPath()
    //    {
    //        return Path.Combine(GetQModsPath(), ModFolderName);
    //    }

    //    public virtual string GetAssetPath()
    //    {
    //        return Path.Combine(GetModPath(), "Assets");
    //    }

    //    public virtual string GetModInfoPath()
    //    {
    //        return Path.Combine(GetModPath(), "mod.json");
    //    }

    //    public static string GetGlobalBundle()
    //    {
    //        return Path.Combine(Path.Combine(QMODFOLDER, "FCSTechWorkBench"), "globalmaterials");
    //    }

    //    public virtual string GetConfigPath()
    //    {
    //        return Path.Combine(GetModPath(), "Configurations");
    //    }

    //    public virtual string GetLanguagePath()
    //    {
    //        return Path.Combine(GetModPath(), "Language");

    //    }

    //    public virtual string GetConfigFile(string modName)
    //    {
    //        return Path.Combine(GetConfigPath(), $"{modName}.json"); ;
    //    }

    //    public static string GetSaveFileDirectory()
    //    {
    //        return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModName);
    //    }
    //}
}
