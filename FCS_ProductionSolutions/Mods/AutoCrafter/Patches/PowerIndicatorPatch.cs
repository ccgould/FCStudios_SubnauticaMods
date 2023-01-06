﻿using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Model.GUI;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.AutoCrafter.Buildable;
using FCS_ProductionSolutions.Mods.AutoCrafter.Helpers;
using FCS_ProductionSolutions.Mods.AutoCrafter.Models;
using FCS_ProductionSolutions.Mods.AutoCrafter.Models.StateMachine.States;
using FCS_ProductionSolutions.Mods.AutoCrafter.Mono;
using FCSCommon.Utilities;
using HarmonyLib;
using SMLHelper.Handlers;
using UnityEngine;
using UnityEngine.UI;

#if SUBNAUTICA

#endif

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Patches
{
    [HarmonyPatch(typeof(uGUI_PowerIndicator))]
    [HarmonyPatch("Initialize")]
    class uGUI_PowerIndicator_Initialize_Patch
    {
        private static FCSHUD tracker;

        private static void Postfix(uGUI_PowerIndicator __instance)
        {

            if (IndicatorInstance == null)
            {
                if (Inventory.main == null)
                {
                    return;
                }

                var hudTransform = GameObject.Find("ScreenCanvas").transform.Find("HUD").Find("Content");


                var autocrafterGui = GameObject.Instantiate(ModelPrefab.GetPrefab("uGUI_AutoCrafter"));

                autocrafterGui.transform.SetParent(hudTransform, false);
                autocrafterGui.transform.SetSiblingIndex(0);
                autocrafterGui.transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
                var autocrafterHud = autocrafterGui.AddComponent<AutocrafterHUD>();
                autocrafterHud.Hide();
                
                IndicatorInstance = __instance;
            }
        }


        public static uGUI_PowerIndicator IndicatorInstance { get; set; }
    }

    public class AutocrafterHUD : uGUI_InputGroup, uGUI_IButtonReceiver
    {
        private GameObject _grid;
        public static AutocrafterHUD Main;
        private bool _isOpen;
        private readonly List<uGUI_FCSDisplayItem> _currentItems = new();
        private readonly Dictionary<string, Toggle> _currentAutocrafterToggles = new();
        private AutoCrafterController _sender;
        private GameObject _craftToggleItemPrefab;
        private GameObject _craftButtonItemPrefab;
        private CraftingOperation _operation;
        private string _currentSearchString;
        private ToggleGroup _toggleGroup;
        private InputField _amountInput;
        private GameObject _fcsToggleItemPrefab;
        private GameObject _autoCrafterGrid;
        private GameObject _pendingItemsGrid;
        private GameObject _neededItemsGrid;
        private TechType _currentTechType;
        private bool _isRecursive = false;
        private Button _craftBTN;
        private Text _craftingAmount;
        private Toggle _isRecursiveToggle;
        private Button _cancelBtn;
        private Button _searchBTN;
        private GameObject _missingItemsGrid;
        private InputField _limitInput;
        private GameObject _continuousLimitSection;
        private Toggle _isInfiniteCraft;
        private Toggle _isLimitedToggle;
        private Toggle _isCraftAmountToggle;
        private GameObject _autoCrafterPanels;
        private GameObject _standByPanel;
        private GameObject _mainPanel;
        private GameObject _requiredItemsPanel;
        private GameObject _missingItemsPanel;
        private Text _standByTxt;

        public override void Update()
        {
            base.Update();

            if (focused && GameInput.GetButtonDown(GameInput.Button.PDA))
            {
                Hide();
            }
        }

        public override void Awake()
        {
            base.Awake();

            if (Main == null)
            {
                Main = this;
                DontDestroyOnLoad(this);
            }
            else if (Main != null)
            {
                Destroy(gameObject);
                return;
            }

            _craftToggleItemPrefab = ModelPrefab.GetPrefabFromGlobal("OnScreenItemToggleSelection").AddComponent<uGUI_FCSDisplayItem>().gameObject;
            _craftButtonItemPrefab = ModelPrefab.GetPrefab("uGUI_AutoCrafterItemButton").AddComponent<uGUI_FCSDisplayItem>().gameObject;
            _fcsToggleItemPrefab = ModelPrefab.GetPrefabFromGlobal("FCSToggle");
            _toggleGroup = gameObject.GetComponentInChildren<ToggleGroup>();
            _amountInput = GameObjectHelpers.FindGameObject(gameObject, "AmountInputField").GetComponent<InputField>();
            _limitInput = GameObjectHelpers.FindGameObject(gameObject, "LimitInputField").GetComponent<InputField>();
            _limitInput.onEndEdit.AddListener((amount =>
            {
                if (int.TryParse(amount, out int amountInt))
                {
                    _sender.CraftMachine.SetLimitAmount(amountInt);
                }
            }));

            _craftingAmount = GameObjectHelpers.FindGameObject(gameObject, "CraftingAmount").GetComponent<Text>();
            _standByTxt = GameObjectHelpers.FindGameObject(gameObject, "StandBy_Txt").GetComponent<Text>();
            UpdateAmountLabel(true);
            
            _continuousLimitSection = GameObjectHelpers.FindGameObject(gameObject, "ContinuousLimitSection");
            _isRecursiveToggle = GameObjectHelpers.FindGameObject(gameObject, "IsRecursiveToggle").GetComponent<Toggle>();
            _isRecursiveToggle.onValueChanged.AddListener((value =>
            {
                _continuousLimitSection.gameObject.SetActive(value);
                _isInfiniteCraft.gameObject.SetActive(value);

                _isRecursive = value;

               _sender.SetIsRecursive(value);

                UpdateAmountLabel();
            }));
            
            _isLimitedToggle = GameObjectHelpers.FindGameObject(gameObject, "IsLimitedToggle").GetComponent<Toggle>();
            _isLimitedToggle.onValueChanged.AddListener((value =>
            {
                _sender.SetIsLimitedOperation(value);
            }));

            _isCraftAmountToggle = GameObjectHelpers.FindGameObject(gameObject, "CraftAmountToggle").GetComponent<Toggle>();

            _isInfiniteCraft = GameObjectHelpers.FindGameObject(gameObject, "IsInfiniteCraft").GetComponent<Toggle>();
            _isInfiniteCraft.onValueChanged.AddListener((value =>
            {
                _sender.SetIsInfiniteOperation(value);
            }));

            _autoCrafterPanels = GameObjectHelpers.FindGameObject(gameObject, "AutoCrafterPanels");
            _standByPanel = GameObjectHelpers.FindGameObject(gameObject, "StandByPanel");
            _mainPanel = GameObjectHelpers.FindGameObject(gameObject, "MainPanel");
            _requiredItemsPanel = GameObjectHelpers.FindGameObject(gameObject, "RequiredItemsPanel");
            _missingItemsPanel = GameObjectHelpers.FindGameObject(gameObject, "MissingItemsPanel");

            #region Search

            var inputField = InterfaceHelpers.FindGameObject(gameObject, "SearchInputField");
            var searchField = inputField.GetComponent<InputField>();
            searchField.onValueChanged.AddListener(UpdateSearch);

            _searchBTN = GameObjectHelpers.FindGameObject(gameObject, "SearchBTN")?.GetComponent<Button>();
            _searchBTN?.onClick.AddListener((() =>
            {
                uGUI.main.userInput.RequestString("Search For Item", "Search", _currentSearchString, 100,
                    (text =>
                    {
                        _currentSearchString = text;
                        UpdateSearch(_currentSearchString);
                    }));

            }));

            #endregion

            _grid = gameObject.GetComponentInChildren<GridLayoutGroup>()?.gameObject;

            _craftBTN = GameObjectHelpers.FindGameObject(gameObject, "CraftBTN").GetComponent<Button>();
            _craftBTN.onClick.AddListener((() =>
            {
                if (_sender != null)
                {
                    _sender.CraftMachine.StartCrafting(new CraftingOperation(_sender.UnitID,_currentTechType, Int32.Parse(_amountInput.text)));
                    _craftBTN.interactable = !_sender.CraftMachine.IsCrafting();
                    _operation = _sender.CraftMachine.GetOperation();
                    GenerateNeededItemsList();
                    UpdateAmountLabel();
                }
            }));


            var openBTN = GameObjectHelpers.FindGameObject(gameObject, "OpenBTN").GetComponent<Button>();
            openBTN.onClick.AddListener((() =>
            {
                _autoCrafterPanels.SetActive(true);
            }));

            var autoCrafterCloseBTN = GameObjectHelpers.FindGameObject(gameObject, "AutoCrafterCloseBTN").GetComponent<Button>();
            autoCrafterCloseBTN.onClick.AddListener((() =>
            {
                _autoCrafterPanels.SetActive(false);
            }));

            var closeBTN = GameObjectHelpers.FindGameObject(gameObject, "CloseBTN").GetComponent<Button>();
            closeBTN.onClick.AddListener((Hide));

            _autoCrafterGrid = GameObjectHelpers.FindGameObject(gameObject, "AutoCraftersContent");
            _pendingItemsGrid = GameObjectHelpers.FindGameObject(gameObject, "PendingItemsContent");
            _neededItemsGrid = GameObjectHelpers.FindGameObject(gameObject, "NeededItemsContent");
            _missingItemsGrid = GameObjectHelpers.FindGameObject(gameObject, "MissingItemsContent");

            _cancelBtn = GameObjectHelpers.FindGameObject(gameObject, "CancelBTN").GetComponent<Button>();

            _cancelBtn.onClick.AddListener((() =>
            {
                if (_sender != null)
                {
                    _sender.CraftMachine?.CancelOperation();
                    _sender.CancelLinkedCraftersOperations();
                    _operation = null;
                    ClearNeedItemsList();
                    ClearMissingItemsList();
                    UpdateAmountLabel();
                    ResetInteractions();
                }
            }));

            InvokeRepeating(nameof(UpdateAutoCrafterToggleInteraction),1,1);
        }

        private void ChangeAutoCrafterToggleStates()
        {
            foreach (var item in _currentAutocrafterToggles)
            {
                var toggle = item.Value;

                if (!toggle.interactable && toggle.isOn)
                {
                    toggle.isOn = false;
                }
            }
        }

        private void UpdateAutoCrafterToggleInteraction()
        {
            foreach (var item in _currentAutocrafterToggles)
            {
                var toggle = item.Value;
                
                if(_sender?.CraftMachine == null) return;

                if (_sender.CraftMachine.IsCrafting())
                {
                    toggle.interactable = false;
                }
                else if (_sender.GetLinkedCrafters().Contains(item.Key))
                {
                    toggle.interactable = true;
                }
                else
                {
                    var fcsdevice = _sender.Manager.FindDeviceById(item.Key);

                    if (fcsdevice != null)
                    {
                        var autoCrafter = (AutoCrafterController)fcsdevice;
                        toggle.interactable = !autoCrafter.GetIsStandBy() && !autoCrafter.CraftMachine.IsCrafting();
                    }
                }
            }
        }

        private void UpdateCrafterItemsStates(bool value)
        {
            foreach (uGUI_FCSDisplayItem item in _currentItems)
            {
                item.SetInteractable(value);
            }
        }

        private void ClearNeedItemsList()
        {
            foreach (Transform child in _neededItemsGrid.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private void ClearMissingItemsList()
        {
            foreach (Transform child in _missingItemsGrid.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private void Start()
        {
            gameObject.SetActive(false);
            InvokeRepeating(nameof(UpdateInteractableStates), .5f, .5f);
        }

        private void UpdateInteractableStates()
        {
            if (_sender == null || _amountInput == null || _craftBTN == null || _cancelBtn == null || _isRecursiveToggle == null) return;
            
            //Amount Input
            if (_isRecursive || _sender.CraftMachine.IsCrafting() || _sender.GetIsStandBy())
            {
                _amountInput.interactable = false;
            }
            else
            {
                _amountInput.interactable = true;
            }

            //Craft Button
            if (_sender.GetIsStandBy() || _sender.CraftMachine.IsCrafting())
            {
                _craftBTN.interactable = false;
            }
            else
            {
                _craftBTN.interactable = true;
            }

            //Cancel Button
            if (_sender.GetIsStandBy() || !_sender.CraftMachine.IsCrafting())
            {
                _cancelBtn.interactable = false;
            }
            else
            {
                _cancelBtn.interactable = true;
            }


            //Recursive Toggle Button
            if (_sender.GetIsStandBy())
            {
                _isRecursiveToggle.interactable = false;
                _limitInput.interactable = false;
                _isLimitedToggle.interactable = false;
                _isInfiniteCraft.interactable = false;
                _isCraftAmountToggle.interactable = false;
                _isRecursiveToggle.SetIsOnWithoutNotify(false);
            }
            else
            {
                _isRecursiveToggle.interactable = true;
                _limitInput.interactable = true;
                _isLimitedToggle.interactable = true;
                _isInfiniteCraft.interactable = true;
                _isCraftAmountToggle.interactable = true;
            }

            UpdateCrafterItemsStates(!_sender.GetIsStandBy());
            _isRecursiveToggle.interactable = !_sender.CraftMachine.IsCrafting();
            _isCraftAmountToggle.interactable = !_sender.CraftMachine.IsCrafting();
        }

        private void ResetInteractions()
        {
            _amountInput.interactable = true;
            _cancelBtn.interactable = true;
            _craftBTN.interactable = true;
            _isRecursiveToggle.interactable = true;
            _isCraftAmountToggle.interactable = true;
        }

        private void OnLoadDisplay()
        {
            try
            {
                if (GenerateCraftables()) return;
                if (GenerateCrafterList()) return;
                if (GenerateNeededItemsList()) return;
                GeneratePendingItemsList();
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }

        }

        private void GeneratePendingItemsList()
        {
            if (_pendingItemsGrid == null || _sender?.CraftMachine == null) return;

            foreach (Transform child in _pendingItemsGrid.transform)
            {
                Destroy(child.gameObject);
            }

            var grouped = _sender.CraftMachine.GetPendingItems();

            if (grouped == null) return;

            for (int i = 0; i < grouped.Count(); i++)
            {
                var item = GameObject.Instantiate(_craftToggleItemPrefab);
                var dsItem = item.AddComponent<uGUI_FCSDisplayItem>();
                dsItem.Initialize(grouped.ElementAt(i), true);
                item.transform.SetParent(_pendingItemsGrid.transform, false);
                item.GetComponent<Toggle>().interactable = false;
                if (i == 0)
                {
                    dsItem.Select();
                }
            }
        }

        private bool GenerateNeededItemsList()

        {
            if (_neededItemsGrid == null || _sender?.CraftMachine == null) return true;
    
            ClearNeedItemsList();

            var grouped = _sender.CraftMachine.GetNeededItems();

            if (grouped == null) return true;

            for (int i = 0; i < grouped.Count(); i++)
            {
                var item = GameObject.Instantiate(_craftButtonItemPrefab);
                var dsItem = item.AddComponent<uGUI_FCSDisplayItem>();
                var techType = grouped.ElementAt(i).Key;
                var amount = grouped.ElementAt(i).Value;
                var fcsToolTip = item.EnsureComponent<FCSToolTip>();
                fcsToolTip.TechType = techType;
                fcsToolTip.RequestPermission += () => true;
                dsItem.Initialize(techType, true);
                item.transform.SetParent(_neededItemsGrid.transform, false);
                dsItem.UpdateAmount(amount);
                var hover = GameObjectHelpers.FindGameObject(item, "Hover");
                var toggle = item.GetComponent<Button>();

                if (_sender.CraftMachine.IsCrafting())
                {
                    var state = (CrafterCraftingState)_sender.StateMachine.CurrentState;
                    
                    if (state.IsCraftRecipeFulfilledAdvanced(techType))
                    {
                        hover.SetActive(true);
                    }
                    else
                    {
                        hover.SetActive(state.IsCraftRecipeFulfilledAdvanced(techType) || _sender.Manager.GetItemCount(techType) >= amount);
                    }
                }


                toggle.onClick.AddListener(() =>
                {
                    ClearMissingItemsList();
                    GenerateMissingItemsList(techType, amount);
                });
            }

            return false;
        }

        private bool GenerateMissingItemsList(TechType techType, int amount)

        {
            if (_missingItemsGrid == null || _sender?.CraftMachine == null) return true;

            ClearMissingItemsList();

            var craftData = CraftDataHandler.GetTechData(techType);

            if (craftData != null)
            {


                QuickLogger.Debug($"Craft Data Result = L{craftData.linkedItemCount} | IC{craftData.ingredientCount} | CA {craftData.craftAmount}");

                for (int i = 0; i < craftData.ingredientCount; i++)
                {
                    var ingredient = craftData.GetIngredient(i);
                    
                    var item = GameObject.Instantiate(_craftButtonItemPrefab);
                    var dsItem = item.AddComponent<uGUI_FCSDisplayItem>();
                    var fcsToolTip = item.EnsureComponent<FCSToolTip>();
                    fcsToolTip.TechType = ingredient.techType;
                    fcsToolTip.RequestPermission += () => true;
                    dsItem.Initialize(ingredient.techType, false);
                    item.transform.SetParent(_missingItemsGrid.transform, false);
                    dsItem.Deselect();

                    var calculatedAmount = CalculateAmount(craftData.craftAmount, amount, ingredient.amount);
                    dsItem.UpdateAmount(calculatedAmount); 
                    
                    if (_sender.CraftMachine.IsCrafting())
                    {
                        var state = (CrafterCraftingState)_sender.StateMachine.CurrentState;
                        var isFullFilled = state.IsCraftRecipeFulfilledAdvanced(ingredient.techType) || _sender.Manager.GetItemCount(ingredient.techType) >= calculatedAmount;
                        if (isFullFilled)
                        {
                            Destroy(item);
                        }
                        else
                        {
                                item.GetComponent<Button>().interactable = false;
                               dsItem.Deselect();
                        }
                    }
                }
            }
            
            return false;
        }

        private int CalculateAmount(int itemAmount, int amountMultiple,int ingredientAmount)
        {
            int totalNeeded = amountMultiple * ingredientAmount;
            int amount = 0;
            int i = 0;

            while (amount < totalNeeded)
            {
                amount += itemAmount;
                i++;
            }

            return i;
        }

        private bool GenerateCrafterList()
        {
            _currentAutocrafterToggles.Clear();

            if (_autoCrafterGrid == null) return true;

            foreach (Transform child in _autoCrafterGrid.transform)
            {
                Destroy(child.gameObject);
            }

            var grouped = _sender.Manager.GetDevices(AutoCrafterPatch.AutoCrafterTabID);


            foreach (FcsDevice device in grouped)
            {
                if (device == _sender) continue;

                var item = GameObject.Instantiate(_fcsToggleItemPrefab);
                var toggleText = item.GetComponentInChildren<Text>();
                toggleText.text = device.UnitID;
                item.transform.SetParent(_autoCrafterGrid.transform, false);
                var toggle = item.gameObject.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener((value =>
                    {
                        var deviceName = toggle.GetComponentInChildren<Text>()?.text;
                        var device = FCSAlterraHubService.PublicAPI.FindDevice(deviceName);
                        if (device.Value != null)
                        {
                            var crafter = (AutoCrafterController) device.Value;

                            if (value)
                            {
                                _sender.AddLinkedDevice(crafter);
                                crafter.SetIsStandBy(true);
                            }
                            else
                            {
                                _sender.RemoveLinkedDevice(crafter);
                                crafter.SetIsStandBy(false);
                                crafter.CraftMachine?.CancelOperation();
                            }
                        }
                        else
                        {
                         QuickLogger.DebugError($"Failed to find autocrafter with ID: {deviceName}",true);   
                        }
                    }));
                _currentAutocrafterToggles.Add(device.UnitID,toggle);
            }

            return false;
        }

        private bool GenerateCraftables()
        {
            if (_grid == null) return true;
            
            var grouped = Mod.Craftables;

            if (!string.IsNullOrEmpty(_currentSearchString?.Trim()))
            {
                grouped = grouped.Where(p =>
                        TechTypeExtensions.Get(Language.main, p).ToLower().Contains(_currentSearchString.Trim().ToLower()))
                    .ToList();

                foreach (uGUI_FCSDisplayItem child in _currentItems)
                {
                    child.Hide();
                }
            }
            else
            {
                _currentItems.Clear();
                foreach (Transform child in _grid.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (TechType techType in grouped)
            {
                var item = GameObject.Instantiate(_craftToggleItemPrefab);

                var fcsToolTip = item.EnsureComponent<FCSToolTip>();
                fcsToolTip.TechType = techType;
                fcsToolTip.RequestPermission += () => true;

                var dsItem = item.EnsureComponent<uGUI_FCSDisplayItem>();
                
                dsItem.Initialize(techType, true);
                item.transform.SetParent(_grid.transform, false);
                
                var toggle = item.GetComponent<Toggle>();
                toggle.group = _toggleGroup;

                toggle.onValueChanged.AddListener((value =>
                {
                    if (value)
                    {
                        _currentTechType = techType;
                    }
                }));

                if (_currentTechType == techType)
                {
                    dsItem.Select();
                }
                else if (_currentTechType == TechType.None)
                {
                    _currentTechType = techType;
                    dsItem.Select();
                }

                _currentItems.Add(dsItem);
            }

            return false;
        }

        private void UpdateSearch(string newSearch)
        {
            _currentSearchString = newSearch;
            OnLoadDisplay();
        }
        
        internal void Show(AutoCrafterController sender)
        {
            try
            {

                _autoCrafterPanels.SetActive(false);
                
                ResetInteractions();

                if (Time.timeSinceLevelLoad < 1f)
                {
                    return;
                }
                
                if (!gameObject.activeInHierarchy)
                {
                    gameObject.SetActive(true);
                    Select();
                }
                
                GetCraftables();
                
                _sender = sender;

                if (_sender.GetIsStandBy())
                {
                    _standByPanel.SetActive(true);
                    _mainPanel.SetActive(false);
                    _requiredItemsPanel.SetActive(false);
                    _missingItemsPanel.SetActive(false);
                    _standByTxt.text = AuxPatchers.StandbyTxt(_sender.GetParentCrafters().ToArray());
                }
                else
                {
                    _standByPanel.SetActive(false);
                    _mainPanel.SetActive(true);
                    _requiredItemsPanel.SetActive(true);
                    _missingItemsPanel.SetActive(true);
                }


                _operation = _sender.CraftMachine.GetOperation();
                _isRecursive = _sender.GetIsRecursive();

                _isRecursiveToggle.Set(_isRecursive);
                _continuousLimitSection.gameObject.SetActive(_isRecursive);
                _isInfiniteCraft.gameObject.SetActive(_isRecursive);


                _isLimitedToggle.SetIsOnWithoutNotify(_sender.GetIsLimitedOperation());
                _isInfiniteCraft.SetIsOnWithoutNotify(_sender.GetIsInfiniteOperation());

                
                _currentTechType = _operation?.TechType ?? TechType.None;
            
                OnLoadDisplay();

                CheckChildCrafters();
                
                UpdateAmountLabel();
                
                _limitInput.SetTextWithoutNotify(_sender.CraftMachine.GetLimitAmount().ToString());
                _amountInput.text = _operation?.Amount.ToString() ?? "1";
                _sender.CraftMachine.OnItemCrafted += GeneratePendingItemsList;
                _sender.CraftMachine.OnNeededItemFound += GenerateNeededItemsList;
                _sender.CraftMachine.OnItemCrafted += OnItemCrafted;
                UpdateIsInUse(true);

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                UpdateIsInUse(false);
                Hide();
            }
        }

        private void OnItemCrafted()
        {
            UpdateAmountLabel();
        }

        private void UpdateAmountLabel(bool reset = false)
        {
            if (_craftingAmount == null) return;

            if (_sender != null)
            {
                if (_sender.GetIsRecursive())
                {
                    _craftingAmount.text = "\u221E";
                    return;
                }
            }
            
            _craftingAmount.text = reset ? "Operation: 0/0" : $"Operation: {_operation?.AmountCompleted ?? 0}/{_operation?.Amount ?? 0}";
        }

        internal void OnComplete()
        {
            _operation = null;
            UpdateAmountLabel(true);
            ClearNeedItemsList();
            ClearMissingItemsList();
            ResetInteractions();

        }

        private void CheckChildCrafters()
        {
            if(_sender == null) return;
            
            ChangeAutoCrafterToggleStates();

            foreach (var connectedCrafter in _sender.GetLinkedCrafters())
            {
                if (_currentAutocrafterToggles.ContainsKey(connectedCrafter))
                {
                    _currentAutocrafterToggles[connectedCrafter].SetIsOnWithoutNotify(true);
                }
            }
        }
        
        internal void GetCraftables()
        {
            Mod.Craftables.Clear();

            var fabricator = CraftTree.GetTree(CraftTree.Type.Fabricator);
            GetCraftTreeData(fabricator.nodes);

            var cyclopsFabricator = CraftTree.GetTree(CraftTree.Type.CyclopsFabricator);
            GetCraftTreeData(cyclopsFabricator.nodes);

            var workbench = CraftTree.GetTree(CraftTree.Type.Workbench);
            GetCraftTreeData(workbench.nodes);

            var maproom = CraftTree.GetTree(CraftTree.Type.MapRoom);
            GetCraftTreeData(maproom.nodes);

            var seamothUpgrades = CraftTree.GetTree(CraftTree.Type.SeamothUpgrades);
            GetCraftTreeData(seamothUpgrades.nodes);
        }

        private void GetCraftTreeData(CraftNode innerNodes)
        {
            foreach (CraftNode craftNode in innerNodes)
            {
                if (string.IsNullOrWhiteSpace(craftNode.id)) continue;
                if (craftNode.id.Equals("CookedFood") || craftNode.id.Equals("CuredFood")) return;
                if (craftNode.techType0 != TechType.None)
                {
                    if (!CrafterLogicHelper.IsItemUnlocked(craftNode.techType0, true)) continue;
                    Mod.Craftables.Add(craftNode.techType0);
                }

                if (craftNode.childCount > 0)
                {
                    GetCraftTreeData(craftNode);
                }
            }
        }

        public void Hide()
        {
            Deselect();
        } 

        private void Clear()
        {
            if (_sender != null)
            {
                _sender.CraftMachine.OnItemCrafted -= GeneratePendingItemsList;
                _sender.CraftMachine.OnNeededItemFound -= GenerateNeededItemsList;
                _sender.CraftMachine.OnItemCrafted -= OnItemCrafted;
                _sender = null;
                _operation = null;
            }
        }

        public override void OnDeselect()
        {
            base.OnDeselect();
            gameObject.SetActive(false);
            Clear();
            gameObject.SetActive(false);
            UpdateIsInUse(false);
        }

        private void UpdateIsInUse(bool isInUse)
        {
            _isOpen = isInUse;
        }

        public bool OnButtonDown(GameInput.Button button)
        {
            if (button == GameInput.Button.UICancel || button == GameInput.Button.PDA)
            {
                Deselect();
                return true;
            }
            return false;
        }

        public override void OnReselect(bool lockMovement)
        {
            base.OnReselect(true);
        }
    }
}