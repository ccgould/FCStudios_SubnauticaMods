using FCSCommon.Utilities;
using FCSTechWorkBench.Models;
using FCSTechWorkBench.Mono;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ARS_SeaBreezeFCS32.Model
{
    /// <summary>
    /// Handles all filters data in the world
    /// </summary>
    internal static class FilterManager
    {
        private static bool _isLoaded = false;

        private static readonly string SaveDirectory = Path.Combine(SaveUtils.GetCurrentSaveDataDir(), "ARSSeaBreezeFCS32");
        private static string SaveFile => Path.Combine(SaveDirectory, "FilterGlobal.json");

        private static List<FilterSaveData> _globalFilters = new List<FilterSaveData>();

        /// <summary>
        /// Adds a new filter to the global list for tracking
        /// </summary>
        /// <param name="filter">The filter to add to the list.</param>
        internal static void AddFilter(Filter filter)
        {
            QuickLogger.Debug("Adding Filter");

            var match = _globalFilters.Where(x => x.PrefabID == filter.PrefabId.Id).DefaultIfEmpty(null).Single();

            if (match != null) return;

            QuickLogger.Debug("No Match Found");

            var item = new FilterSaveData
            {
                PrefabID = filter.PrefabId.Id,
                FilterState = filter.FilterState,
                FilterType = filter.FilterType,
                RemainingTime = filter.MaxTime
            };

            item.Initialize(filter);

            _globalFilters.Add(item);

            QuickLogger.Debug($"Global Filters Count: {_globalFilters.Count}");
        }

        /// <summary>
        /// Removes a filter from the Global list
        /// </summary>
        /// <param name="filter"></param>
        internal static void RemoveFilter(Filter filter)
        {
            var match = _globalFilters.Single(x => x.PrefabID == filter.PrefabId.Id);
            _globalFilters.Remove(match);
        }

        /// <summary>
        /// Saves all filters to the GlobalFilters.json
        /// </summary>
        internal static void SaveFilters()
        {
            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            foreach (FilterSaveData filterSaveData in _globalFilters)
            {
                filterSaveData.RefreshData();
            }

            //TODO save file
            var output = JsonConvert.SerializeObject(_globalFilters, Formatting.Indented);
            File.WriteAllText(SaveFile, output);

            QuickLogger.Debug($"Saved Filters Global Data");
        }

        /// <summary>
        /// Gets the filter data from the global filters list
        /// </summary>
        /// <param name="prefabId">The filter id to find</param>
        /// <returns>A Filter Save Data</returns>
        public static FilterSaveData GetFilterData(string prefabId)
        {
            return _globalFilters.Where(x => x.PrefabID == prefabId).DefaultIfEmpty(null).Single();
        }

        /// <summary>
        /// Loads the GlobalFilter.json
        /// </summary>
        internal static void LoadSave()
        {
            if (_isLoaded) return;

            if (!File.Exists(SaveFile)) return;

            string savedDataJson = File.ReadAllText(SaveFile).Trim();

            _globalFilters = JsonConvert.DeserializeObject<List<FilterSaveData>>(savedDataJson);

            QuickLogger.Debug($"Global Filters Count: {_globalFilters.Count}");
        }
    }
}
