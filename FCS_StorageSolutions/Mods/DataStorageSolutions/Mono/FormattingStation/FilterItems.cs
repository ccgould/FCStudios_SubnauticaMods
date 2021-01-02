using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.FormattingStation
{
    internal class FilterItems : MonoBehaviour
    {
        private Text _itemLBL;
        private Text _catergoryLBL;
        private Filter _filter;
        private FCSToggleButton _itemRad;
        private FCSToggleButton _categoryToggle;
        private uGUI_Icon _icon;
        private bool _isInitialized;
        private TechType _setTechType;

        private void Initialize()
        {
            if (_isInitialized) return;
            _itemLBL = InterfaceHelpers.FindGameObject(gameObject, "ItemLBL").GetComponent<Text>();
            _catergoryLBL = InterfaceHelpers.FindGameObject(gameObject, "CatergoryLBL").GetComponent<Text>();
            _itemRad = InterfaceHelpers.FindGameObject(gameObject, "ItemRAD").AddComponent<FCSToggleButton>();
            _itemRad.OnButtonClick += (s, o) =>
            {
                _filter = FilterList.FindFilter(_setTechType);
                _categoryToggle.DeSelect();
            };
            _icon = InterfaceHelpers.FindGameObject(gameObject, "Icon").AddComponent<uGUI_Icon>();
            _icon.sprite = SpriteManager.defaultSprite;
            _itemRad.ButtonMode = InterfaceButtonMode.RadialButton;
            _categoryToggle = InterfaceHelpers.FindGameObject(gameObject, "CatergoryRAD").AddComponent<FCSToggleButton>();
            _categoryToggle.OnButtonClick += (s, o) =>
            {
                _filter = FilterList.FindCategory(_setTechType);
                _itemRad.DeSelect();
            };
            _categoryToggle.ButtonMode = InterfaceButtonMode.RadialButton;
            _isInitialized = true;
        }

        public void Set(TechType techType)
        {
            Initialize();
            _setTechType = techType;
            _filter = FilterList.FindFilter(_setTechType);
            if (_filter == null)
            {
                QuickLogger.Debug("Filter is null");
                return;
            }
            _itemRad.Select();
            _itemLBL.text = AuxPatchers.FilterItemNameFormat(Language.main.Get(techType));
            _categoryToggle?.SetVisible(!_filter.IsUnknown);
            _catergoryLBL.gameObject.SetActive(!_filter.IsUnknown);
            _catergoryLBL.text = AuxPatchers.FilterCategoryNameFormat(FilterList.FindCategory(techType)?.Category);
            _icon.sprite = SpriteManager.Get(techType);
            gameObject.SetActive(true);
            
        }

        public void Reset()
        {
            gameObject.SetActive(false);
        }

        public Filter GetFilter()
        {
            return _filter;
        }
    }
}