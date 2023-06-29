using FCS_AlterraHub.Core.Extensions;
using System;
using UnityEngine;


namespace FCS_AlterraHub.Core.Components;
public class PaginatorController : MonoBehaviour
{
    public event EventHandler<OnPageChangedArgs> OnPageChanged;
    public class OnPageChangedArgs : EventArgs
    {
        public int PageIndex;
    }

    private GameObject[] _paginatorBTNs;
    private bool _isInitalized;

    private void Start()
    {
        if (_isInitalized) return;

        _paginatorBTNs = gameObject.GetChildren();

        for (var i = 0; i < _paginatorBTNs.Length; i++)
        {
            GameObject child = _paginatorBTNs[i];
            var paginator = child.GetComponent<PaginatorToggle>();
            paginator.SetPageIndex(i + 1);
            paginator.onValueChanged.AddListener((isChecked) =>
            {
                GotoPage(paginator.GetPageIndex());
            });           
        }
        _isInitalized = true;
    }

    public void ResetCount(int index)
    {
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

    private void GotoPage(int index)
    {
        OnPageChanged?.Invoke(this, new OnPageChangedArgs { PageIndex = index});
    }
}