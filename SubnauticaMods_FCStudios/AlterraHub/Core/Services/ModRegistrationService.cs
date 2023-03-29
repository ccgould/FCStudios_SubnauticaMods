using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCSCommon.Utilities;
using SMLHelper.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace FCS_AlterraHub.Core.Services;

/// <summary>
/// This class handles the registration and data of mods
/// </summary>
internal static class ModRegistrationService
{
    private static readonly Dictionary<string, ModPackData> _registeredMods = new();
    private static Dictionary<TechType, Spawnable> modNameCache = new();

    /// <summary>
    /// Checks if the modPack by the provided name exist in memory.
    /// </summary>
    /// <param name="modPackName">The name of the mod to check for.</param>
    /// <returns>Returns <see cref="bool"/> of true if mod exists and false if it doesnt.</returns>
    internal static bool Exist(string modPackName)
    {
        if(string.IsNullOrWhiteSpace(modPackName)) return false;
        return _registeredMods.ContainsKey(modPackName);
    }

    internal static bool GetModPackData(string modPackName, out ModPackData data)
    {
        data = null;
        var result = GetModPackData(modPackName);
        if (result is not null)
        {
            data = result;
            return true;
        }
        return false;
    }

    internal static ModPackData GetModPackData(string modPackName)
    {
        _registeredMods.TryGetValue(modPackName, out ModPackData modData);
        return modData;
    }

    /// <summary>
    /// Registers a mod to be used in subnautica
    /// </summary>
    /// <param name="modPackName">Name of the mod pack.</param>
    /// <param name="assembly">The executing assembly.</param>
    /// <param name="mods">All the mods spawnables for patching.</param>
    internal static void Register(string modPackName, Assembly assembly,string assetBundleName)
    {
        if (Exist(modPackName))
        {
            QuickLogger.Warning($"The mod {modPackName} already has been registered. This operation has been canceled");
            return;
        };

        _registeredMods.Add(modPackName, new ModPackData(modPackName, assembly, assetBundleName));
        QuickLogger.Info($"Registered {modPackName}");
    }

    /// <summary>
    /// Registers an individual mod for a mod pack"/>
    /// </summary>
    /// <param name="modPackName">Name of the registered mod. Warning: ModPack must be registered first using <see cref="ModRegistrationService.Register(string, Assembly, string)"/></param>
    /// <param name="modID">The ID of the mod. Will be used in the default name e.g DD001</param>
    /// <param name="mod">The mod to patch</param>
    internal static void RegisterMod(string modPackName,string modID,Spawnable mod)
    {
        try
        {
            var modPack = GetModPackData(modPackName);
            modPack.AddModItem(modID,mod);
            ((IModBase)mod).PatchSMLHelper();
        }
        catch (Exception e)
        {
            QuickLogger.Error($"Failed to patch mod item {mod.FriendlyName}");
            QuickLogger.Error(e.Message);
            QuickLogger.Error(e.StackTrace);
        }
    }

    internal static string GetModID(TechType techType)
    {
        var result = _registeredMods.FirstOrDefault(x => x.Value.HasMod(techType) == true);

        if (result.Value is null) return string.Empty;

        return result.Value.GetModID(techType);
    }

    internal static string GetModName(FCSDevice device)
    {
        var techType = device.GetTechType();
        if (modNameCache.ContainsKey(techType))
        {
            return modNameCache[techType].FriendlyName;
        }

        var result = _registeredMods.FirstOrDefault(x => x.Value.HasMod(techType) == true);

        if(result.Value is null) return string.Empty;

        var spawnable = result.Value.GetSpawnable(techType);
        

        modNameCache.Add(techType,spawnable);

        return spawnable.FriendlyName;
    }

    internal static Dictionary<string, ModPackData> GetRegisteredMods()
    {
        return _registeredMods;
    }

    internal static string GetModPackID(TechType techType)
    {
        foreach (var item in _registeredMods)
        {
            if(item.Value.HasMod(techType))
            {
                return item.Key;
            }
        }

        return string.Empty;
    }
}
