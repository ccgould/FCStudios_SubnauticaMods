using System;
using FCS_AlterraHub.Enumerators;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono
{
    public class GridHelper : MonoBehaviour
    {
        private int _itemsPerPage;
        private Action<string, object> _onButtonClick;
        private Text _pageNumberText;
        private int _maxPage = 1;
        private int _currentPage;
        private GameObject _itemPrefab;
        private GameObject _itemsGrid;

        public string HomeButtonMessage { get; set; }
        public Action<DisplayData> OnLoadDisplay { get; set; }
        public int EndingPosition { get; private set; }
        public int StartingPosition { get; private set; }

        public void Setup(int itemsPerPage, GameObject itemPrefab, GameObject gridPage, Color startColor, Color hoverColor, Action<string, object> onButtonClick, int maxInteractionRange = 5, string prevBtnNAme = "PrevBTN", string nextBtnName = "NextBTN", string gridName = "Grid", string paginatorName = "Paginator", string homeBtnName = "HomeBTN", string customHomeBTN = "")
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

        public int GetCurrentPage()
        {
            return _currentPage;
        }

        public void ChangePageBy(int amount)
        {
            DrawPage(_currentPage + amount);
        }

        public void DrawPage()
        {
            DrawPage(_currentPage);
        }

        public void DrawPage(int page)
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

        public void ClearPage()
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

        public void UpdaterPaginator(int count)
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
    public struct DisplayData
    {
        public GameObject ItemsPrefab { get; set; }
        public GameObject ItemsGrid { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public int MaxPerPage { get; set; }
    }
}