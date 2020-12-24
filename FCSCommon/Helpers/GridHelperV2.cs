using System;
using UnityEngine;

namespace FCSCommon.Helpers
{
    internal class GridHelperV2 : MonoBehaviour
    {
        private int _itemsPerPage;
        private Action<string, object> _onButtonClick;
        private int _maxPage = 1;
        private int _currentPage;
        private GameObject _itemsGrid;

        internal Action<DisplayData> OnLoadDisplay { get; set; }
        internal int EndingPosition { get; private set; }
        internal int StartingPosition { get; private set; }

        internal void Setup(int itemsPerPage, GameObject gridPage, Color startColor, Color hoverColor, Action<string, object> onButtonClick, string gridName = "Grid")
        {
            //#region Prev Button

            //if (prevBtnNAme != string.Empty)
            //{
            //    var prevBtn = InterfaceHelpers.FindGameObject(gridPage, prevBtnNAme);
            //    InterfaceHelpers.CreatePaginator(prevBtn, -1, ChangePageBy, startColor, hoverColor);
            //}

            //#endregion

            //#region Next Button

            //if (nextBtnName != string.Empty)
            //{
            //    var nextBtn = InterfaceHelpers.FindGameObject(gridPage, nextBtnName);
            //    InterfaceHelpers.CreatePaginator(nextBtn, 1, ChangePageBy, startColor, hoverColor);
            //}
            //#endregion
            
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
                StartPosition = StartingPosition,
                EndPosition = EndingPosition,
                MaxPerPage = _itemsPerPage
            };

            OnLoadDisplay?.Invoke(data);
        }

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
}