using System;
using System.Collections.Generic;
using System.Linq;
using FCSCommon.Components;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using FCSTechFabricator.Objects;
using UnityEngine;
using UnityEngine.UI;

namespace FCSCommon.Controllers
{
    internal class ColorManager : MonoBehaviour
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
        private readonly Color _defaultColor = new Color(0.7529412f, 0.7529412f, 0.7529412f, 1f);

        internal Action<Color> OnColorChanged;
        
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

        internal void Initialize(GameObject gameObject, string bodyMaterial)
        {
            _gameObject = gameObject;
            _bodyMaterial = bodyMaterial;
            ColorList.AddColor(GetColor());
        }

        internal void SetColorFromSave(Color color)
        {
            ChangeColor(color);
            SelectSavedColor(color);
        }

        internal Color GetColor()
        {
            var color = MaterialHelpers.GetBodyColor(_gameObject, _bodyMaterial);
            if (color == null) return _defaultColor;
            return (Color)color;
        }

        internal void ChangeColor(Color color)
        {
            MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, color);
            ResetColorSelections(color);
            OnColorChanged?.Invoke(color);
        }

        internal void ChangeColorPageBy(int amount)
        {
            DrawColorPage(_currentColorPage + amount);
        }
        
        internal void SetupGrid(int colorsPerPage, GameObject colorItemPrefab, GameObject colorGrid, Text paginator, Action<string, object> onButtonClick)
        {
            _colorsPerPage = colorsPerPage;
            _colorItemPrefab = colorItemPrefab;
            _colorPageContainer = colorGrid;
            _colorPageNumber = paginator;
            _onButtonClick = onButtonClick;
            DrawColorPage(1);
        }
    }
}