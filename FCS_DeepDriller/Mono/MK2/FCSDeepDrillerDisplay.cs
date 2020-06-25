using System;
using System.Collections.Generic;
using System.Linq;
using FCSCommon.Abstract;
using FCSCommon.Components;
using FCSCommon.Utilities;
using UnityEngine.UI;

namespace FCS_DeepDriller.Mono.MK2
{
    internal class FCSDeepDrillerDisplay : AIDisplay
    {
        private FCSDeepDrillerController _mono;
        private bool _isInitialized;
        private string _currentBiome;
        private bool IsOperational => _isInitialized && _mono != null && _mono.IsInitialized && !_mono.IsBeingDeleted;
        private HashSet<InterfaceButton> TrackedFilterItems = new HashSet<InterfaceButton>();

        internal void Setup(FCSDeepDrillerController mono)
        {
            _mono = mono;

            if (FindAllComponents())
            {
                _isInitialized = true;
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                
            }
        }

        public override bool FindAllComponents()
        {
            try
            {

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Has been caught:");
                QuickLogger.Error($"Message:\n {e.Message}");
                QuickLogger.Error($"StackTrace:\n {e.StackTrace}");
                return false;
            }

            return true;
        }

        public void UpdateBiome(string currentBiome)
        {
            if (!IsOperational) return;
            //TODO Change to the text component
            _currentBiome = currentBiome;
        }

        /// <summary>
        /// Updates the checked state of the focus items on the screen
        /// </summary>
        /// <param name="dataFocusOres"></param>
        internal void UpdateListItemsState(HashSet<TechType> dataFocusOres)
        {
            for (int dataFocusOresIndex = 0; dataFocusOresIndex < dataFocusOres.Count; dataFocusOresIndex++)
            {
                for (int trackedFilterItemsIndex = 0; trackedFilterItemsIndex < TrackedFilterItems.Count; trackedFilterItemsIndex++)
                {
                    var filterData = (FilterBtnData)TrackedFilterItems.ElementAt(trackedFilterItemsIndex).Tag;
                    if (filterData.TechType == dataFocusOres.ElementAt(dataFocusOresIndex))
                    {
                        filterData.Toggle.isOn = true;
                    }
                }
            }
        }
    }

    internal struct FilterBtnData
    {
        public TechType TechType { get; set; }
        public Toggle Toggle { get; set; }
    }
}
