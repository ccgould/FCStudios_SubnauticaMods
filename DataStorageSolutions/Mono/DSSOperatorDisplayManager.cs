using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Display;
using DataStorageSolutions.Enumerators;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Model;
using DataStorageSolutions.Structs;
using FCSCommon.Abstract;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Enums;
using rail;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace DataStorageSolutions.Mono
{
    internal class DSSOperatorDisplayManager : AIDisplay, IMessageDialogSender
    {
        private DSSOperatorController _mono;
        private bool _isBeingDestroyed;
        private int _page;
        private readonly Color _startColor = Color.grey;
        private readonly Color _hoverColor = Color.white;
        private FCSMessageBoxDialog _messageBoxDialog;
        private GameObject _devicesList;
        private GameObject _propertyList;
        private GameObject _propertyView;
        private Toggle _visibilityToggle;
        private FCSOperation _currentUnit;
        private Toggle _transferToggle;
        private Toggle _autocraftToggle;
        private GameObject _transferList;
        private GameObject _autocraftingList;
        private GameObject _autocraftingSection;
        private GameObject _transferSection;
        private bool _isResetting;
        private bool _updatingSettingsPage;
        private AutoCraftingWizardDialog _autoCraftDialog;
        private AutoCraftOperationData _pendingAutocraftDelete;
        private bool _dialogOpen;
        private string _previousID;
        private TransferWizardDialog _transferDialog;
        private TransferOperationData _pendingTransferDelete;

        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }

        internal void Setup(DSSOperatorController mono)
        {
            _mono = mono;
            _page = Animator.StringToHash("Pages");
            if (FindAllComponents())
            {
                LoadDevicesList();
                HideSettings();
            }
        }

        public void ShowMessageBox(string message, Action<FCSDialogResult> callback = null,FCSMessageBox mode = FCSMessageBox.OK)
        {
            _messageBoxDialog.ShowMessageBox(message,"",callback, mode);
        }

        public override bool FindAllComponents()
        {
            try
            {
                #region Home

                var home = GameObjectHelpers.FindGameObject(gameObject, "Home");

                //Devices List
                _devicesList = GameObjectHelpers.FindGameObject(home, "DevicesViewContent");

                //Properties View
                _propertyView = GameObjectHelpers.FindGameObject(home, "PropertiesView");
                _propertyList = GameObjectHelpers.FindGameObject(home, "PropertiesViewContent");

                //Is Visible
                GameObjectHelpers.FindGameObject(_propertyList, "VisibilityLabel").GetComponent<Text>().text = AuxPatchers.IsVisible();
                _visibilityToggle = GameObjectHelpers.FindGameObject(_propertyList, "VisibilityToggle").GetComponent<Toggle>();
                _visibilityToggle.GetComponentInChildren<Text>().text = AuxPatchers.IsVisibleToggle();
                    _visibilityToggle.onValueChanged.AddListener(value =>{
                    {
                        if (!_isResetting && !_updatingSettingsPage)
                        {
                            _currentUnit.ConnectableDevice.IsVisible = value;
                        }
                    }});

                //Transfer Items
                GameObjectHelpers.FindGameObject(home, "TransferLabel").GetComponent<Text>().text = AuxPatchers.ItemTransfer();
                _transferList = GameObjectHelpers.FindGameObject(home, "TransferContent");
                _transferSection = GameObjectHelpers.FindGameObject(home, "TransferSection");
                _transferToggle = GameObjectHelpers.FindGameObject(_propertyList, "TransferToggle").GetComponent<Toggle>();
                _transferToggle.GetComponentInChildren<Text>().text = AuxPatchers.ItemTransferToggle();
                _transferToggle.onValueChanged.AddListener(value => { if (!_isResetting) _currentUnit.IsTransferAllowed = value; });
                
                var transferSectionAddButton = GameObjectHelpers.FindGameObject(_propertyList, "TransferSectionAddBTN");
                InterfaceHelpers.CreateButton(transferSectionAddButton, "TransferSectionAddBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.AddToTransferList());

                //AutoCraft
                GameObjectHelpers.FindGameObject(home, "AutoCraftLabel").GetComponent<Text>().text = AuxPatchers.AutoCraft();
                _autocraftingSection = GameObjectHelpers.FindGameObject(home, "AutoCraftingSection");
                _autocraftingList = GameObjectHelpers.FindGameObject(home, "AutoCraftingContent");
                _autocraftToggle = GameObjectHelpers.FindGameObject(_propertyList, "AutoCraftToggle").GetComponent<Toggle>();
                _autocraftToggle.GetComponentInChildren<Text>().text = AuxPatchers.AutoCraftToggle();
                _autocraftToggle.onValueChanged.AddListener(value => { if (!_isResetting) _currentUnit.IsAutoCraftingAllowed = value; });

                var autoCraftAddButton =GameObjectHelpers.FindGameObject(_propertyList, "AutoCraftingSectionAddBTN");
                InterfaceHelpers.CreateButton(autoCraftAddButton, "AutoCraftingSectionAddBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE,AuxPatchers.AddToAutoCraftingList());

                #endregion

                #region MessageBox

                var messageBox = GameObjectHelpers.FindGameObject(gameObject, "MessageBox");
                _messageBoxDialog =  messageBox.AddComponent<FCSMessageBoxDialog>();
                _messageBoxDialog.OnConfirmButtonClick += id =>
                {
                    
                };

                #endregion

                #region Add Crafting Dialog

                var autoCraftingDialog = GameObjectHelpers.FindGameObject(gameObject, "AutoCraftWizardDialog");
                _autoCraftDialog = autoCraftingDialog.AddComponent<AutoCraftingWizardDialog>();
                _autoCraftDialog.OnConfirmButtonClick += () =>
                {
                    //QuickLogger.Debug($"Showing Settings For UnitID: {_currentUnit.ID}",true);
                    //StartCoroutine(UpdateSettingPageCo());
                };

                #endregion

                #region Add Transfer Dialog

                var transferDialog = GameObjectHelpers.FindGameObject(gameObject, "TransferWizardDialog");
                _transferDialog = transferDialog.AddComponent<TransferWizardDialog>();
                _transferDialog.OnConfirmButtonClick += () =>
                {
                    //QuickLogger.Debug($"Showing Settings For UnitID: {_currentUnit.ID}", true);
                    //StartCoroutine(UpdateSettingPageCo());
                };

                #endregion

            }
            catch (Exception e)
            {
                QuickLogger.Error($"Error: {e.Message} | StackTrace: {e.StackTrace}");
                return false;
            }

            return true;
        }

        private IEnumerator UpdateSettingPageCo()
        {
            yield return new WaitForEndOfFrame();

            if (_currentUnit == null)
            {
                QuickLogger.Debug("CurrentUnit is null");
                yield break;
            }
            ShowSettings(_currentUnit.ID);
        }

        private void HideSettings()
        {
            _propertyView.SetActive(false);

        }

        private void ShowSettings(string unitID)
        {
            ResetSettingPage(unitID);
            UpdateSettingPage(unitID);
            _propertyView.SetActive(true);
        }

        private void UpdateSettingPage(string unitID)
        {
            _updatingSettingsPage = true;

            if (!BaseManager.Operations.ContainsKey(unitID))
            {
                ResetToggles();
                _updatingSettingsPage = false;
                return;
            }
            _currentUnit = BaseManager.Operations[unitID];

            if (!_currentUnit.ConnectableDevice.CanBeVisible())
            {
                _visibilityToggle.interactable = false;
            }
            
            if (_currentUnit.ConnectableDevice.GetTechType() == Mod.GetAutoCrafterTechType())
            {
                _transferSection.SetActive(false);
                foreach (AutoCraftOperationData operation in _currentUnit.AutoCraftRequestOperations)
                {
                    CreateAutoCraftEntry(operation);
                }
            }
            else
            {
                _autocraftingSection.SetActive(false);
                foreach (TransferOperationData operation in _currentUnit.TransferRequestOperations)
                {
                    CreateTransferEntry(operation);
                }
            }

            ResetToggles();

            _updatingSettingsPage = false;
        }

        private void ResetToggles()
        {
            _visibilityToggle.isOn = _currentUnit.ConnectableDevice.IsVisible;
            _autocraftToggle.isOn = _currentUnit.IsAutoCraftingAllowed;
            _transferToggle.isOn = _currentUnit.IsTransferAllowed;
        }

        private void CreateTransferEntry(TransferOperationData operation)
        {
            QuickLogger.Debug("Creating Transfer", true);
            GameObject buttonPrefab = GameObject.Instantiate(DSSModelPrefab.TransferItemEntryPrefab);
            var deleteBTN = GameObjectHelpers.FindGameObject(buttonPrefab, "DeleteBTN").GetComponent<Button>();
            deleteBTN.onClick.AddListener(() =>
            {
                ShowMessageBox(string.Format(AuxPatchers.DeleteConfirmationMessageFormat(), Language.main.Get(operation.TransferRequestItem)), TransferCallback, FCSMessageBox.YesNo);
                _pendingTransferDelete = operation;
                _dialogOpen = true;
            });

            GameObjectHelpers.FindGameObject(buttonPrefab, "FromLabel").GetComponent<Text>().text =
                $"Item: {Language.main.Get(operation.TransferRequestItem)}";
            GameObjectHelpers.FindGameObject(buttonPrefab, "MaximumAmountLabel").GetComponent<Text>().text =
                $"Amount: {operation.TransferRequestAmount}";
            buttonPrefab.transform.SetParent(_transferList.transform, false);
        }

        private void CreateAutoCraftEntry(AutoCraftOperationData operation)
        {
            GameObject buttonPrefab = GameObject.Instantiate(DSSModelPrefab.AutoCraftItemPrefab);
            buttonPrefab.transform.SetParent(_autocraftingList.transform, false);
            var deleteBTN = buttonPrefab.GetComponentInChildren<Button>();
            GameObjectHelpers.FindGameObject(buttonPrefab, "ItemLabel").GetComponent<Text>().text =
                $"Item: {Language.main.Get(operation.AutoCraftRequestItem)}";
            GameObjectHelpers.FindGameObject(buttonPrefab, "RequestAmountLabel").GetComponent<Text>().text =
                $"Amount: {operation.AutoCraftMaxAmount}";
            deleteBTN.onClick.AddListener(() =>
            { 
                ShowMessageBox(string.Format(AuxPatchers.DeleteConfirmationMessageFormat(), Language.main.Get(operation.AutoCraftRequestItem)),AutoCraftCallback,FCSMessageBox.YesNo);
                _pendingAutocraftDelete = operation;
                _dialogOpen = true;
            });
        }

        private void AutoCraftCallback(FCSDialogResult result)
        {
            if (result == FCSDialogResult.Yes)
            {
                _currentUnit.AutoCraftRequestOperations.Remove(_pendingAutocraftDelete);
            }

            _dialogOpen = false;
        }

        private void TransferCallback(FCSDialogResult result)
        {
            if (result == FCSDialogResult.Yes)
            {
                _currentUnit.TransferRequestOperations.Remove(_pendingTransferDelete);
            }

            _dialogOpen = false;
        }

        private void OpenItemSelectionWindow()
        {
            QuickLogger.Debug("Opening Item Selection",true);
        }
        
        private void ShowAutocraftWizardDialog()
        {
            _autoCraftDialog.ShowDialog(this,_currentUnit);
        }

        private void ResetSettingPage(string unitID)
        {
            _isResetting = true;
            _transferSection.SetActive(true);
            _autocraftingSection.SetActive(true);
            _visibilityToggle.interactable = true;

            _visibilityToggle.isOn = false;
            _autocraftToggle.isOn = false;
            _transferToggle.isOn = false;


            foreach (Transform list in _autocraftingList.transform)
            {
                Destroy(list.gameObject);
            }

            foreach (Transform list in _transferList.transform)
            {
                Destroy(list.gameObject);
            }

            _isResetting = false;
        }
        
        internal void GoToPage(OperatorPages page)
        {
            _mono.AnimationHandler.SetIntHash(_page,(int) page);
        }

        internal OperatorPages GetCurrentPage()
        {
            return (OperatorPages)_mono.AnimationHandler.GetIntHash(_page);
        }

        private void LoadDevicesList()
        {
            ClearDeviceList();

            foreach (var connectable in _mono.Manager.FCSConnectables)
            {
                GameObject buttonPrefab = GameObject.Instantiate(DSSModelPrefab.DeviceButtonPrefab);
                buttonPrefab.transform.SetParent(_devicesList.transform, false);
                var buttonLabel = buttonPrefab.GetComponentInChildren<Text>().text = connectable.Value.UnitID;
                var button = buttonPrefab.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    if (_dialogOpen) return;
                    ShowSettings(connectable.Value.UnitID);
                });
                if (connectable.Value.GetTechType() == Mod.GetAutoCrafterTechType())
                {
                    var controller = ((DSSAutoCrafterController)connectable.Value.GetController());
                   if(!controller.IsLinked())
                   {
                       controller.LinkOperation(BaseManager.Operations[connectable.Value.UnitID]);
                   }
                }
            }
        }

        private void ClearDeviceList()
        {
            try
            {
                _currentUnit = null;

                foreach (Transform device in _devicesList.transform)
                {
                    Destroy(device.gameObject);
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error($"[Method ClearDeviceList]: {e.Message}");
            }
        }

        internal void Refresh()
        {
            _previousID = _currentUnit?.ID;
#if DEBUG
            QuickLogger.Debug($"Refreshing Operator: {_mono?.GetPrefabID()}", true);
#endif
            LoadDevicesList();

            if (!string.IsNullOrWhiteSpace(_previousID))
            {
                UpdateSettingPage(_previousID);
            }

            StartCoroutine(UpdateSettingPageCo());
        }
        
        public override void OnButtonClick(string btnName, object tag)
        {
            if(_dialogOpen) return;
            switch (btnName)
            {
                case "AutoCraftingSectionAddBTN":
                    ShowAutocraftWizardDialog();
                    break;

                case "TransferSectionAddBTN":
                    ShowTransferItemDialog();
                    break;

                case "DeleteBTN":
                    QuickLogger.Debug("Delete Me",true);
                    break;
            }
        }

        private void ShowTransferItemDialog()
        {
            _transferDialog.ShowDialog(this, _currentUnit);
        }

        private void AddDummy()
        {
            _currentUnit.AddTransferOperation(this,TechType.Peeper,1);
        }
    }
}
