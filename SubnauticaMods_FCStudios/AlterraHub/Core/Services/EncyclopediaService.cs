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

namespace FCS_AlterraHub.Core.Services
{
    /// <summary>
    /// This class has all the methods and properties for maintaining the PDA Encyclopedia
    /// </summary>
    internal static class EncyclopediaService
    {
        internal static List<Dictionary<string, List<EncyclopediaEntryData>>> EncyclopediaEntries { get; set; } = new();
        internal static bool IsRegisteringEncyclopedia { get; set; }

        public static void RegisterEncyclopediaEntries(List<Dictionary<string, List<EncyclopediaEntryData>>> encyclopediaEntries)
        {
            LanguageHandler.SetLanguageLine($"EncyPath_fcs", "Field Creators Studios");

            foreach (Dictionary<string, List<EncyclopediaEntryData>> entry in encyclopediaEntries)
            {
                foreach (KeyValuePair<string, List<EncyclopediaEntryData>> data in entry)
                {
                    foreach (EncyclopediaEntryData entryData in data.Value)
                    {
                        LanguageHandler.SetLanguageLine($"Ency_{data.Key}", entryData.Title);
                        LanguageHandler.SetLanguageLine($"EncyDesc_{data.Key}", entryData.Body);
                        LanguageHandler.SetLanguageLine($"EncyPath_{entryData.Path}", entryData.TabTitle);

                        FMODAsset fModAsset = null;

                        if (!string.IsNullOrWhiteSpace(entryData.AudioName))
                        {
                            if (ModRegistrationService.GetModPackData(entryData.ModPackID, out ModPackData modPackData))
                            {
                                var audioPath = Path.Combine(modPackData.GetAssetPath(), "Audio", $"{entryData.AudioName}.mp3");
                                if (File.Exists(audioPath))
                                {
                                    fModAsset = FModHelpers.CreateFmodAsset(string.Empty, audioPath);

                                    CustomSoundHandler.RegisterCustomSound(fModAsset.id, fModAsset.path, AudioUtils.BusPaths.PDAVoice);
                                }
                                else
                                {
                                    QuickLogger.Error($"Failed to located audio path: {audioPath} for entry {entryData.Path}");
                                }
                            }

                        }

                        QuickLogger.Debug($"Registering entry {data.Key}");

                        PDAEncyclopedia.mapping.Add(data.Key, new PDAEncyclopedia.EntryData
                        {
                            audio = fModAsset,
                            image = GetEncyclopediaTexture2D(entryData.ImageName, ModRegistrationService.GetModPackData(entryData.ModPackID).GetBundleName()),
                            key = data.Key,
                            nodes = PDAEncyclopedia.ParsePath(entryData.Path),
                            path = entryData.Path,
                            unlocked = entryData.Unlocked
                        });

                        if (!entryData.Unlocked)
                        {
                            if (TechTypeExtensions.FromString(entryData.UnlockedBy, out TechType techType, true))
                            {
                                TechType bluePrintTechType = TechType.None;
                                if (!string.IsNullOrWhiteSpace(entryData.Blueprint))
                                {
                                    TechTypeExtensions.FromString(entryData.Blueprint, out bluePrintTechType, true);
                                }

                                PDAHandler.AddCustomScannerEntry(new PDAScanner.EntryData()
                                {
                                    encyclopedia = data.Key,
                                    locked = true,
                                    key = techType,
                                    blueprint = bluePrintTechType,
                                    destroyAfterScan = entryData.DestroyFragmentAfterScan,
                                    isFragment = entryData.HasFragment,
                                    totalFragments = entryData.TotalFragmentsToUnlock,
                                    scanTime = entryData.ScanTime
                                });
                            }
                            else
                            {
                                QuickLogger.Error($"Failed to Parse TechType {entryData.UnlockedBy} to unlock Ency entry: {data.Key}", true);
                            }
                        }
                    }
                }
            }
        }

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
    }
}
