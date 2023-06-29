using FCSCommon.Utilities;
using UnityEngine;
using FCS_AlterraHub.API;
using System.Collections.Generic;
using System;
using FCS_AlterraHub.Models;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using FCS_AlterraHub.Core.Extensions;
using FCS_AlterraHub.ModItems.FCSPDA.Data.Models;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;
using FCS_AlterraHub.Models.Abstract;

namespace FCS_AlterraHub.Core.Services;

/// <summary>
/// This class has all the methods and properties for maintaining the PDA Encyclopedia
/// </summary>
internal static class EncyclopediaService
{
    private static Dictionary<string, EncyclopediaData> _encyclopediaEntries = new();
    internal static bool IsRegisteringEncyclopedia { get; set; }
    internal static Action<FCSDevice> OnOpenEncyclopedia;
    private static TechType _selectedEntry;
    private static FCSDevice _currentDevice;

    public static Texture2D GetEncyclopediaTexture2D(string imageName, string bundleName = "")
    {
        QuickLogger.Debug($"Trying to find {imageName} in bundle {bundleName}");

        if (string.IsNullOrWhiteSpace(imageName)) return null;

        if (string.IsNullOrWhiteSpace(bundleName))
        {
            bundleName = FCSAssetBundlesService.PublicAPI.GlobalBundleName;
        }

        return FCSAssetBundlesService.PublicAPI.GetTextureByName(imageName, bundleName);
    }

    public static void GetEncyclopediaEntries(string modID)
    {
        try
        {
            QuickLogger.Debug("Get Encyclopedia Entries");

            if (ModRegistrationService.GetRegisteredMods().TryGetValue(modID, out ModPackData data))
            {
                var encyclopediaJson = Path.Combine(data.GetAssetPath(), "Encyclopedia", "EncyclopediaEntries.json");

                if (File.Exists(encyclopediaJson))
                {
                    QuickLogger.Debug("Encyclopedia Json Found");
                    var jsonData = JsonConvert.DeserializeObject<EncyclopediaData>(File.ReadAllText(encyclopediaJson));

                    _encyclopediaEntries.Add(jsonData.ModPackID, jsonData);

                    QuickLogger.Debug($"Count of Encyclopedia Entries: {_encyclopediaEntries.Count}");
                }
                else
                {
                    QuickLogger.Warning($"No encyclopedia file found for mod pack {modID}");
                }
            }
            else
            {
                QuickLogger.Error($"Failed to find {modID}! Registering Encyclopedia Data failed");
            }
        }
        catch (Exception e)
        {
            QuickLogger.Error(e.Message);
            QuickLogger.Error(e.StackTrace);
        }
    }

    internal static EncyclopediaData GetCategoryEntries(string category)
    {
        return _encyclopediaEntries[category];
    }

    internal static string GetModPackID(EncyclopediaEntryData entryData)
    {
        return _encyclopediaEntries.First(x => x.Value.Data.Contains(entryData)).Value?.ModPackID ?? string.Empty;
    }

    internal static void OpenEncyclopedia(FCSDevice device)
    {
        OnOpenEncyclopedia?.Invoke(device);
    }

    internal static EncyclopediaEntryData GetDataEntryByTechType(TechType techType)
    {
        foreach (var item in _encyclopediaEntries)
        {
            foreach (var data in item.Value.Data)
            {
                if (data.TechTypeString.ToTechType() == techType)
                {
                    return data;
                }
            }
        }
        return null;
    }

    internal static EncyclopediaData GetEntryByTechType(TechType techType)
    {
        foreach (var item in _encyclopediaEntries)
        {
            foreach (var data in item.Value.Data)
            {
                if (data.TechTypeString.ToTechType() == techType)
                {
                    return item.Value;
                }
            }
        }
        return null;
    }

    internal static void SetSelectedEntry(TechType techType)
    {
        _selectedEntry = techType;
    }

    internal static TechType GetSelectedEntry() { return _selectedEntry; }

    internal static void ClearSelectedEntry()
    {
        _selectedEntry = TechType.None;
    }

    internal static EncyclopediaTabController GetEncyclopediaTabController()
    {
        return FCSPDAController.Main.GetGUI().GetEncyclopediaTabController();
    }

    internal static Dictionary<string, EncyclopediaData> GetEntries()
    {
         return _encyclopediaEntries.OrderBy(x=>x.Value.Index).ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    internal static FCSDevice GetCurrentDevice()
    {
        return _currentDevice;
    }

    internal static void SetCurrentDevice(FCSDevice device)
    {
        _currentDevice = device;
    }

    internal static void ClearCurrentDevice()
    {
        _currentDevice = null;
    }
}