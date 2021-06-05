using System.Collections.Generic;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Interfaces;
using UnityEngine;

namespace FCS_AlterraHub.Mono.Controllers
{
    public class PaginatorController : MonoBehaviour
    {
        private IFCSDisplay _display;
        private FCSToggleButton _currentSelected;
        private List<FCSToggleButton> _toggleButtons = new List<FCSToggleButton>();
        private GameObject[] _paginatorBTNs;
        private bool _isInitalized;

        private void Setup()
        {
            if (_isInitalized) return;
            _paginatorBTNs = gameObject.GetChildren();
            for (var i = 0; i < _paginatorBTNs.Length; i++)
            {
                GameObject child = _paginatorBTNs[i];
                var paginator = child.AddComponent<FCSToggleButton>();
                paginator.Tag = i + 1;
                paginator.STARTING_COLOR = Color.white;
                paginator.HOVER_COLOR = new Color(0, 1, 1);
                paginator.OnButtonClick += (s, o) =>
                {
                    _currentSelected = paginator;
                    GotoPage((int)o);
                    UpdateButtons();
                };
                _toggleButtons.Add(paginator);
            }
            _isInitalized = true;
        }

        public void ResetCount(int index)
        {
            Setup();

            if (_paginatorBTNs == null) return;

            for (int i = 0; i < _paginatorBTNs.Length - 1; i++)
            {
                _paginatorBTNs[i]?.SetActive(false);
            }

            for (int i = 0; i < index; i++)
            {
                _paginatorBTNs[i]?.SetActive(true);
            }
        }

        private void UpdateButtons()
        {
            Setup();

            foreach (var paginatorBtN in _toggleButtons)
            {
                if (paginatorBtN != _currentSelected)
                {
                    paginatorBtN.DeSelect();
                }
            }
        }

        public void Initialize(IFCSDisplay display)
        {
            _display = display;
            Setup();
        }

        private void GotoPage(int index)
        {
            Setup();
            _display.GoToPage(index);
            _display.GoToPage(index,this);
        }
    }
}
