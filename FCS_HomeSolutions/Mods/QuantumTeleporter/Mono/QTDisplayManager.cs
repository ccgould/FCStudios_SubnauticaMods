using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_AlterraHub.Abstract;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Buildable;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Enumerators;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Model;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.QuantumTeleporter.Mono
{
    internal class QTDisplayManager : AIDisplay
    {
        private QuantumTeleporterController _mono;
        private Text _baseName;
        private Text _networkLabel;
        private Text _requirements;
        private Text _location;
        private Text _message;
        private Text _destination;
        private readonly Color _startColor = new Color(0.4431373f, 0.8666667f, 0.4196079f,1f);
        private readonly Color _hoverColor = Color.white;
        private FCSToggleButton _globalNetworkToggleBtn;
        private FCSToggleButton _toggleGlobalNetworkToggleBtn;
        private FCSToggleButton _homeNetworkToggleBtn;
        private GridHelper _teleportGrid;
        private GameObject _destinationsScreen;
        private GameObject _confirmationScreen;
        private QuantumTeleporterController _toUnit;
        private readonly StringBuilder _sb = new StringBuilder();
        private GameObject _canvasGo;
        public QTTeleportTypes SelectedTab { get; private set; } = QTTeleportTypes.Intra;

        internal void Setup(QuantumTeleporterController mono)
        {
            _mono = mono;
            _teleportGrid = gameObject.AddComponent<GridHelper>();

            if(FindAllComponents())
            {
                _location.text = AuxPatchers.LocationFormat(gameObject.transform.position);
                _baseName.text = _mono.GetName();
                _message.text = AuxPatchers.TeleporterConfirmMessage();
                _requirements.text = AuxPatchers.Requirements(
                    QPatch.Configuration.QuantumTeleporterInternalTeleportPowerUsage,
                    QPatch.Configuration.QuantumTeleporterGlobalTeleportPowerUsage);
                _teleportGrid.DrawPage();
                _homeNetworkToggleBtn.Select();
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "RenameBTN":
                    _mono.NameController.Show();
                    break;
                case "HomeNetworkBTN":
                    SetTab(QTTeleportTypes.Intra);
                    break;
                case "GlobalNetworkBTN":
                    SetTab(QTTeleportTypes.Global);
                    break;
                case "ToggleGlobalNetworkBTN":
                    _mono.ToggleIsGlobal();
                    break;
                case "NetworkItem":
                    _toUnit = (QuantumTeleporterController)tag;

                    if (_toUnit.PowerManager.HasEnoughPower(SelectedTab) && _mono.PowerManager.HasEnoughPower(SelectedTab))
                    {
                        _destination.text = $"[{_toUnit.GetName()}]";
                        GotoPage(QttPages.Confirmation);
                    }
                    
                    break;
                case "CancelBTN":
                    GotoPage(QttPages.Destinations);
                    break;
                case "ConfirmBTN":
                    GotoPage(QttPages.Destinations);
                    TeleportManager.TeleportPlayer(_mono, _toUnit, SelectedTab);
                    break;
            }
        }

        private void GotoPage(QttPages page)
        {
            if (page == QttPages.Destinations)
            {
                _confirmationScreen.SetActive(false);
                _destinationsScreen.SetActive(true);
            }
            else
            {
                _confirmationScreen.SetActive(true);
                _destinationsScreen.SetActive(false);
            }
        }

        public override bool FindAllComponents()
        {
            try
            {
                var canvas = gameObject.GetComponentInChildren<Canvas>();

                _canvasGo = canvas.gameObject;

                //Add ProximityActivate to controller screen visiblity
                var px = gameObject.AddComponent<ProximityActivate>();
                px.Initialize(canvas.gameObject,gameObject,2);

                var home = InterfaceHelpers.FindGameObject(canvas.gameObject, "Home");
                
                _destinationsScreen = InterfaceHelpers.FindGameObject(canvas.gameObject, "DestinationsScreen");
                
                _confirmationScreen = InterfaceHelpers.FindGameObject(canvas.gameObject, "ConfirmationScreen");
      
                //Base name
                _baseName = GameObjectHelpers.FindGameObject(gameObject, "BaseName")?.GetComponent<Text>();

                //Network Label
                _networkLabel = GameObjectHelpers.FindGameObject(gameObject, "NetworkLabel")?.GetComponent<Text>();

                //Requirments
                _requirements = GameObjectHelpers.FindGameObject(gameObject, "Requirements")?.GetComponent<Text>();

                //Location
                _location = GameObjectHelpers.FindGameObject(gameObject, "Location")?.GetComponent<Text>();

                //Destination
                _destination = GameObjectHelpers.FindGameObject(gameObject, "Destination")?.GetComponent<Text>();

                //Message
                _message = GameObjectHelpers.FindGameObject(gameObject, "Message")?.GetComponent<Text>();

                //Home Network Toggle
                var homeNetworkToggle = GameObjectHelpers.FindGameObject(gameObject, "HomeNetworkBTN");
                _homeNetworkToggleBtn = homeNetworkToggle.AddComponent<FCSToggleButton>();
                _homeNetworkToggleBtn.ButtonMode = InterfaceButtonMode.Background;
                _homeNetworkToggleBtn.STARTING_COLOR = _startColor;
                _homeNetworkToggleBtn.HOVER_COLOR = _hoverColor;
                _homeNetworkToggleBtn.BtnName = "HomeNetworkBTN";
                _homeNetworkToggleBtn.TextLineOne = AuxPatchers.HomeNetworkToggle();
                _homeNetworkToggleBtn.TextLineTwo = AuxPatchers.HomeNetworkToggleDesc();
                _homeNetworkToggleBtn.OnButtonClick = OnButtonClick;
                _homeNetworkToggleBtn.InteractionRequirement = InteractionRequirement.None;

                //Global Network Toggle
                var globalNetworkToggle = GameObjectHelpers.FindGameObject(gameObject, "GlobalNetworkBTN");
                _globalNetworkToggleBtn = globalNetworkToggle.AddComponent<FCSToggleButton>();
                _globalNetworkToggleBtn.ButtonMode = InterfaceButtonMode.Background;
                _globalNetworkToggleBtn.STARTING_COLOR = _startColor;
                _globalNetworkToggleBtn.HOVER_COLOR = _hoverColor;
                _globalNetworkToggleBtn.BtnName = "GlobalNetworkBTN";
                _globalNetworkToggleBtn.TextLineOne = AuxPatchers.GlobalNetworkToggle();
                _globalNetworkToggleBtn.TextLineTwo = AuxPatchers.GlobalNetworkToggleDesc();
                _globalNetworkToggleBtn.OnButtonClick = OnButtonClick;
                _globalNetworkToggleBtn.InteractionRequirement = InteractionRequirement.None;

                //Add To Global Network Toggle
                var toggleGlobalNetworkBTN = GameObjectHelpers.FindGameObject(gameObject, "ToggleGlobalNetworkBTN");
                _toggleGlobalNetworkToggleBtn = toggleGlobalNetworkBTN.AddComponent<FCSToggleButton>();
                _toggleGlobalNetworkToggleBtn.ButtonMode = InterfaceButtonMode.Background;
                _toggleGlobalNetworkToggleBtn.STARTING_COLOR = _startColor;
                _toggleGlobalNetworkToggleBtn.HOVER_COLOR = _hoverColor;
                _toggleGlobalNetworkToggleBtn.BtnName = "ToggleGlobalNetworkBTN";
                _toggleGlobalNetworkToggleBtn.TextLineOne = AuxPatchers.AddToGlobalNetworkToggle();
                _toggleGlobalNetworkToggleBtn.TextLineTwo = AuxPatchers.AddToGlobalNetworkToggleDesc();
                _toggleGlobalNetworkToggleBtn.OnButtonClick = OnButtonClick;
                _toggleGlobalNetworkToggleBtn.InteractionRequirement = InteractionRequirement.None;

                //Rename
                var renameObj = InterfaceHelpers.FindGameObject(canvas.gameObject, "RenameBTN");
                var renameBTN = InterfaceHelpers.CreateButton(renameObj, "RenameBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, 5);
                renameBTN.TextLineOne = AuxPatchers.Rename();
                renameBTN.TextLineTwo = AuxPatchers.RenameDesc();
                renameBTN.InteractionRequirement = InteractionRequirement.None;

                #region Grid
                _teleportGrid.Setup(4, ModelPrefab.NetworkItemPrefab, home, Color.gray, Color.white, OnButtonClick,
                    5, "PrevBTN", "NextBTN", "Grid", "Paginator", string.Empty);
                _teleportGrid.OnLoadDisplay += OnLoadDisplay;
                #endregion

                var cancelGO = InterfaceHelpers.FindGameObject(home, "CancelBTN");
                var cancelBTN = InterfaceHelpers.CreateButton(cancelGO, "CancelBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.black, Color.white, 5);
                cancelBTN.ChangeText(AuxPatchers.Cancel());
                cancelBTN.InteractionRequirement = InteractionRequirement.None;

                var confirmGO = InterfaceHelpers.FindGameObject(home, "ConfirmBTN");
                var confirmBTN = InterfaceHelpers.CreateButton(confirmGO, "ConfirmBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.black, Color.white, 5);
                confirmBTN.ChangeText(AuxPatchers.Confirm());
                confirmBTN.InteractionRequirement = InteractionRequirement.None;
            }
            catch (Exception e)
            {
                QuickLogger.Error($"[FindAllComponents]: {e.Message}");
                QuickLogger.Error($"[FindAllComponents]: {e.StackTrace}");
            }

            return true;
        }

        private void OnLoadDisplay(DisplayData data)
        {
            List<FcsDevice> items = null;

            _teleportGrid.ClearPage();

            if (_mono.Manager != null)
            {
                if (SelectedTab == QTTeleportTypes.Global)
                {
                    items = new List<FcsDevice>();
                    foreach (var device in FCSAlterraHubService.PublicAPI.GetRegisteredDevicesOfId(QuantumTeleporterBuildable.QuantumTeleporterTabID))
                    {
                        var fcsDevice = (QuantumTeleporterController)device.Value;
                        if (fcsDevice.IsGlobal && fcsDevice.Manager != _mono.Manager)
                        {
                            items.Add(device.Value);
                        }
                    }
                }
                else if (SelectedTab == QTTeleportTypes.Intra)
                {
                    items = _mono.Manager.GetDevices(QuantumTeleporterBuildable.QuantumTeleporterTabID).Where(x => x.Manager == _mono.Manager)
                        .ToList();
                }

                if (items == null)
                {
                    QuickLogger.Error<QTDisplayManager>("Items list returned null");
                    return;
                }

                if (data.EndPosition > items.Count)
                {
                    data.EndPosition = items.Count;
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    var unit = items[i];
                    var unitName = ((QuantumTeleporterController)unit).GetName();

                    if (unit == _mono) continue;

                    GameObject itemDisplay = Instantiate(data.ItemsPrefab);

                    if (itemDisplay == null || data.ItemsGrid == null)
                    {
                        if (itemDisplay != null)
                        {
                            Destroy(itemDisplay);
                        }
                        return;
                    }

                    itemDisplay.transform.SetParent(data.ItemsGrid.transform, false);
                    var textField = itemDisplay.transform.Find("Name").gameObject;
                    var text = textField.GetComponent<Text>();
                    text.text = unitName;

                    var itemButton = itemDisplay.AddComponent<InterfaceButton>();
                    var status =  itemDisplay.AddComponent<StatusUpdater>();
                    status.Unit = unit;
                    itemButton.ButtonMode = InterfaceButtonMode.TextColor;
                    itemButton.Tag = unit;
                    itemButton.TextComponent = text;
                    itemButton.GetAdditionalDataFromString = true;
                    itemButton.GetAdditionalString += fcsDevice => $"{Language.main.GetFormat("HUDPowerStatus", ((FcsDevice) fcsDevice).Manager.Habitat.powerRelay.GetPower(), ((FcsDevice) fcsDevice).Manager.Habitat.powerRelay.GetMaxPower())}";
                    itemButton.OnButtonClick += OnButtonClick;
                    itemButton.BtnName = "NetworkItem";
                    itemButton.MaxInteractionRange = 5;

                    QuickLogger.Debug($"Added Unit {unitName}");
                }
            }
            
            _teleportGrid.UpdaterPaginator(items?.Count ?? 0);
        }
        
        private string NetworkFormat(object go)
        {
            _sb.Clear();

            var controller = (FcsDevice) go;

            if (controller != null)
            {
                _sb.Append($"{AuxPatchers.Coordinate()}: {_mono.transform.position}");
                _sb.Append(Environment.NewLine);
                _sb.Append($"{AuxPatchers.PowerAvailable()}: {_mono.PowerManager.PowerAvailable()}");
            }

            return _sb.ToString();
        }

        public Text GetNameTextBox()
        {
            return _baseName;
        }

        public QTTeleportTypes GetSelectedTab()
        {
            return SelectedTab;
        }

        private void SetTab(QTTeleportTypes tabs)
        {
            if (tabs == QTTeleportTypes.Intra)
            {
                _networkLabel.text = AuxPatchers.LocalNetwork();
                _homeNetworkToggleBtn.Select();
                _globalNetworkToggleBtn.DeSelect();
                SelectedTab = QTTeleportTypes.Intra;
            }
            else
            {
                _networkLabel.text = AuxPatchers.GlobalNetwork();
                _homeNetworkToggleBtn.DeSelect();
                _globalNetworkToggleBtn.Select();
                SelectedTab = QTTeleportTypes.Global;
            }

            RefreshTabs();
        }
        
        public void Load(QuantumTeleporterDataEntry data)
        {
            SetTab(data.SelectedTab);

            if (data.IsGlobal)
            {
                _toggleGlobalNetworkToggleBtn.Select();
            }
        }

        public void RefreshBaseName(string name)
        {
            _baseName.text = $"UnitID: {_mono.UnitID} - {name}";
        }

        public void ShowTeleportPage(bool value)
        {
            
        }

        public void RefreshTabs()
        {
            _teleportGrid.DrawPage();
        }

        private enum QttPages
        {
            Destinations,
            Confirmation
        }

        public void ChangeScreenVisiblity(bool isVisible)
        {
            _canvasGo.SetActive(isVisible);
        }
    }

    internal class StatusUpdater : MonoBehaviour
    {
        private GameObject _icon;
        private QuantumTeleporterController _controller;
        public FcsDevice Unit { get; set; }

        private void Start()
        {
            _icon = GameObjectHelpers.FindGameObject(gameObject, "ConnectionIcon");
            _controller = (QuantumTeleporterController) Unit;
        }

        private void Update()
        {
            if (Unit != null && _icon != null && _controller != null)
            {
                _icon.SetActive(_controller.PowerManager.HasEnoughPower(_controller.DisplayManager.SelectedTab));
            }
        }
    }
}
