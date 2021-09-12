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
        private CookingItem _pendingItem;
        private float _cookingTime = -1;
        private InterfaceInteraction _interfaceInteraction;
        private Text _textDisplay;
        public CookingMode CookingMode;
        public GameObject DummyFoodObject;

        private void Update()
        {
            if (Time.timeScale <= 0 || _interfaceInteraction == null || _storageContainer == null ||
                _storageContainer.container == null) return;

            _storageContainer.enabled = !_interfaceInteraction.IsInRange;

            if (_cookingTime > 0)
            {
                _storageContainer.enabled = false;
                _cookingTime -= Time.deltaTime;
                _textDisplay.text = TimeConverters.SecondsToMS(_cookingTime);
                if (_cookingTime <= 0 && _pendingItem.ReturnItem != TechType.None && _pendingItem.TechType != TechType.None)
                {
                    _storageContainer.container.DestroyItem(_pendingItem.TechType);
                    _storageContainer.container.UnsafeAdd(_pendingItem.ReturnItem.ToInventoryItemLegacy());
                    _pendingItem = new CookingItem();
                    _storageContainer.enabled = true;
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

            var startBTN = GameObjectHelpers.FindGameObject(gameObject, "StartBTN").GetComponent<Button>();
            _textDisplay = GameObjectHelpers.FindGameObject(gameObject, "InputField").GetComponentInChildren<Text>();
            _textDisplay.text = TimeConverters.SecondsToMS(0);

            startBTN.onClick.AddListener((() =>
            {
                if (_storageContainer.container.Any())
                {
                    _cookingTime = 60f; //GetCookingTime();
                    _textDisplay.text = TimeConverters.SecondsToMS(_cookingTime);
                    _pendingItem = GetCookingItemData(_storageContainer.container.ElementAt(0).item.GetTechType());
                }
            }));


            var ejectBTN = GameObjectHelpers.FindGameObject(gameObject, "EjectBTN").GetComponent<Button>();
            ejectBTN.onClick.AddListener((() =>
            {
                _pendingItem = new CookingItem();
                _cookingTime = 0;
                _storageContainer.enabled = true;
                _storageContainer.Open(Player.main.transform);
                _textDisplay.text = TimeConverters.SecondsToMS(0);
            }));
        }

        private void Container_onRemoveItem(InventoryItem item)
        {
            if (!_storageContainer.container.Any())
            {
                DummyFoodObject?.SetActive(false);
            }
        }

        private void Container_onAddItem(InventoryItem item)
        {
            DummyFoodObject?.SetActive(true);
        }

        public CookingItem GetCookingItemData(TechType techType)
        {
            if (_knownCookingData.ContainsKey(techType))
            {
                return _knownCookingData[techType];
            }

            var newCookingData = new CookingItem { TechType = techType};

            if (CraftData.cookedCreatureList.ContainsKey(techType))
            {
                var foodData = CraftData.cookedCreatureList[techType]; 
                if (CookingMode == CookingMode.Cooking) newCookingData.ReturnItem = foodData;
                newCookingData.CookedItem = foodData;
            }

            if (Mod.CuredCreatureList.ContainsKey(techType))
            {
                var foodData = Mod.CuredCreatureList[techType];
                if (CookingMode == CookingMode.Curing) newCookingData.ReturnItem = foodData;
                newCookingData.CuredItem = foodData;
            }

            if (Mod.CustomFoods.ContainsKey(techType))
            {
                var foodData = Mod.CustomFoods[techType];
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
            if (CraftData.GetCraftTime(_pendingItem.ReturnItem, out var duration))
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
#if SUBNAUTICA_STABLE
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
                    TechType.Spinefish
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
