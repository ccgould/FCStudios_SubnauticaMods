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
        public string HomeButtonMessage { get; set; }

        [Obsolete("This method will be removed in upcoming updates please use setup instead")]
        internal void Initialize(GameObject itemPrefab, GameObject itemsGrid,GameObject pageNumberText, int itemsPerPage, Action<string, object> onButtonClick)
        {
            _itemPrefab = itemPrefab;
            _itemsGrid = itemsGrid;
            _itemsPerPage = itemsPerPage;
            _onButtonClick = onButtonClick;
            _pageNumberText = pageNumberText.GetComponent<Text>();
            DrawPage(1);
        }

        internal void Setup(int itemsPerPage, GameObject itemPrefab, GameObject gridPage, Color startColor, Color hoverColor, Action<string, object> onButtonClick, int maxInteractionRange = 5, string prevBtnNAme = "PrevBTN", string nextBtnName = "NextBTN", string gridName = "Grid", string paginatorName = "Paginator", string homeBtnName = "HomeBTN")
        {
            _itemPrefab = itemPrefab;

            #region Prev Color Button
            var prevColorBtn = InterfaceHelpers.FindGameObject(gridPage, prevBtnNAme);

            InterfaceHelpers.CreatePaginator(prevColorBtn, -1, ChangePageBy, startColor, hoverColor);
            #endregion

            #region Next Color Button
            var nextColorBtn = InterfaceHelpers.FindGameObject(gridPage, nextBtnName);

            InterfaceHelpers.CreatePaginator(nextColorBtn, 1, ChangePageBy, startColor, hoverColor);
            #endregion

            #region HomeButton
            var homeBTN = InterfaceHelpers.FindGameObject(gridPage, homeBtnName);

            InterfaceHelpers.CreateButton(homeBTN, "HomeBTN", InterfaceButtonMode.Background,
                onButtonClick, startColor, hoverColor, maxInteractionRange, HomeButtonMessage);

            #endregion
            
            _itemsGrid = InterfaceHelpers.FindGameObject(gridPage, gridName); ;
            _itemsPerPage = itemsPerPage;
            _onButtonClick = onButtonClick;
            _pageNumberText = InterfaceHelpers.FindGameObject(gridPage, paginatorName)?.GetComponent<Text>();
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
            
            OnLoadDisplay?.Invoke(_itemPrefab,_itemsGrid,StartingPosition,EndingPosition);
        }

        internal Action<GameObject, GameObject,int,int> OnLoadDisplay { get; set; }

        internal int EndingPosition { get; private set; }

        internal int StartingPosition { get; private set; }

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
            _maxPage = (count - 1) / _itemsPerPage + 1;

            if (_currentPage > _maxPage)
            {
                _currentPage = _maxPage;
            }
        }
    }
}
