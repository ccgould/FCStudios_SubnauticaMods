using System;
using System.Collections.Generic;
using System.Text;
using FCSCommon.Abstract;
using FCSCommon.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace FCSCommon.Helpers
{
    public class GridHelper : MonoBehaviour
    {
        private int _itemsPerPage;
        private Action<string, object> _onButtonClick;
        private Text _pageNumberText;
        

        private int _maxColorPage = 1;
        private int _currentPage;
        private GameObject _itemPrefab;
        private GameObject _itemsGrid;
        private int _count;

        public void Initialize(GameObject itemPrefab, GameObject itemsGrid,GameObject pageNumberText, int itemsPerPage, Action<string, object> onButtonClick)
        {
            _itemPrefab = itemPrefab;
            _itemsGrid = itemsGrid;
            _itemsPerPage = itemsPerPage;
            _onButtonClick = onButtonClick;
            _pageNumberText = pageNumberText.GetComponent<Text>();
            DrawPage(1);
        }

        public int GetCurrentPage()
        {
            return _currentPage;
        }

        public void ChangePageBy(int amount)
        {
            DrawPage(_currentPage + amount);
        }

        public void DrawPage(int page)
        {
            _currentPage = page;

            if (_currentPage <= 0)
            {
                _currentPage = 1;
            }
            else if (_currentPage > _maxColorPage)
            {
                _currentPage = _maxColorPage;
            }

            StartingPosition = (_currentPage - 1) * _itemsPerPage;
            EndingPosition = StartingPosition + _itemsPerPage;

            if (EndingPosition > _count)
            {
                EndingPosition = _count;
            }

            ClearPage();

            OnLoadDisplay?.Invoke(_itemPrefab,_itemsGrid);

            UpdaterPaginator();
        }

        public Action<GameObject, GameObject> OnLoadDisplay { get; set; }

        public int EndingPosition { get; private set; }

        public int StartingPosition { get; private set; }

        //private void LoadDisplay<T>(T item)
        //{
            //for (int i = StartingPosition; i < EndingPosition; i++)
            //{
            //    var colorID = SerializedColors.ElementAt(i);
            //    LoadDisplay(colorID);
            //}

            //GameObject itemDisplay = Instantiate(_itemPrefab);

            //itemDisplay.transform.SetParent(_itemsGrid.transform, false);
            //var text = itemDisplay.transform.Find("Location_LBL").GetComponent<Text>();
            //text.text = storageItemName.Name;

            //var itemButton = itemDisplay.AddComponent<InterfaceButton>();
            //itemButton.ButtonMode = InterfaceButtonMode.TextColor;
            //itemButton.Tag = storageItemName;
            //itemButton.TextComponent = text;
            //itemButton.OnButtonClick += _onButtonClick;
            //itemButton.BtnName = "ShippingContainer";
        //}

        private void ClearPage()
        {
            for (int i = 0; i < _itemsGrid.transform.childCount; i++)
            {
                Destroy(_itemsGrid.transform.GetChild(i).gameObject);
            }
        }

        private void UpdaterPaginator()
        {
            CalculateNewMaxPages();
            if (_pageNumberText == null) return;
            _pageNumberText.text = $"{_currentPage.ToString()} | {_maxColorPage}";
        }

        private void CalculateNewMaxPages()
        {
            _maxColorPage = Mathf.CeilToInt((_count - 1) / _itemsPerPage) + 1;
            if (_currentPage > _maxColorPage)
            {
                _currentPage = _maxColorPage;
            }
        }

        public void UpdateCount(int value)
        {
            _count = value;
        }
    }
}
