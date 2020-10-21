using System.Collections.Generic;
using FCSCommon.Utilities;

namespace FCS_AlterraHub.Patch
{
    internal class PDATabPatcher
    {
        private const string TechTypeEnumName = "PDATab";
        internal static readonly int startingIndex = 7;
        internal static readonly List<int> bannedIndices = new List<int>();

        internal static readonly EnumCacheManager<PDATab> cacheManager =
            new EnumCacheManager<PDATab>(
                enumTypeName: TechTypeEnumName,
                startingIndex: startingIndex,
                bannedIDs: bannedIndices);

        internal static PDATab AddPDATab(string name)
        {
            EnumTypeCache cache = cacheManager.RequestCacheForTypeName(name);

            if (cache == null)
            {
                cache = new EnumTypeCache()
                {
                    Name = name,
                    Index = cacheManager.GetNextAvailableIndex()
                };
            }

            if (cacheManager.IsIndexAvailable(cache.Index))
                cache.Index = cacheManager.GetNextAvailableIndex();

            var techType = (PDATab)cache.Index;
            cacheManager.Add(techType, cache.Index, cache.Name);
            
            QuickLogger.Debug($"Successfully added Tech Type: '{name}' to Index: '{cache.Index}'");
            return techType;
        }

        private static List<int> PreRegisteredTechTypes()
        {
            // Make sure to exclude already registered TechTypes.
            // Be aware that this approach is still subject to race conditions.
            // Any mod that patches after this one will not be picked up by this method.
            // For those cases, there are additional ways of excluding these IDs.

            var bannedIndices = new List<int>();

            //Dictionary<string, PDATab> knownTechTypes = TechTypeExtensions.keyTechTypes;
            //foreach (PDATab knownTechType in knownTechTypes.Values)
            //{
            //    int currentTechTypeKey = (int)knownTechType;

            //    if (currentTechTypeKey < startingIndex)
            //        continue; // This is possibly a default TechType,
            //    // Anything below this range we won't ever assign

            //    if (bannedIndices.Contains(currentTechTypeKey))
            //        continue; // Already exists in list

            //    bannedIndices.Add(currentTechTypeKey);
            //}

            //if (bannedIndices.Count > 0)
            //    QuickLogger.Debug($"Finished known TechTypes exclusion. {bannedIndices.Count} IDs were added in ban list.");

            return bannedIndices;
        }

        internal static void Patch()
        {
            //IngameMenuHandler.Main.RegisterOneTimeUseOnSaveEvent(() => cacheManager.SaveCache());

            //Logger.Log($"Added {cacheManager.ModdedKeysCount} TechTypes succesfully into the game.", LogLevel.Info);

            //Logger.Log("TechTypePatcher is done.", LogLevel.Debug);
        }

    }
}