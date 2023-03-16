using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Interfaces;
using FCSCommon.Utilities;
using SMLHelper.Assets;
using System;
using System.Collections.Generic;
using System.Reflection;
using static System.Xml.Xsl.Xslt.XslAstAnalyzer;

namespace FCS_AlterraHub.Core.Services;

/// <summary>
/// This class handles the registration and data of mods
/// </summary>
internal static class ModRegistrationService
{
    private static Dictionary<string, ModPackData> _registeredMods = new(); 

    /// <summary>
    /// Checks if the mod by the provided name exist in memory.
    /// </summary>
    /// <param name="modName">Thename of the mod to check for.</param>
    /// <returns>Returns <see cref="bool"/> of true if mod exists and false if it doesnt.</returns>
    internal static bool Exist(string modName)
    {
        if(string.IsNullOrWhiteSpace(modName)) return false;
        return _registeredMods.ContainsKey(modName);
    }

    internal static ModPackData GetModPackData(string modName)
    {
        _registeredMods.TryGetValue(modName, out ModPackData modData);
        return modData;
    }

    /// <summary>
    /// Registers a mod to be used in subnautica
    /// </summary>
    /// <param name="modName">Name of the mod.</param>
    /// <param name="assembly">The executing assembly.</param>
    /// <param name="mods">All the mods spawnables for patching.</param>
    internal static void Register(string modName, Assembly assembly,string assetBundleName)
    {
        if (Exist(modName))
        {
            QuickLogger.Warning($"The mod {modName} already has been registered. This operation has been canceled");
            return;
        };

        _registeredMods.Add(modName, new ModPackData(modName, assembly, assetBundleName));
        QuickLogger.Info($"Registered {modName}");
    }

    /// <summary>
    /// Registers an individual mod for a mod pack"/>
    /// </summary>
    /// <param name="modPackName">Name of the registered mod. Warning: ModPack must be registered first using <see cref="ModRegistrationService.Register(string, Assembly, string)"/></param>
    /// <param name="mod">The mod to patch</param>
    internal static void RegisterMod(string modPackName,Spawnable mod)
    {
        try
        {
            var modPack = GetModPackData(modPackName);
            modPack.AddModItem(mod);
            ((IModBase)mod).PatchSMLHelper();
        }
        catch (Exception e)
        {
            QuickLogger.Error($"Failed to patch mod item {mod.FriendlyName}");
            QuickLogger.Error(e.Message);
            QuickLogger.Error(e.StackTrace);
        }
    }
}
