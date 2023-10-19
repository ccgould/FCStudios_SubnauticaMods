using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Core.Components;

public class PaginatorToggle : Toggle
{
    private int _pageIndex;
    private Text _textLbl;


    public void SetPageIndex(int index)
    {
        if(_textLbl == null)
        {
            _textLbl = GetComponentInChildren<Text>();
        }


        this._pageIndex = index;
        _textLbl.text = _pageIndex.ToString();
    }

    public int GetPageIndex()
    {
        return _pageIndex;
    }
}