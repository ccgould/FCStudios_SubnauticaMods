using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Objects;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_AlterraHub.Helpers
{
    public class ColorPickerDialog : MonoBehaviour
    {
        private int _colorsPerPage;
        private GameObject _colorPageContainer;
        private GameObject _colorItemPrefab;
        private Action<Color> _onButtonClick;
        private Text _colorPageNumber;
        private int _maxColorPage = 1;
        private int _currentColorPage;
        private readonly List<ColorItemButton> _colorItemsTracker = new List<ColorItemButton>();
        private readonly Color _defaultColor = new Color(0.7529412f, 0.7529412f, 0.7529412f, 1f);

        public Action<Color> OnColorChanged;
        private Action<bool> _onInterfaceButton;
        private Color _currentColor;
        private bool _isInitialized;
        private GameObject _hud;

        private void ResetColorSelections(Color color)
        {
            foreach (ColorItemButton colorItemButton in _colorItemsTracker)
            {
                if (colorItemButton.Color != color)
                {
                    colorItemButton.SetIsSelected(false);
                }
            }
        }

        private void SelectSavedColor(Color color)
        {
            var colorMatch = _colorItemsTracker.SingleOrDefault(x => x.Color == color);

            if (colorMatch != null)
            {
                colorMatch.SetIsSelected();
            }
        }

        private void DrawColorPage(int page)
        {
            _currentColorPage = page;

            if (_currentColorPage <= 0)
            {
                _currentColorPage = 1;
            }
            else if (_currentColorPage > _maxColorPage)
            {
                _currentColorPage = _maxColorPage;
            }

            int startingPosition = (_currentColorPage - 1) * _colorsPerPage;
            int endingPosition = startingPosition + _colorsPerPage;

            if (endingPosition > ColorList.Colors.Count)
            {
                endingPosition = ColorList.Colors.Count;
            }

            ClearColorPage();

            for (int i = startingPosition; i < endingPosition; i++)
            {
                var colorID = ColorList.Colors.ElementAt(i);
                CreateColorItem(colorID);
            }

            UpdateColorPaginator();
            SelectSavedColor(GetColor());
        }

        internal void OpenColorPicker(ColorVec4 color)
        {
            if (!_hud.activeSelf)
            {
                _hud.SetActive(true);
            }
        }

        internal void CloseColorPicker()
        {
            _hud.SetActive(false);
        }

        private void CreateColorItem(ColorVec4 color)
        {
            GameObject itemDisplay = Instantiate(AlterraHub.ColorItemPrefab);
            itemDisplay.transform.SetParent(_colorPageContainer.transform, false);
            itemDisplay.GetComponentInChildren<Image>().color = color.Vector4ToColor();

            //var itemButton = itemDisplay.AddComponent<ColorItemButton>();
            //itemButton.OnButtonClick += (text, itemColor) => { _currentColor = (Color)itemColor; };

            //if (!_colorItemsTracker.Contains(itemButton))
            //    _colorItemsTracker.Add(itemButton);
        }

        private void ClearColorPage()
        {
            _colorItemsTracker.Clear();
            for (int i = 0; i < _colorPageContainer.transform.childCount; i++)
            {
                Destroy(_colorPageContainer.transform.GetChild(i).gameObject);
            }
        }

        private void UpdateColorPaginator()
        {
            CalculateNewMaxColorPages();
            if (_colorPageNumber == null) return;
            _colorPageNumber.text = $"{_currentColorPage.ToString()} | {_maxColorPage}";
        }

        private void CalculateNewMaxColorPages()
        {
            _maxColorPage = Mathf.CeilToInt((ColorList.Colors.Count - 1) / _colorsPerPage) + 1;
            if (_currentColorPage > _maxColorPage)
            {
                _currentColorPage = _maxColorPage;
            }
        }

        public void SetColorFromSave(Color color)
        {
            _currentColor = color;
            SelectSavedColor(color);
        }
        
        public Color GetColor()
        {
           return _currentColor;
        }
        
        public void ChangeColorPageBy(int amount)
        {
            DrawColorPage(_currentColorPage + amount);
        }
        
        public void SetupGrid(GameObject parent, int colorsPerPage, string prevBtnNAme = "PrevBTN", string nextBtnName = "NextBTN" , string gridName = "Grid", string paginatorName = "Paginator")
        {
            if (_isInitialized) return;
            _hud = gameObject.FindChild("Hud");
            _colorsPerPage = colorsPerPage;
            QuickLogger.Info($"GridName: {gameObject.name}");
            _colorPageContainer = InterfaceHelpers.FindGameObject(gameObject, gridName);
            //_colorPageNumber = InterfaceHelpers.FindGameObject(gameObject, paginatorName)?.GetComponent<Text>();

            #region Prev Color Button

            var prevBTNObj = InterfaceHelpers.FindGameObject(gameObject, prevBtnNAme);
            var prevColorBtn = prevBTNObj.EnsureComponent<UIButton>();

            prevColorBtn.OnClick+= color =>
            {
                ChangeColorPageBy(-1);
            };
            #endregion

            #region Next Color Button
            var nextColorBtn = InterfaceHelpers.FindGameObject(gameObject, nextBtnName).EnsureComponent<UIButton>(); ;
            nextColorBtn.OnClick += color =>
            {
                ChangeColorPageBy(-1);
            };
            #endregion

            var doneBTN = InterfaceHelpers.FindGameObject(gameObject, "Button").EnsureComponent<UIButton>();
            doneBTN.OnClick += color =>
            {
                CloseColorPicker();
            };

            var hudTransform = GameObject.Find("ScreenCanvas").transform.Find("HUD");
            gameObject.transform.SetParent(hudTransform, false);
            gameObject.transform.SetSiblingIndex(0);
            _isInitialized = true;

            DrawColorPage(1);
        }
    }

    public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        private Image _image;
        private bool _hover;
        private static readonly Color HoverColor = new Color(0.956f, 0.796f, 0.258f);
        internal Color ColorValue;

        private void Awake()
        {
            _image = gameObject.GetComponent<Image>();
        }

        private void Update()
        {
            _image.color = _hover ? HoverColor : Color.white;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hover = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {

        }

        public void OnPointerUp(PointerEventData eventData)
        {

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(ColorValue);
        }

        public Action<Color> OnClick { get; set; }
    }
}
