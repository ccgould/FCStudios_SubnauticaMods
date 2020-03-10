using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSCommon.Abstract;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Managers;
using GasPodCollector.Buildables;
using UnityEngine;
using UnityEngine.UI;

namespace GasPodCollector.Mono.Managers
{
    internal class GasopodCollectorDisplayManager: AIDisplay
    {
        private GaspodCollectorController _mono;
        private Text _amountOfPodsCount;
        private Color _startColor = Color.white;
        private Color _hoverColor = new Color(0.07f, 0.38f, 0.7f, 1f);
        private Color _fireBrickColor = new Color(0.6980392f, 0.1333333f, 0.1333333f, 1f);
        private ColorManager _colorPage;

        internal void Setup(GaspodCollectorController mono)
        {
            _mono = mono;
            _colorPage = mono.ColorManager;
            if (FindAllComponents())
            {
                _mono.GaspodCollectorStorage.OnAmountChanged += OnStorageAmountChange;
                _amountOfPodsCount.text = $"0/{QPatch.Configuration.Config.StorageLimit}";
            }
        }

        private void OnStorageAmountChange(int amount)
        {
            QuickLogger.Debug($"Changing Amount to : {amount}",true);
            _amountOfPodsCount.text = $"{amount}/{QPatch.Configuration.Config.StorageLimit}";
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "GasPodBTN":
                    _mono.GaspodCollectorStorage.RemoveGaspod();
                    break;
                case "ColorBTN":
                    _mono.ChangePage(2);
                    break;
                case "HomeBTN":
                    _mono.ChangePage(1);
                    break;
                case "PowerBTN":
                    QuickLogger.Debug("Cutting Power Off", true);
                    break;
            }
        }

        public override bool FindAllComponents()
        {
            try
            {
                QuickLogger.Debug("In FindAllComponents");
                //Locate all the ui controls
                #region Canvas  
                var canvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

                if (canvasGameObject == null)
                {
                    QuickLogger.Error("Canvas cannot be found");
                    return false;
                }
                #endregion

                #region Home
                var home = InterfaceHelpers.FindGameObject(canvasGameObject, "Home");
                #endregion

                #region Amount
                _amountOfPodsCount = InterfaceHelpers.FindGameObject(home, "AmountOfPodsCount")?.GetComponent<Text>();
                #endregion

                #region GasPodButton
                var gasPodButtonObj = InterfaceHelpers.FindGameObject(home, "GasPodButton");

                InterfaceHelpers.CreateButton(gasPodButtonObj, "GasPodBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.black, Color.white, MAX_INTERACTION_DISTANCE);

                uGUI_Icon icon = gasPodButtonObj.transform.Find("Image").gameObject.AddComponent<uGUI_Icon>();
                icon.sprite = SpriteManager.Get(TechType.GasPod);
                #endregion

                #region ColorPicker Button

                var colorBTN = InterfaceHelpers.FindGameObject(home, "ColorBTN");

                InterfaceHelpers.CreateButton(colorBTN, "ColorBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE);

                #endregion

                #region Power Button

                var powerBTN = InterfaceHelpers.FindGameObject(home, "PowerBTN");

                InterfaceHelpers.CreateButton(powerBTN, "PowerBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE);

                #endregion

                #region ColorPicker

                var colorPicker = InterfaceHelpers.FindGameObject(canvasGameObject, "ColorPicker");
                #endregion

                #region Color Paginator
                var _colorPaginator = InterfaceHelpers.FindGameObject(colorPicker, "Paginator");
                #endregion

                #region Color Grid
                var _colorGrid = InterfaceHelpers.FindGameObject(colorPicker, "Grid");
                #endregion

                #region ColorPage
                _colorPage.SetupGrid(66, GaspodCollectorBuildable.ColorItemPrefab, _colorGrid, _colorPaginator.GetComponent<Text>(), OnButtonClick);
                #endregion

                #region Prev Color Button
                var prevColorBtn = InterfaceHelpers.FindGameObject(colorPicker, "PrevBTN");
                
                InterfaceHelpers.CreatePaginator(prevColorBtn, -1, _colorPage.ChangeColorPageBy, _startColor, _hoverColor);
                #endregion

                #region Next Color Button
                var nextColorBtn = InterfaceHelpers.FindGameObject(colorPicker, "NextBTN");
                
                InterfaceHelpers.CreatePaginator(nextColorBtn, 1, _colorPage.ChangeColorPageBy, _startColor, _hoverColor);
                #endregion

                #region HomeButton
                var homeBTN = InterfaceHelpers.FindGameObject(colorPicker, "HomeBTN");

                InterfaceHelpers.CreateButton(homeBTN, "HomeBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE);

                #endregion

                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error<GasopodCollectorDisplayManager>($"{e.Message}\n{e.StackTrace}");
                return false;
            }
        }
    }
}
