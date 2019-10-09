using AE.MiniFountainFilter.Display;
using FCSCommon.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AE.MiniFountainFilter.Helpers
{
    internal class ColorPageHelper : MonoBehaviour
    {
        internal int ColorsPerPage;
        internal List<SerializableColor> SerializedColors;
        internal GameObject ColorPageContainer;
        internal GameObject ColorItemPrefab;
        internal Action<string, object> OnButtonClick;
        internal Text ColorPageNumber;

        private int _maxColorPage = 1;
        private int _currentColorPage;


        internal void Initialize()
        {
            DrawColorPage(1);
        }

        internal void ChangeColorPageBy(int amount)
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

            if (endingPosition > SerializedColors.Count)
            {
                endingPosition = SerializedColors.Count;
            }

            ClearColorPage();

            for (int i = startingPosition; i < endingPosition; i++)
            {
                var colorID = SerializedColors.ElementAt(i);
                LoadColorPicker(colorID);
            }

            UpdateColorPaginator();
        }

        private void LoadColorPicker(SerializableColor color)
        {
            GameObject itemDisplay = Instantiate(ColorItemPrefab);
            itemDisplay.transform.SetParent(ColorPageContainer.transform, false);
            itemDisplay.GetComponentInChildren<Image>().color = color.ToColor();

            var itemButton = itemDisplay.AddComponent<ColorItemButton>();
            itemButton.OnButtonClick = OnButtonClick;
            itemButton.BtnName = "ColorItem";
            itemButton.Color = color.ToColor();
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
