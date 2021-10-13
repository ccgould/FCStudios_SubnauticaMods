using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Model.GUI;
using FCS_AlterraHub.Mods.OreConsumer.Mono;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.AutoCrafter.Buildable;
using FCS_ProductionSolutions.Mods.AutoCrafter.Helpers;
using FCS_ProductionSolutions.Mods.AutoCrafter.Mono;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
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


                var autocrafterGui = Object.Instantiate(ModelPrefab.GetPrefab("uGUI_AutoCrafter"));

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
        private readonly Dictionary<string,Toggle> _currentAutocrafterToggles = new();
        private AutoCrafterController _sender;
        private GameObject _craftToggleItemPrefab;
        private CraftingOperation _operation;
        private string _currentSearchString;
        private ToggleGroup _toggleGroup;
        private Toggle _isStandbyToggle;
        private InputField _amountInput;
        private GameObject _fcsToggleItemPrefab;
        private GameObject _autoCrafterGrid;
        private GameObject _pendingItemsGrid;
        private GameObject _neededItemsGrid;
        private TechType _currentTechType;
        private bool _isRecursive = false;
        private Button _craftBTN;
        private Text _craftingAmount;

        public override void Update()
        {
            base.Update();


            if (focused && GameInput.GetButtonDown(GameInput.Button.PDA))
            {
                Hide();
            }



            UpdateScreen();
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
            _fcsToggleItemPrefab = ModelPrefab.GetPrefabFromGlobal("FCSToggle");
            _toggleGroup = gameObject.GetComponentInChildren<ToggleGroup>();
            _amountInput = GameObjectHelpers.FindGameObject(gameObject, "AmountInputField").GetComponent<InputField>();
            _craftingAmount = GameObjectHelpers.FindGameObject(gameObject, "CraftingAmount").GetComponent<Text>();
            _isStandbyToggle = GameObjectHelpers.FindGameObject( gameObject, "IsStandByToggle").GetComponent<Toggle>();
            _isStandbyToggle.onValueChanged.AddListener((value =>
            {
                if (value)
                {
                    SetIsInteractable(false);
                }
                else
                {
                    SetIsInteractable(true);
                }
            }));
            
            #region Search
            var inputField = InterfaceHelpers.FindGameObject(gameObject, "SearchInputField");
            var searchField = inputField.AddComponent<SearchField>();
            searchField.OnSearchValueChanged += UpdateSearch;
            #endregion

            _grid = gameObject.GetComponentInChildren<GridLayoutGroup>()?.gameObject;
            
            _craftBTN = GameObjectHelpers.FindGameObject(gameObject, "CraftBTN").GetComponent<Button>();
            _craftBTN.onClick.AddListener((() =>
            {
                if (_sender != null)
                {
                    _sender.CraftMachine.StartCrafting(new CraftingOperation(_currentTechType,Int32.Parse(_amountInput.text),_isRecursive));
                    _craftBTN.interactable = !_sender.CraftMachine.IsCrafting();
                    _operation = _sender.CraftMachine.GetOperation();
                    GenerateNeededItemsList();
                    GeneratePendingItemsList();
                    UpdateAmountLabel();
                }
            }));

            var closeBTN = GameObjectHelpers.FindGameObject(gameObject, "CloseBTN").GetComponent<Button>();
            closeBTN.onClick.AddListener((Hide));

            _autoCrafterGrid = GameObjectHelpers.FindGameObject(gameObject, "AutoCraftersContent");
            _pendingItemsGrid = GameObjectHelpers.FindGameObject(gameObject, "PendingItemsContent");
            _neededItemsGrid = GameObjectHelpers.FindGameObject(gameObject, "NeededItemsContent");
            
            GameObjectHelpers.FindGameObject(gameObject, "CancelBTN").GetComponent<Button>().onClick.AddListener((() =>
            {
                if (_sender != null)
                {
                    _sender.CraftMachine?.CancelOperation();
                    _craftBTN.interactable = true;
                    _operation = null;
                    UpdateAmountLabel();
                }
            }));

        }

        private void SetIsInteractable(bool value)
        {
            _amountInput.interactable = value;

            _sender.SetIsStandBy(!value);

            foreach (uGUI_FCSDisplayItem item in _currentItems)
            {
                item.SetInteractable(value);
            }

            ChangeAutoCrafterToggleStates(!value);
        }
        
        private void Start()
        {
            gameObject.SetActive(false);
        }
        
        private void UpdateScreen()
        {
            if (_sender == null || !_isOpen) return;

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

            foreach (Transform child in _neededItemsGrid.transform)
            {
                Destroy(child.gameObject);
            }

            var grouped = _sender.CraftMachine.GetNeededItems();

            if (grouped == null) return true;

            for (int i = 0; i < grouped.Count(); i++)
            {
                var item = GameObject.Instantiate(_craftToggleItemPrefab);
                var dsItem = item.AddComponent<uGUI_FCSDisplayItem>();
                dsItem.Initialize(grouped.ElementAt(i), true);
                item.transform.SetParent(_neededItemsGrid.transform, false);
                item.GetComponent<Toggle>().interactable = false;
            }

            return false;
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
                    toggle.GetComponent<Toggle>().interactable = false;
                    toggle.onValueChanged.AddListener((value =>
                    {
                        var deviceName = toggle.GetComponentInChildren<Text>()?.text;
                        var device = FCSAlterraHubService.PublicAPI.FindDevice(deviceName);
                        if (device.Value != null)
                        {
                            var crafter = (AutoCrafterController) device.Value;

                            if (value)
                            {
                                crafter.AddLinkedDevice(_sender);
                            }
                            else
                            {
                                crafter.RemoveLinkedDevice(_sender);
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

                foreach (TechType techType in grouped)
                {
                    _currentItems.FirstOrDefault(x=>x.GetTechType() == techType)?.Show();
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
                var dsItem = item.AddComponent<uGUI_FCSDisplayItem>();
                item.AddComponent<FCSToolTip>().TechType = techType;
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

        private void Clear()
        {

        }

        internal void Show(AutoCrafterController sender)
        {
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

            _craftBTN.interactable = !_sender.CraftMachine.IsCrafting();

            _isStandbyToggle.SetIsOnWithoutNotify(_sender.GetIsStandBy());
            _operation = _sender.CraftMachine.GetOperation();
            _currentTechType = _operation?.TechType ?? TechType.None;
            
            OnLoadDisplay();
            CheckChildCrafters();
            UpdateAmountLabel();
            _amountInput.text = _operation?.Amount.ToString() ?? "1";
            _sender.CraftMachine.OnItemCrafted += GeneratePendingItemsList;
            _sender.CraftMachine.OnNeededItemFound += GenerateNeededItemsList;
            _sender.CraftMachine.OnComplete += OnComplete;
            _sender.CraftMachine.OnItemCrafted += OnItemCrafted;
            _isOpen = true;
        }

        private void OnItemCrafted()
        {
            UpdateAmountLabel();
        }

        private void UpdateAmountLabel()
        {
            _craftingAmount.text = $"{_operation?.AmountCompleted ?? 0}/{_operation?.Amount ?? 0}";
        }

        private void OnComplete(CraftingOperation obj)
        {
            if (_sender?.CraftMachine == null) return;
            if (_sender.CraftMachine.IsCrafting())
            {
                _craftBTN.interactable = false;
            }
        }

        private void CheckChildCrafters()
        {
            if(_sender == null) return;

            if (_sender.GetIsStandBy())
            {
                ChangeAutoCrafterToggleStates(true);

                foreach (var connectedCrafter in _sender?.GetConnectedCrafters())
                {
                    if (_currentAutocrafterToggles.ContainsKey(connectedCrafter))
                    {
                        _currentAutocrafterToggles[connectedCrafter].SetIsOnWithoutNotify(true);
                    }
                }
            }
        }

        private void ChangeAutoCrafterToggleStates(bool value)
        {
            foreach (var item in _currentAutocrafterToggles)
            {
                var toggle = item.Value;
                toggle.interactable = value;

                if (!toggle.interactable && toggle.isOn)
                {
                    toggle.isOn = false;
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
                //QuickLogger.Debug($"Craftable: {craftNode.id} | {craftNode.string0} | {craftNode.string1} | {craftNode.techType0}");

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
            if (_sender != null)
            {
                _sender.CraftMachine.OnItemCrafted -= GeneratePendingItemsList;
                _sender.CraftMachine.OnNeededItemFound -= GenerateNeededItemsList;
                _sender.CraftMachine.OnItemCrafted -= OnItemCrafted;
                _sender.CraftMachine.OnComplete -= OnComplete;
                _sender = null;
                _operation = null;
            }
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

        public override void OnDeselect()
        {
            base.OnDeselect();
            gameObject.SetActive(false);
            _isOpen = false;
            if (_sender != null)
            {
                //_sender.OnProcessingCompleted -= OnLoadDisplay;
                _sender = null;
            }
        }

        public override void OnReselect(bool lockMovement)
        {
            base.OnReselect(true);
        }
    }
}