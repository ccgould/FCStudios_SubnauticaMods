using System;
using FCS_AlterraHub.Mono;
using UnityEngine;

namespace FCSCommon.Helpers
{
    public class GridHelperV2 : MonoBehaviour
    {
        private int _itemsPerPage;
        private int _maxPage = 1;
        private int _currentPage;
        private GameObject _itemsGrid;

        public Action<DisplayData> OnLoadDisplay { get; set; }
        public int EndingPosition { get; private set; }
        public int StartingPosition { get; private set; }

        public void Setup(int itemsPerPage, GameObject gridPage, Color startColor, Color hoverColor, Action<string, object> onButtonClick, string gridName = "Grid")
        {
            _itemsGrid = InterfaceHelpers.FindGameObject(gridPage, gridName); 
            _itemsPerPage = itemsPerPage;
            DrawPage(1);
        }

        public int GetCurrentPage()
        {
            return _currentPage;
        }

        public void DrawPage()
        {
            DrawPage(_currentPage);
        }

        public void DrawPage(int page)
        {
            _currentPage = page;
            FixPageLimits();
            SetStartingAndEndingPosition();
            OnLoadDisplay?.Invoke(CreateDisplayData());
        }

        private DisplayData CreateDisplayData()
        {
            var data = new DisplayData
            {
                ItemsGrid = _itemsGrid,
                StartPosition = StartingPosition,
                EndPosition = EndingPosition,
                MaxPerPage = _itemsPerPage
            };
            return data;
        }

        private void SetStartingAndEndingPosition()
        {
            StartingPosition = (_currentPage - 1) * _itemsPerPage;

            EndingPosition = StartingPosition + _itemsPerPage;
        }
        
        private void FixPageLimits()
        {
            if (_currentPage <= 0)
            {
                _currentPage = 1;
            }
            else if (_currentPage > _maxPage)
            {
                _currentPage = _maxPage;
            }
        }
        
        public void UpdaterPaginator(int count)
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

        public int GetMaxPages()
        {
            return _maxPage;
        }
    }
}