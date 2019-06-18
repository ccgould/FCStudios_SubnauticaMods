using FCSTechWorkBench.Mono;
using System.Collections.Generic;

namespace FCSTechWorkBench.Models
{
    /// <summary>
    /// Handles all filters data in the world
    /// </summary>
    internal class FilterManager
    {
        /// <summary>
        /// A list of all the filters in the world
        /// </summary>
        private readonly List<Filter> _globalFilters = new List<Filter>();

        /// <summary>
        /// Adds a new filter to the global list for tracking
        /// </summary>
        /// <param name="filter">The filter to add to the list.</param>
        internal void AddFilter(Filter filter)
        {
            _globalFilters.Add(filter);
        }

        /// <summary>
        /// Removes a filter from the Global list
        /// </summary>
        /// <param name="filter"></param>
        internal void RemoveFilter(Filter filter)
        {
            _globalFilters.Remove(filter);
        }

        internal List<Filter> SaveFilters()
        {
            return _globalFilters;
        }
    }
}
