using System;
using System.Linq;
using FCSCommon.Abstract;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using MAC.OxStation.Buildables;
using MAC.OxStation.Display;
using MAC.OxStation.Mono;
using UnityEngine;
using UnityEngine.UI;

namespace MAC.OxStation.Managers
{
    internal class ScreenDisplayManager : AIDisplay
    {
        private Text _damageAmountText;
        private OxStationScreenController _mono;
        private GridHelper _oxGrid;
        private Color _pingBTNStartColor = new Color(0f, 0.8745099f,1f);
        private Color _pingBTNHoverColor = Color.cyan;
        private int _page;

        public override void OnButtonClick(string btnName, object tag)
        {
            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "PingBTN":
                    var ox = (OxStationController) tag;
                    QuickLogger.Debug($"Pinging: {ox.GetPrefabIDString()}");
                    ox.TogglePinging();
                    break;
            }
        }

        public override bool FindAllComponents()
        {
            try
            {
                #region Canvas  
                var canvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

                if (canvasGameObject == null)
                {
                    QuickLogger.Error("Canvas cannot be found");
                    return false;
                }
                #endregion

                #region Damage Screen

                var damageScreen = InterfaceHelpers.FindGameObject(canvasGameObject, "DamagesScreen");

                #endregion

                #region Damage Amount Text

                _damageAmountText = InterfaceHelpers.FindGameObject(damageScreen, "DamageAmount").GetComponent<Text>();

                #endregion

                #region Ping BTN

                var pingBTN = InterfaceHelpers.FindGameObject(damageScreen, "PingBTN");
                var pingBTNText = pingBTN?.GetComponentInChildren<Text>();
                pingBTNText.text = "PING ALL";

                #endregion

                #region Info Screen

                var infoScreen = InterfaceHelpers.FindGameObject(canvasGameObject, "InfoScreen");

                #endregion

                #region Grid

                var grid = infoScreen?.GetComponentInChildren<GridLayoutGroup>().gameObject;
                _oxGrid = gameObject.AddComponent<GridHelper>();
                _oxGrid.Setup(6, OxStationModelPrefab.ItemPrefab, infoScreen, Color.black, Color.white, OnButtonClick,
                    5, "PrevBTN", "NextBTN", "Grid", "Paginator", string.Empty);
                _oxGrid.OnLoadDisplay += OnLoadDisplay;
                
                #endregion
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return false;
            }

            return true;
        }

        private void OnLoadDisplay(DisplayData info)
        {
            _oxGrid.ClearPage();

            var grouped = _mono.TrackedDevices.ToList();

            if (info.EndPosition > grouped.Count)
            {
                info.EndPosition = grouped.Count;
            }

            for (int i = info.StartPosition; i < info.EndPosition; i++)
            {
                GameObject buttonPrefab = Instantiate(info.ItemsPrefab);

                if (buttonPrefab == null || info.ItemsGrid == null)
                {
                    if (buttonPrefab != null)
                    {
                        Destroy(buttonPrefab);
                    }

                    return;
                }
                
                buttonPrefab.transform.SetParent(info.ItemsGrid.transform, false);
                //buttonPrefab.GetComponentInChildren<Text>().text = grouped[i].Value.ToString();
               
                var mainButton = buttonPrefab.FindChild("PingBTN");
                var mainBTN = mainButton.AddComponent<OxStationItemButton>();
                mainBTN.Tag = grouped[i].Value;
                mainBTN.OxstationOxygen = InterfaceHelpers.FindGameObject(buttonPrefab, "Oxygen_Preloader_Bar_Front").GetComponent<Image>();
                mainBTN.OxstationHealth = InterfaceHelpers.FindGameObject(buttonPrefab, "Health_Preloader_Bar_Front").GetComponent<Image>();
                mainBTN.UpdateDisplay();
                mainBTN.ButtonMode = InterfaceButtonMode.Background;
                mainBTN.STARTING_COLOR = _pingBTNStartColor;
                mainBTN.HOVER_COLOR = _pingBTNHoverColor;
                mainBTN.OnButtonClick = OnButtonClick;
                mainBTN.BtnName = "PingBTN";
            }

            _oxGrid.UpdaterPaginator(grouped.Count);
        }

        internal void UpdateDamageAmount(int amount)
        {
            _damageAmountText.text = amount.ToString();
        }

        internal void Setup(OxStationScreenController mono)
        {
            _mono = mono;
            _page = Animator.StringToHash("Page");
            if (FindAllComponents())
            {
                PowerOnDisplay();
                _oxGrid.DrawPage(1);
            }
        }

        internal void UpdateDisplay()
        {
            _oxGrid.DrawPage();
        }

        public override void PowerOnDisplay()
        {
            _mono.AnimationManager.SetIntHash(_page,1);
        }

        internal void GoToAlertPage(int amount)
        {
            _damageAmountText.text = amount.ToString();
            _mono.AnimationManager.SetIntHash(_page, 2);
        }

        internal void ResetDisplay()
        {
            PowerOnDisplay();
        }
    }
}
