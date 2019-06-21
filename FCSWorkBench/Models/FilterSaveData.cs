using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechWorkBench.Mono;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCSTechWorkBench.Models
{
    /// <summary>
    /// This class defines the a Filter to be saved in the FilterGlobal.json
    /// </summary>
    public class FilterSaveData
    {
        /// <summary>
        /// The PrefabId of the filter
        /// </summary>
        public string PrefabID { get; set; }

        /// <summary>
        /// The Remaining Time (MaxTime) of the filter
        /// </summary>
        public float RemainingTime { get; set; }

        /// <summary>
        /// The type of filter
        /// </summary>
        public FilterTypes FilterType { get; set; }

        /// <summary>
        /// The state of the filter
        /// </summary>
        public FilterState FilterState { get; set; }

        /// <summary>
        /// The Filter Object
        /// </summary>
        [JsonIgnore] public Filter Filter { get; set; }
        [JsonIgnore] public WorkBenchFilter WFilter { get; set; }
        [JsonIgnore] public Pickupable Pickupable { get; set; }
        public TechType TechType { get; set; }

        /// <summary>
        /// Iniializes the class by using the filter to fill in the properties
        /// </summary>
        /// <param name="filter"></param>
        public void Initialize(Filter filter)
        {
            if (filter == null) return;

            QuickLogger.Debug("Initialize FileSaveData");

            Filter = filter;
            FilterState = filter.FilterState;
            FilterType = filter.FilterType;
            RemainingTime = filter.MaxTime;
            PrefabID = filter.PrefabId.Id;
        }

        /// <summary>
        /// Refresh the class with the new information from the filter
        /// </summary>
        public void RefreshData()
        {
            if (Filter == null)
            {
                QuickLogger.Error($"Filter returned null on refresh");
                return;
            }
            FilterState = Filter.FilterState;
            FilterType = Filter.FilterType;
            RemainingTime = Filter.MaxTime;
            PrefabID = Filter.PrefabId.Id;
        }

        /// <summary>
        /// Creates a new filter from the FilterGlobal.json file
        /// </summary>
        /// <param name="go">The gameobject to apply the correct filter component</param>
        /// <param name="filterSave">The save data loaded from the FilterGlobal.json</param>
        /// <returns></returns>
        public Filter CreateNewFilterFromGlobal(GameObject go, FilterSaveData filterSave)
        {
            QuickLogger.Debug("In CreateNewFilter");

            if (go == null)
            {
                QuickLogger.Error("GameObject is null");
                return null;
            }

            WFilter = go.GetComponent<WorkBenchFilter>();

            Pickupable = go.GetComponent<Pickupable>();

            switch (filterSave.FilterType)
            {
                case FilterTypes.None:
                    return null;

                case FilterTypes.LongTermFilter:
                    var lFilter = go.AddComponent<LongTermFilterController>();
                    lFilter.Initialize(true);
                    lFilter.FilterType = filterSave.FilterType;
                    //lFilter.PrefabId.Id = filterSave.PrefabID;
                    lFilter.FilterState = filterSave.FilterState;
                    lFilter.MaxTime = filterSave.RemainingTime;
                    Filter = lFilter;
                    RefreshData();
                    return lFilter;

                case FilterTypes.ShortTermFilter:
                    var sFilter = go.AddComponent<ShortTermFilterController>();
                    sFilter.Initialize(true);
                    sFilter.FilterType = filterSave.FilterType;
                    //sFilter.PrefabId.Id = filterSave.PrefabID;
                    sFilter.FilterState = filterSave.FilterState;
                    sFilter.MaxTime = filterSave.RemainingTime;
                    Filter = sFilter;
                    RefreshData();
                    return sFilter;
                default:
                    QuickLogger.Error("Filter Type not found.");
                    return null;

            }
        }
    }
}
