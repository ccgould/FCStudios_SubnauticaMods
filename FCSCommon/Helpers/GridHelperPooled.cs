using System;
using FCS_AlterraHub.Mono.ObjectPooler;
using FCSCommon.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace FCSCommon.Helpers
{
    internal class GridHelperPooled : MonoBehaviour
    {
        private int _itemsPerPage;
        private Action<string, object> _onButtonClick;
        private Text _pageNumberText;
        private int _maxPage = 1;
        private int _currentPage;
        private ObjectPooler _pooler;
        private GameObject _itemsGrid;
        
        internal string HomeButtonMessage { get; set; }
        internal Action<DisplayDataPooled> OnLoadDisplay { get; set; }
        internal int EndingPosition { get; private set; }
        internal int StartingPosition { get; private set; }
        internal void Setup(int itemsPerPage, ObjectPooler pool, GameObject gridPage,Action<string,object> callBack,bool throwException = true)
        {
            _pooler = pool;

            #region Prev Button

            var prevBtn = InterfaceHelpers.FindGameObject(gridPage, "PrevBTN", throwException);
            if(prevBtn != null)
            {
                InterfaceHelpers.CreatePaginator(prevBtn, -1, ChangePageBy, Color.gray, Color.white);
            }

            #endregion

            #region Next Button
            var nextBtn = InterfaceHelpers.FindGameObject(gridPage, "NextBTN", throwException);
            if (nextBtn != null) 
            {
                InterfaceHelpers.CreatePaginator(nextBtn, 1, ChangePageBy, Color.gray, Color.white);
            }
            #endregion

            #region HomeButton
            var homeBTN = InterfaceHelpers.FindGameObject(gridPage, "HomeBTN", throwException);
            if(homeBTN != null)
            {
                InterfaceHelpers.CreateButton(homeBTN, "HomeBTN", InterfaceButtonMode.Background, callBack, Color.gray, Color.white, 5, HomeButtonMessage);
            }
            #endregion

            _pageNumberText = InterfaceHelpers.FindGameObject(gridPage, "Paginator", throwException)?.GetComponent<Text>();
            _itemsGrid = InterfaceHelpers.FindGameObject(gridPage, "Grid", throwException);
            _itemsPerPage = itemsPerPage;
            _onButtonClick = callBack;

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
            
            var data = new DisplayDataPooled
            {
                ItemsGrid = _itemsGrid,
                Pool = _pooler,
                StartPosition = StartingPosition,
                EndPosition = EndingPosition,
            };

            OnLoadDisplay?.Invoke(data);
        }
        
        internal void ClearPage()
        {
            if(_itemsGrid == null) return;

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

            if(count - 1 == 0 || _itemsPerPage + 1 == 0)  return;

            _maxPage = (count - 1) / _itemsPerPage + 1;

            if (_currentPage > _maxPage)
            {
                _currentPage = _maxPage;
            }
        }
    }

    internal struct DisplayDataPooled
    {
        public ObjectPooler Pool { get; set; }
        public GameObject ItemsGrid { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
    }
}
