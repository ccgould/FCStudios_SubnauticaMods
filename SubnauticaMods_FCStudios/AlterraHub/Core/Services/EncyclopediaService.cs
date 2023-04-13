using FCSCommon.Utilities;
using static AssetBundleManager;
using UnityEngine;
using FCS_AlterraHub.API;
using FCS_AlterraHub.ModItems.FCSPDA.Data.Models;
using System.Collections.Generic;
using System;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models;
using SMLHelper.Handlers;
using SMLHelper.Utility;
using System.IO;
using Newtonsoft.Json;
using System.Xml.Schema;
using System.Linq;
using Discord;

namespace FCS_AlterraHub.Core.Services
{
    /// <summary>
    /// This class has all the methods and properties for maintaining the PDA Encyclopedia
    /// </summary>
    internal static class EncyclopediaService
    {
        internal static Dictionary<string, EncyclopediaData> EncyclopediaEntries { get; set; } = new();
        internal static bool IsRegisteringEncyclopedia { get; set; }

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

                        EncyclopediaEntries.Add(jsonData.ModPackID, jsonData);

                        QuickLogger.Debug($"Count of Encyclopedia Entries: {EncyclopediaEntries.Count}");
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
            return EncyclopediaEntries[category];
        }

        internal static string GetModPackID(EncyclopediaEntryData entryData)
        {
            return EncyclopediaEntries.First(x=>x.Value.Data.Contains(entryData)).Value?.ModPackID ?? string.Empty;
        }
    }
}
