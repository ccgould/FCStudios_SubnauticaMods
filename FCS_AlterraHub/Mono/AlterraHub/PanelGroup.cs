using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    public class PanelGroup : MonoBehaviour
    {
        private Dictionary<GameObject,PanelHelper> _panels = new Dictionary<GameObject, PanelHelper>();
        internal List<PanelHelper> PanelHelpers { get;} = new List<PanelHelper>();
        public TabGroup TabGroup;
        public int panelIndex;

        internal void Initialize()
        {
            ShowCurrentPanel();
            KnownTech.onAdd += KnownTechOnOnAdd;
        }

        private void KnownTechOnOnAdd(TechType techtype, bool verbose)
        {
            ShowCurrentPanel();
        }

        internal void LinkPanels(GameObject[] panelList)
        {
           
            foreach (GameObject panel in panelList)
            {
                var storeGrid = GameObjectHelpers.FindGameObject(panel, "Grid");

                if (storeGrid != null)
                {
                    var panelHelper = panel.AddComponent<PanelHelper>();
                    var category = FindCategory(panel);
                    //QuickLogger.Debug($"Found Category: {category} || Panel name: {panel.name}");
                    panelHelper.StoreCategory = category;
                    PanelHelpers.Add(panelHelper);
                    _panels.Add(panel,panelHelper);
                    continue;
                }
                _panels.Add(panel,null);
            }
        }

        private StoreCategory FindCategory(GameObject go)
        {
            if (go.name.StartsWith("Home", StringComparison.OrdinalIgnoreCase))
            {
                return StoreCategory.Home;
            }

            if (go.name.StartsWith("Energy", StringComparison.OrdinalIgnoreCase))
            {
                return StoreCategory.Energy;
            }

            if (go.name.StartsWith("Life", StringComparison.OrdinalIgnoreCase))
            {
                return StoreCategory.LifeSupport;
            }

            if (go.name.StartsWith("Production", StringComparison.OrdinalIgnoreCase))
            {
                return StoreCategory.Production;
            }

            if (go.name.StartsWith("Storage", StringComparison.OrdinalIgnoreCase))
            {
                return StoreCategory.Storage;
            }

            if (go.name.StartsWith("Misc", StringComparison.OrdinalIgnoreCase))
            {
                return StoreCategory.Misc;
            }

            return go.name.StartsWith("Vehicle", StringComparison.OrdinalIgnoreCase) ? StoreCategory.Vehicles : StoreCategory.None;
        }

        private void ShowCurrentPanel()
        {
            for (int i = 0; i < _panels.Count; i++)
            {
                var page = _panels.ElementAt(i);
                page.Key.SetActive(i == panelIndex);

                if (page.Value != null)
                {
                    page.Value.RefreshStoreItems();
                }
            }
        }

        public void SetPageIndex(int index)
        {
            panelIndex = index;
            ShowCurrentPanel();
        }
    }
}
