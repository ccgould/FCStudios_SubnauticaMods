using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Model.Converters;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Enums;
using FCS_HomeSolutions.Mods.Stove.Struct;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.Microwave.Mono
{
    internal class CookerController : MonoBehaviour
    {
        private StorageContainer _storageContainer;
        private HashSet<TechType> _allowedTech;
        private readonly Dictionary<TechType, CookingItem> _knownCookingData = new();
        private Queue<CookingItem> _pendingItems = new();
        private CookingItem _pendingItem;
        private float _cookingTime = -1;
        private InterfaceInteraction _interfaceInteraction;
        private Text _textDisplay;
        public CookingMode CookingMode;
        public GameObject DummyFoodObject;
        private const float MAX_COOKING_TIME = 60f;
        public int SlotCountX;
        public int SlotCountY;
        private float _maxCookingTime;


        private HashSet<TechType> _rawTechTypes = new HashSet<TechType>
        {
#if SUBNAUTICA
            TechType.Bladderfish,
            TechType.Boomerang,
            TechType.Eyeye,
            TechType.GarryFish,
            TechType.HoleFish,
            TechType.Hoopfish,
            TechType.Hoverfish,
            TechType.LavaBoomerang,
            TechType.Oculus,
            TechType.Peeper,
            TechType.LavaEyeye,
            TechType.Reginald,
            TechType.Spadefish,
            TechType.Spinefish
#else
            TechType.ArcticPeeper,
            TechType.ArrowRay,
            TechType.Bladderfish,
            TechType.Boomerang,
            TechType.DiscusFish,
            TechType.FeatherFish,
            TechType.FeatherFishRed,
            TechType.Hoopfish,
            TechType.NootFish,
            TechType.Spinefish,
            TechType.SpinnerFish,
            TechType.Symbiote,
            TechType.Triops
#endif
        };

        private HashSet<TechType> _curedTechTypes = new HashSet<TechType>
        {
#if SUBNAUTICA
            TechType.CuredBladderfish,
            TechType.CuredBoomerang,
            TechType.CuredEyeye,
            TechType.CuredGarryFish,
            TechType.CuredHoleFish,
            TechType.CuredHoopfish,
            TechType.CuredHoverfish,
            TechType.CuredLavaBoomerang,
            TechType.CuredOculus,
            TechType.CuredPeeper,
            TechType.CuredLavaEyeye,
            TechType.CuredReginald,
            TechType.CuredSpadefish,
            TechType.CuredSpinefish,
#else
            TechType.CuredArcticPeeper,
            TechType.CuredArrowRay,
            TechType.CuredBladderfish,
            TechType.CuredBoomerang,
            TechType.CuredDiscusFish,
            TechType.CuredFeatherFish,
            TechType.CuredFeatherFishRed,
            TechType.CuredHoopfish,
            TechType.CuredNootFish,
            TechType.CuredSpinefish,
            TechType.CuredSpinnerfish,
            TechType.CuredSymbiote,
            TechType.CuredTriops
#endif
        };

        private void Update()
        {
            if (WorldHelpers.CheckIfPaused() || _interfaceInteraction == null || _storageContainer == null ||
                _storageContainer.container == null) return;

            _storageContainer.enabled = !_interfaceInteraction.IsInRange;

            if (_cookingTime > 0)
            {
                _storageContainer.enabled = false;
                _cookingTime -= Time.deltaTime;
                _maxCookingTime -= Time.deltaTime;
                UpdateTimerUI(_maxCookingTime);
                if (_cookingTime <= 0 && _pendingItem.ReturnItem != TechType.None &&
                    _pendingItem.TechType != TechType.None)
                {
                    if (_storageContainer.container.DestroyItem(_pendingItem.TechType))
                    {
#if SUBNAUTICA_STABLE
                        _storageContainer.container.UnsafeAdd(_pendingItem.TechType.ToInventoryItem());
#else
                        StartCoroutine(_pendingItem.ReturnItem.AddTechTypeToContainerUnSafe(_storageContainer.container));
#endif
                        _pendingItem = new CookingItem();
                        _storageContainer.container.DestroyItem(TechType.Salt);
                        if (_pendingItems.Any())
                        {
                            _pendingItem = _pendingItems.Dequeue();
                            _cookingTime = MAX_COOKING_TIME;
                            return;
                        }

                        _storageContainer.enabled = true;
                    }
                }
            }
        }

        private void Awake()
        {
            _interfaceInteraction = gameObject.GetComponentInChildren<Canvas>().gameObject
                .AddComponent<InterfaceInteraction>();
            _storageContainer = GetComponent<StorageContainer>();
            _storageContainer.container.allowedTech = GetAllowedTech();
            _storageContainer.container.onAddItem += Container_onAddItem;
            _storageContainer.container.onRemoveItem += Container_onRemoveItem;
            _storageContainer.container.Resize(SlotCountX, SlotCountY);

            var startBTN = GameObjectHelpers.FindGameObject(gameObject, "StartBTN").GetComponent<Button>();
            _textDisplay = GameObjectHelpers.FindGameObject(gameObject, "InputField").GetComponentInChildren<Text>();
            UpdateTimerUI(0);

            startBTN.onClick.AddListener((() =>
            {
                if (_storageContainer.container.Any())
                {
                    //Check if enough salt
                    var saltCount = _storageContainer.container.GetCount(TechType.Salt);
                    var rawItemsCount = GetRawItemsCount();


                    if (rawItemsCount <= 0)
                    {
                        QuickLogger.ModMessage("Please add a raw fish to process.");
                        return;
                    }

                    if (CookingMode == CookingMode.Curing)
                    {
                        if (saltCount >= rawItemsCount)
                        {
                            StartCookingProcess(rawItemsCount);
                        }
                        else
                        {
                            QuickLogger.ModMessage("Please add more salt to start curing.");
                        }
                    }
                    else
                    {
                        StartCookingProcess(rawItemsCount);
                    }
                }
            }));


            var ejectBTN = GameObjectHelpers.FindGameObject(gameObject, "EjectBTN").GetComponent<Button>();
            ejectBTN.onClick.AddListener((() =>
            {
                _pendingItem = new CookingItem();
                _cookingTime = 0;
                _storageContainer.enabled = true;
                _storageContainer.Open(Player.main.transform);
                UpdateTimerUI(0);
            }));
            DummyFoodObject?.SetActive(false);
        }

        private void StartCookingProcess(int rawItemsCount)
        {
            for (int i = _storageContainer.container.count - 1; i >= 0; i--)
            {
                var item = _storageContainer.container.ElementAt(i);
                if (item.item.GetTechType() == TechType.Salt) continue;
                var techType = item.item.GetTechType();
                _pendingItems.Enqueue(GetCookingItemData(techType));
                _storageContainer.container.DestroyItem(techType);
            }

            _cookingTime = MAX_COOKING_TIME;
            _maxCookingTime = MAX_COOKING_TIME * rawItemsCount;
            UpdateTimerUI(_maxCookingTime);
            _pendingItem = _pendingItems.Dequeue();
        }

        private int GetCuredItemsCount()
        {
            int amount = 0;
            foreach (InventoryItem item in _storageContainer.container)
            {
                if (_curedTechTypes.Contains(item.item.GetTechType()))
                {
                    amount++;
                }
            }

            return amount;
        }

        private int GetRawItemsCount()
        {
            int amount = 0;
            foreach (InventoryItem item in _storageContainer.container)
            {
                if (_rawTechTypes.Contains(item.item.GetTechType()))
                {
                    amount++;
                }
            }

            return amount;
        }

        private void UpdateTimerUI(float seconds)
        {
            _textDisplay.text = TimeConverters.SecondsToMS(seconds);
        }

        private void Container_onRemoveItem(InventoryItem item)
        {
            if (GetRawItemsCount() <= 0 && GetCuredItemsCount() <= 0)
            {
                DummyFoodObject?.SetActive(false);
            }
        }

        private void Container_onAddItem(InventoryItem item)
        {
            if (_rawTechTypes.Contains(item.item.GetTechType()))
            {
                DummyFoodObject?.SetActive(true);
            }
        }

        public CookingItem GetCookingItemData(TechType techType)
        {
            if (_knownCookingData.ContainsKey(techType))
            {
                return _knownCookingData[techType];
            }

            var newCookingData = new CookingItem { TechType = techType };

#if SUBNAUTICA
            TechType foodData = TechType.None;
            if (CraftData.cookedCreatureList.ContainsKey(techType))
            {
                foodData = CraftData.cookedCreatureList[techType];
#else
            var foodData = TechData.GetProcessed(techType);
            if (foodData != TechType.None)
            {
#endif
                if (CookingMode == CookingMode.Cooking) newCookingData.ReturnItem = foodData;
                newCookingData.CookedItem = foodData;
            }

            if (Mod.CuredCreatureList.ContainsKey(techType))
            {
                foodData = Mod.CuredCreatureList[techType];
                if (CookingMode == CookingMode.Curing) newCookingData.ReturnItem = foodData;
                newCookingData.CuredItem = foodData;
            }

            if (Mod.CustomFoods.ContainsKey(techType))
            {
                foodData = Mod.CustomFoods[techType];
                if (CookingMode == CookingMode.Custom) newCookingData.ReturnItem = foodData;
                newCookingData.CustomItem = foodData;
            }

            if (newCookingData.CustomItem != TechType.None || newCookingData.CookedItem != TechType.None ||
                newCookingData.CuredItem != TechType.None)
            {
                _knownCookingData.Add(techType, newCookingData);
                return newCookingData;
            }

            return new CookingItem();
        }

        private float GetCookingTime()
        {
#if SUBNAUTICA
            if (CraftData.GetCraftTime(_pendingItem.ReturnItem, out var duration))
#else
            if (TechData.GetCraftTime(_pendingItem.ReturnItem, out var duration))
#endif
            {
                duration = Mathf.Max(2.7f, duration);
            }
            else
            {
                duration = 2.7f;
            }

            QuickLogger.Debug($"Cooking Time Set to: {TimeConverters.SecondsToMS(duration)}");
            return duration;
        }

        private HashSet<TechType> GetAllowedTech()
        {
            if (_allowedTech == null)
            {
#if SUBNAUTICA
                _allowedTech = new HashSet<TechType>
                {
                    TechType.Bladderfish,
                    TechType.Boomerang,
                    TechType.Eyeye,
                    TechType.GarryFish,
                    TechType.HoleFish,
                    TechType.Hoopfish,
                    TechType.Hoverfish,
                    TechType.LavaBoomerang,
                    TechType.Oculus,
                    TechType.Peeper,
                    TechType.LavaEyeye,
                    TechType.Reginald,
                    TechType.Spadefish,
                    TechType.Spinefish,
                    TechType.Salt
                };
#else
                _allowedTech = new HashSet<TechType>
                {
                    TechType.ArcticPeeper,
                    TechType.ArrowRay,
                    TechType.Bladderfish,
                    TechType.Boomerang,
                    TechType.DiscusFish,
                    TechType.FeatherFish,
                    TechType.FeatherFishRed,
                    TechType.Hoopfish,
                    TechType.NootFish,
                    TechType.Spinefish,
                    TechType.SpinnerFish,
                    TechType.Symbiote,
                    TechType.Triops,
                    TechType.Salt
                };
#endif

                //foreach (var item in CraftData.cookedCreatureList)
                //{
                //    _allowedTech.Add(item.Key);
                //}

                //foreach (var item in Mod.CuredCreatureList)
                //{
                //    _allowedTech.Add(item.Key);
                //}

                //foreach (var item in Mod.CustomFoods)
                //{
                //    _allowedTech.Add(item.Key);
                //}
            }

            return _allowedTech;
        }
    }
}