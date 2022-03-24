using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal.Enumerators;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class MoonPoolPageController : MonoBehaviour, IFCSDumpContainer,ITerminalPage
    {
        private DSSTerminalDisplayManager _mono;
        private FCSToggleButton _pullFromVehicles;
        private GameObject _blackListButtonObj;
        private GameObject _pullFromVehiclesToggleObj;
        private DumpContainerSimplified _blackListDumpContainer;
        private readonly List<FilterItemButton> _filterItemButtons = new List<FilterItemButton>();
        private GameObject _vehiclesSettingsSection;
        private GameObject _vehiclesSection;
        private Text _vehicleSectionName;
        private GameObject _vehiclesSectionGrid;
        private readonly List<VehicleItemButton> _vehicleItemButtons = new List<VehicleItemButton>();
        private Vehicle _currentVehicle;


        private void OnButtonClick(string arg1, object arg2)
        {
            switch (arg1)
            {
                case "PullFromVehicles":
                    _mono.GetController().Manager.PullFromDockedVehicles = IsPullingFromVehicles();
                    break;
                case "BlackListButton":
                    _blackListDumpContainer.OpenStorage();
                    break;
                case "VehSettingBTN":
                    _vehiclesSettingsSection.SetActive(true);
                    _vehiclesSection.SetActive(false);
                    break;
            }
        }

        internal void RefreshBlackListItems()
        {
            for (int i = 0; i < 7; i++)
            {
                _filterItemButtons[i].Reset();
            }

            var techTypes = _mono.GetController().Manager.DockingBlackList;

            for (int i = 0; i < techTypes.Count; i++)
            {
                _filterItemButtons[i].Set(techTypes[i]);
            }
        }

        public void Initialize(DSSTerminalDisplayManager mono)
        {
            _mono = mono;

            _pullFromVehiclesToggleObj = GameObjectHelpers.FindGameObject(gameObject, "PullFromVehicles");
            _blackListButtonObj = GameObjectHelpers.FindGameObject(gameObject, "BlackListButton");
            _vehiclesSection = GameObjectHelpers.FindGameObject(gameObject, "VehiclesSection");
            _vehicleSectionName = _vehiclesSection.FindChild("VehicleName").GetComponent<Text>();
            _vehiclesSectionGrid = _vehiclesSection.FindChild("Grid");


            InterfaceHelpers.CreateButton(_blackListButtonObj, "BlackListButton", InterfaceButtonMode.Background,
                OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f, AuxPatchers.AddToBlackList());

            _pullFromVehicles = _pullFromVehiclesToggleObj.AddComponent<FCSToggleButton>();
            _pullFromVehicles.TextLineOne = "Pull from vehicles";
            _pullFromVehicles.ButtonMode = InterfaceButtonMode.RadialButton;
            _pullFromVehicles.BtnName = "PullFromVehicles";
            _pullFromVehicles.OnButtonClick += OnButtonClick;

            if (GetManager().PullFromDockedVehicles)
            {
                _pullFromVehicles.Select();
            }

            var backButton = GameObjectHelpers.FindGameObject(gameObject, "BackBTN").AddComponent<FCSButton>();
            backButton.TextLineOne = "Go Back";
            backButton.Subscribe((() => { _mono.GoToTerminalPage(TerminalPages.Configuration); }));

            _vehiclesSettingsSection = GameObjectHelpers.FindGameObject(gameObject, "MoonPoolSettings");
            var vehiclesSettingsBTN = GameObjectHelpers.FindGameObject(gameObject, "SettingsBTN");
            InterfaceHelpers.CreateButton(vehiclesSettingsBTN, "VehSettingBTN", InterfaceButtonMode.Background,
                OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f, AuxPatchers.MoonpoolSettings());

            var settingsGrid = GameObjectHelpers.FindGameObject(_vehiclesSettingsSection, "Grid");

            if (settingsGrid != null)
            {
                foreach (Transform filterChild in settingsGrid.transform)
                {
                    var item = filterChild.gameObject.AddComponent<FilterItemButton>();
                    item.MoonPoolPage = this;
                    _filterItemButtons.Add(item);
                }
            }

            var vehicleDockingManager = InterfaceHelpers.FindGameObject(gameObject, "SideBar").AddComponent<MoonPoolDialog>();
            vehicleDockingManager.Initialize(GetManager(), this);

            foreach (Transform vgChild in _vehiclesSectionGrid.transform)
            {
                var item = vgChild.gameObject.AddComponent<VehicleItemButton>();
                _vehicleItemButtons.Add(item);
            }


            if (_blackListDumpContainer == null)
            {
                _blackListDumpContainer = gameObject.AddComponent<DumpContainerSimplified>();
                _blackListDumpContainer.Initialize(transform, "Add to blacklist", this);
                RefreshBlackListItems();
            }

        }

        internal bool IsPullingFromVehicles() => _pullFromVehicles.IsSelected;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public BaseManager GetManager()
        {
            return _mono.GetController().Manager;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            if (!GetManager().DockingBlackList.Contains(item.item.GetTechType()))
            {
                GetManager().DockingBlackList.Add(item.item.GetTechType());
            }

            PlayerInteractionHelper.GivePlayerItem(item);
            RefreshBlackListItems();

            return true;
        }

        public bool IsAllowedToAdd(TechType techType, bool verbose)
        {
            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return IsAllowedToAdd(pickupable.GetTechType(), verbose);
        }

        public void RefreshVehicleName()
        {
            if (_vehicleSectionName == null) return;
#if SUBNAUTICA
            _vehicleSectionName.text = _currentVehicle?.GetName();
#else
            _vehicleSectionName.text = _currentVehicle?.vehicleName;
#endif
        }

        public void ShowVehicleContainers(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                if (GetManager().DockingManager.IsVehicleDocked(_currentVehicle))
                {
                    vehicle = _currentVehicle;
                }
                else
                {
                    _vehiclesSection.SetActive(false);
                    _vehiclesSettingsSection.SetActive(false);
                    for (int i = 0; i < 8; i++)
                    {
                        _vehicleItemButtons[i].Reset();
                    }

                    return;
                }
            }

            var storage = DSSVehicleDockingManager.GetVehicleContainers(vehicle);
            _vehiclesSection.SetActive(true);
            _vehiclesSettingsSection.SetActive(false);
            
            for (int i = 0; i < 8; i++)
            {
                _vehicleItemButtons[i].Reset();
            }

            for (int i = 0; i < storage.Count; i++)
            {
                _vehicleItemButtons[i].Set(vehicle, storage[i], i);
            }

            _currentVehicle = vehicle;
        }

        public DSSTerminalController GetController()
        {
            return _mono.GetController();
        }
    }

    internal interface ITerminalPage
    {
        void Initialize(DSSTerminalDisplayManager mono);
        void Show();
        void Hide();
    }
}