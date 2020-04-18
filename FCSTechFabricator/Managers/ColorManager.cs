using System;
using System.Collections.Generic;
using System.Linq;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSTechFabricator.Configuration;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Objects;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTechFabricator.Managers
{
    public class ColorManager : MonoBehaviour
    {
        private string _bodyMaterial;
        private GameObject _gameObject;
        private int _colorsPerPage;
        private GameObject _colorPageContainer;
        private GameObject _colorItemPrefab;
        private Action<string, object> _onButtonClick;
        private Text _colorPageNumber;
        private int _maxColorPage = 1;
        private int _currentColorPage;
        private readonly List<ColorItemButton> _colorItemsTracker = new List<ColorItemButton>();
        private readonly Color _defaultColor = DefaultConfigurations.DefaultColor;

        public Action<Color> OnColorChanged;
        public string HomeButtonMessage { get; set; }
        private Action<bool> _onInterfaceButton;

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
                LoadColorPicker(colorID);
            }

            UpdateColorPaginator();
            SelectSavedColor(GetColor());
        }

        private void LoadColorPicker(ColorVec4 color)
        {
            GameObject itemDisplay = Instantiate(_colorItemPrefab);
            itemDisplay.transform.SetParent(_colorPageContainer.transform, false);
            itemDisplay.GetComponentInChildren<Image>().color = color.Vector4ToColor();

            var itemButton = itemDisplay.AddComponent<ColorItemButton>();
            itemButton.OnButtonClick = _onButtonClick;
            itemButton.BtnName = "ColorItem";
            itemButton.Color = color.Vector4ToColor();
            if (_onInterfaceButton != null)
            {
                itemButton.OnInterfaceButton = _onInterfaceButton;
            }
            _colorItemsTracker.Add(itemButton);
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

        public void Initialize(GameObject gameObject, string bodyMaterial)
        {
            _gameObject = gameObject;
            _bodyMaterial = bodyMaterial;
            ColorList.AddColor(GetColor());
        }

        public void SetColorFromSave(Color color)
        {
            ChangeColor(color);
            SelectSavedColor(color);
        }

        public void SetMaskColorFromSave(Color color)
        {
            ChangeColorMask(color);
            SelectSavedColor(color);
        }

        public Color GetColor()
        {
            var color = MaterialHelpers.GetBodyColor(_gameObject, _bodyMaterial);
            if (color == null) return _defaultColor;
            return (Color)color;
        }

        public Color GetMaskColor()
        {
            var color = MaterialHelpers.GetBodyMaskColor(_gameObject, _bodyMaterial);
            if (color == null) return _defaultColor;
            return (Color)color;
        }

        public void ChangeColor(Color color)
        {
            MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, color);
            ResetColorSelections(color);
            OnColorChanged?.Invoke(color);
        }

        public void ChangeColorMask(Color color)
        {
            MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, Color.white,color,Color.white);
            ResetColorSelections(color);
            OnColorChanged?.Invoke(color);
        }

        public void ChangeColorPageBy(int amount)
        {
            DrawColorPage(_currentColorPage + amount);
        }

        [Obsolete("This method will be removed in upcoming updates of the techfabricator")]
        public void SetupGrid(int colorsPerPage, GameObject colorItemPrefab, GameObject colorGrid, Text paginator, Action<string, object> onButtonClick)
        {
            _colorsPerPage = colorsPerPage;
            _colorItemPrefab = colorItemPrefab;
            _colorPageContainer = colorGrid;
            _colorPageNumber = paginator;
            _onButtonClick = onButtonClick;
            DrawColorPage(1);
        }

        public void SetupGrid(int colorsPerPage, GameObject colorItemPrefab, GameObject colorPage, Action<string, object> onButtonClick,Color startColor, Color hoverColor, int maxInteractionRange = 5, string prevBtnNAme = "PrevBTN", string nextBtnName = "NextBTN" , string gridName = "Grid", string paginatorName = "Paginator", string homeBtnName = "HomeBTN")
        {
            _colorsPerPage = colorsPerPage;
            _colorItemPrefab = colorItemPrefab;
            _colorPageContainer = InterfaceHelpers.FindGameObject(colorPage, gridName);
            _colorPageNumber = InterfaceHelpers.FindGameObject(colorPage, paginatorName)?.GetComponent<Text>();

            #region Prev Color Button
            var prevColorBtn = InterfaceHelpers.FindGameObject(colorPage, prevBtnNAme);

            InterfaceHelpers.CreatePaginator(prevColorBtn, -1, ChangeColorPageBy, startColor, hoverColor);
            #endregion

            #region Next Color Button
            var nextColorBtn = InterfaceHelpers.FindGameObject(colorPage, nextBtnName);

            InterfaceHelpers.CreatePaginator(nextColorBtn, 1, ChangeColorPageBy, startColor, hoverColor);
            #endregion

            #region HomeButton
            var homeBTN = InterfaceHelpers.FindGameObject(colorPage, homeBtnName);

            InterfaceHelpers.CreateButton(homeBTN, "HomeBTN", InterfaceButtonMode.Background,
                onButtonClick, startColor, hoverColor, maxInteractionRange, HomeButtonMessage);

            #endregion

            _onButtonClick = onButtonClick;
            DrawColorPage(1);
        }
    }
}
