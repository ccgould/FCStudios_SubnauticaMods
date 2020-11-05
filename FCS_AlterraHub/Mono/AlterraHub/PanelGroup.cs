using System;
using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    public class PanelGroup : MonoBehaviour
    {
        private GameObject[] panels = {};
        internal readonly List<PanelHelper> PanelHelpers = new List<PanelHelper>();
        public TabGroup TabGroup;
        public int panelIndex;

        internal void Initialize()
        {
            ShowCurrentPanel();
        }

        internal void LinkPanels(GameObject[] panelList)
        {
            panels = panelList;
            
            foreach (GameObject panel in panelList)
            {
                var storeGrid = GameObjectHelpers.FindGameObject(panel, "Grid");
                if (storeGrid != null)
                {
                    var panelHelper = panel.AddComponent<PanelHelper>();
                    var category = FindCategory(panel);
                    QuickLogger.Debug($"Found Category: {category} || Panel name: {panel.name}");
                    panelHelper.StoreCategory = category;
                    PanelHelpers.Add(panelHelper);
                }
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

            if (go.name.StartsWith("LifeSupport", StringComparison.OrdinalIgnoreCase))
            {
                return StoreCategory.LifeSupport;
            }

            if (go.name.StartsWith("Production", StringComparison.OrdinalIgnoreCase))
            {
                return StoreCategory.Production;
            }

            return go.name.StartsWith("Vehicles", StringComparison.OrdinalIgnoreCase) ? StoreCategory.Vehicles : StoreCategory.None;
        }

        private void ShowCurrentPanel()
        {
            for (int i = 0; i < panels.Length; i++)
            {
                panels[i].SetActive(i == panelIndex);
            }
        }

        public void SetPageIndex(int index)
        {
            panelIndex = index;
            ShowCurrentPanel();
        }
    }
}
