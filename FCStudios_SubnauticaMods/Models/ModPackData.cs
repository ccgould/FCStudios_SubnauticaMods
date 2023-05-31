﻿using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Structs;
using FCSCommon.Helpers;
using Nautilus.Assets;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FCS_AlterraHub.Models;

/// <summary>
/// Stores data about a mod
/// </summary>
internal class ModPackData
{
    private string _modName;
    private Dictionary<string, ModBase> _modItems = new();
    private Assembly _assembly;
    private string _assetBundleName;

    /// <summary>
    /// Create a new ModData.
    /// </summary>
    /// <param name="modName">Name of the mod</param>
    /// <param name="assembly">The executing assembly</param>
    public ModPackData(string modName, Assembly assembly,string assetBundleName)
    {
        _modName = modName;
        _assembly = assembly;
        _assetBundleName = assetBundleName;
    }

    /// <summary>
    /// Gets the asset directory path on the system
    /// </summary>
    /// <returns><see cref="string"/> path</returns>
    public string GetAssetPath()
    {
        return Path.Combine(GetModDirectory(), "Assets");
    }

    /// <summary>
    /// Gets the mod directory path on the system
    /// </summary>
    /// <returns><see cref="string"/> path</returns>
    public string GetModDirectory()
    {
        return Path.GetDirectoryName(_assembly.Location);
    }

    /// <summary>
    /// Gets the save directory path on the system for the mod
    /// </summary>
    /// <returns><see cref="string"/> path</returns>
    public string GetSaveFileDirectory()
    {
        return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), _modName);
    }

    /// <summary>
    /// Adds a new mod to the ModData
    /// </summary>
    /// <param name="mod"></param>
    internal void AddModItem(string modID, ModBase mod)
    {
        _modItems.Add(modID,mod);
    }

    /// <summary>
    /// The name of the assetBundle
    /// </summary>
    /// <returns></returns>
    internal string GetBundleName()
    {
        return _assetBundleName;
    }

    internal string GetModID(TechType techType)
    {
        var result = _modItems.FirstOrDefault(x => x.Value.PrefabInfo.TechType == techType);

        if (result.Value is null) return string.Empty;

        return result.Key;
    }

    internal Dictionary<string, FCSModItemSettings> GetSettings()
    {
        return FileSystemHelper.DeseriaizeSettings(GetModDirectory());
    }

    internal ModBase GetSpawnable(TechType techType)
    {
        return _modItems.FirstOrDefault(x => x.Value.TechType == techType).Value;
    }

    internal bool HasMod(TechType techType)
    {
        return _modItems.Any(x=>x.Value.TechType == techType);
    }

    internal string GetModPackName()
    {
        return _modName;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"Mod Pack Name: {_modName}");
        sb.Append(Environment.NewLine);
        sb.Append($"Mod Pack Count: {_modItems.Count()}");
        sb.Append(Environment.NewLine);
        sb.Append($"Mod Pack Asset Path: {GetAssetPath()}");
        return sb.ToString();
    }
}