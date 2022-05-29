using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    public class TabGroup : MonoBehaviour
    {
        public List<TabButton> tabButtons;
        public Color tabIdle;
        public Color tabHover;
        public Color tabActive;
        public TabButton selectedTab;
        public PanelGroup panelGroup;

        internal void Initialize()
        {
            StartActiveTab();
        }

        private void StartActiveTab()
        {
            if (selectedTab != null)
            {
                SetActiveTab(selectedTab);
            }
        }

        private void SetActiveTab(TabButton tabButton)
        {
            tabButton.Select();
        }

        public void Subscribe(TabButton button)
        {
            if (tabButtons == null)
            {
                tabButtons = new List<TabButton>();
            }

            tabButtons.Add(button);
        }

        public void OnTabEnter(TabButton button)
        {
            ResetTabs();
            if (selectedTab == null || button != selectedTab)
            {
                button.background.color = tabHover;
            }
        }

        public void OnTabExit(TabButton button)
        {
            ResetTabs();
        }

        public void OnTabSelected(TabButton button)
        {
            if (selectedTab != null)
            {
                selectedTab.Deselect();
            }

            selectedTab = button;

            selectedTab.Select();

            ResetTabs();

            button.background.color = tabActive;

            if (panelGroup != null)
            {
                panelGroup.SetPageIndex(button.transform.GetSiblingIndex());
            }
        }

        public void ResetTabs()
        {
            foreach (TabButton button in tabButtons)
            {
                if (selectedTab != null && selectedTab == button) continue;
                button.background.color = tabIdle;
            }
        }
    }
}
