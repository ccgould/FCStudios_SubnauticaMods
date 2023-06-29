using System;
using UnityEngine;

namespace FCS_AlterraHub.Core.Components;

public class GridHelper : MonoBehaviour
{
    [SerializeField] private int _itemsPerPage;
    private int _maxPage = 1;
    private int _currentPage;
    [SerializeField] private GameObject _itemsGrid;

    public Action<DisplayData> OnLoadDisplay { get; set; }
    public int EndingPosition { get; private set; }
    public int StartingPosition { get; private set; }

    public void  Awake()
    {
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
public struct DisplayData
{
    public GameObject ItemsPrefab { get; set; }
    public GameObject ItemsGrid { get; set; }
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public int MaxPerPage { get; set; }
}
