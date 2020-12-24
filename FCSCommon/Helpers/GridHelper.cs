using System;
using FCSCommon.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace FCSCommon.Helpers
{
    internal class GridHelper : MonoBehaviour
    {
        private int _itemsPerPage;
        private Action<string, object> _onButtonClick;
        private Text _pageNumberText;
        private int _maxPage = 1;
        private int _currentPage;
        private GameObject _itemPrefab;
        private GameObject _itemsGrid;

        internal string HomeButtonMessage { get; set; }
        internal Action<DisplayData> OnLoadDisplay { get; set; }
        internal int EndingPosition { get; private set; }
        internal int StartingPosition { get; private set; }

        internal void Setup(int itemsPerPage, GameObject itemPrefab, GameObject gridPage, Color startColor, Color hoverColor, Action<string, object> onButtonClick, int maxInteractionRange = 5, string prevBtnNAme = "PrevBTN", string nextBtnName = "NextBTN", string gridName = "Grid", string paginatorName = "Paginator", string homeBtnName = "HomeBTN", string customHomeBTN = "")
        {
            _itemPrefab = itemPrefab;

            #region Prev Button

            if (prevBtnNAme != string.Empty)
            {
                var prevBtn = InterfaceHelpers.FindGameObject(gridPage, prevBtnNAme);
                InterfaceHelpers.CreatePaginator(prevBtn, -1, ChangePageBy, startColor, hoverColor);
            }

            #endregion

            #region Next Button

            if (nextBtnName != string.Empty)
            {
                var nextBtn = InterfaceHelpers.FindGameObject(gridPage, nextBtnName);
                InterfaceHelpers.CreatePaginator(nextBtn, 1, ChangePageBy, startColor, hoverColor);
            }
            #endregion

            #region HomeButton
            if (homeBtnName != string.Empty)
            {
                var homeBTN = InterfaceHelpers.FindGameObject(gridPage, homeBtnName);
                InterfaceHelpers.CreateButton(homeBTN, string.IsNullOrEmpty(customHomeBTN) ? homeBtnName : customHomeBTN, InterfaceButtonMode.Background,
                    onButtonClick, startColor, hoverColor, maxInteractionRange, HomeButtonMessage);
            }
            #endregion

            if (paginatorName != string.Empty)
            {
                _pageNumberText = InterfaceHelpers.FindGameObject(gridPage, paginatorName)?.GetComponent<Text>();
            }

            _itemsGrid = InterfaceHelpers.FindGameObject(gridPage, gridName); ;
            _itemsPerPage = itemsPerPage;
            _onButtonClick = onButtonClick;

            DrawPage(1);
        }

        internal int GetCurrentPage()
        {
            return _currentPage;
        }

        internal void ChangePageBy(int amount)
        {
            DrawPage(_currentPage + amount);
        }

        internal void DrawPage()
        {
            DrawPage(_currentPage);
        }

        internal void DrawPage(int page)
        {
            _currentPage = page;

            if (_currentPage <= 0)
            {
                _currentPage = 1;
            }
            else if (_currentPage > _maxPage)
            {
                _currentPage = _maxPage;
            }

            StartingPosition = (_currentPage - 1) * _itemsPerPage;

            EndingPosition = StartingPosition + _itemsPerPage;

            var data = new DisplayData
            {
                ItemsGrid = _itemsGrid,
                ItemsPrefab = _itemPrefab,
                StartPosition = StartingPosition,
                EndPosition = EndingPosition,
            };

            OnLoadDisplay?.Invoke(data);
        }

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

        internal void ClearPage()
        {
            if (_itemsGrid == null) return;

            for (int i = 0; i < _itemsGrid?.transform?.childCount; i++)
            {
                var item = _itemsGrid?.transform?.GetChild(i)?.gameObject;

                if (item != null)
                {
                    Destroy(item);
                }
            }
        }

        internal void UpdaterPaginator(int count)
        {
            CalculateNewMaxPages(count);
            if (_pageNumberText == null) return;
            _pageNumberText.text = $"{_currentPage.ToString()} | {_maxPage}";
        }

        private void CalculateNewMaxPages(int count)
        {
            _maxPage = (count - 1) / _itemsPerPage + 1;

            if (_currentPage > _maxPage)
            {
                _currentPage = _maxPage;
            }
        }
    }
    internal struct DisplayData
    {
        public GameObject ItemsPrefab { get; set; }
        public GameObject ItemsGrid { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public int MaxPerPage { get; set; }
    }
}