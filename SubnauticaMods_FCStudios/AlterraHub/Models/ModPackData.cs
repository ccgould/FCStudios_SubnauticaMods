using FCS_AlterraHub.Models.Structs;
using FCSCommon.Helpers;
using SMLHelper.Assets;
using SMLHelper.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FCS_AlterraHub.Models;

/// <summary>
/// Stores data about a mod
/// </summary>
internal class ModPackData
{
    private string _modName;
    private HashSet<Spawnable> _modItems = new();
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
    internal void AddModItem(Spawnable mod)
    {
        _modItems.Add(mod);
    }

    /// <summary>
    /// The name of the assetBundle
    /// </summary>
    /// <returns></returns>
    internal string GetBundleName()
    {
        return _assetBundleName;
    }

    internal Dictionary<string, FCSModItemSettings> GetSettings()
    {
        return FileSystemHelper.DeseriaizeSettings(GetModDirectory());
    }
}
