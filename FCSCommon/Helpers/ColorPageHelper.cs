using System;
using System.Collections.Generic;
using System.Linq;
using FCSCommon.Components;
using FCSCommon.Extensions;
using FCSCommon.Objects;
using UnityEngine;
using UnityEngine.UI;

namespace FCSCommon.Helpers
{
    public class ColorPageHelper : MonoBehaviour
    {
        public int ColorsPerPage;
        public List<ColorVec4> SerializedColors;
        public GameObject ColorPageContainer;
        public GameObject ColorItemPrefab;
        public Action<string, object> OnButtonClick;
        public Text ColorPageNumber;

        private int _maxColorPage = 1;
        private int _currentColorPage;

        public void Initialize()
        {
            DrawColorPage(1);
        }

        public void ChangeColorPageBy(int amount)
        {
            DrawColorPage(_currentColorPage + amount);
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

            int startingPosition = (_currentColorPage - 1) * ColorsPerPage;
            int endingPosition = startingPosition + ColorsPerPage;

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
        }

        private void LoadColorPicker(ColorVec4 color)
        {
            GameObject itemDisplay = Instantiate(ColorItemPrefab);
            itemDisplay.transform.SetParent(ColorPageContainer.transform, false);
            itemDisplay.GetComponentInChildren<Image>().color = color.Vector4ToColor();

            var itemButton = itemDisplay.AddComponent<ColorItemButton>();
            itemButton.OnButtonClick = OnButtonClick;
            itemButton.BtnName = "ColorItem";
            itemButton.Color = color.Vector4ToColor();
        }

        private void ClearColorPage()
        {
            for (int i = 0; i < ColorPageContainer.transform.childCount; i++)
            {
                Destroy(ColorPageContainer.transform.GetChild(i).gameObject);
            }
        }

        private void UpdateColorPaginator()
        {
            CalculateNewMaxColorPages();
            if (ColorPageNumber == null) return;
            ColorPageNumber.text = $"{_currentColorPage.ToString()} | {_maxColorPage}";
        }

        private void CalculateNewMaxColorPages()
        {
            _maxColorPage = Mathf.CeilToInt((SerializedColors.Count - 1) / ColorsPerPage) + 1;
            if (_currentColorPage > _maxColorPage)
            {
                _currentColorPage = _maxColorPage;
            }
        }
    }
}

