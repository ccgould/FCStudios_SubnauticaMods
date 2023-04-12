using System;
using System.Collections.Generic;
using System.Net;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Data.Models;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

internal class EncyclopediaMainTabController : Page
{
    private GameObject _itemPrefab;
    private Text _title;
    private Text _message;
    private RawImage _image;
    private LayoutElement _imageLayout;
    private ScrollRect _listScrollRect;
    private RectTransform _listCanvas;
    private bool _isInitialize;
    internal static EncyclopediaMainTabController Instance;
    public override void Enter(object arg = null)
    {
        base.Enter(arg);
        Initialize();
        _title?.gameObject?.SetActive(true);
    }

    internal bool HasEntry(TechType techType)
    {
        return true;
    }

    internal bool HasEntry(string techType)
    {
        return true;
    }

    internal void Initialize()
    {

        if (_isInitialize) return;

        Instance = this;
        _itemPrefab = FCSAssetBundlesService.PublicAPI.GetLocalPrefab("encyclopediaListItem");
        _title = GameObjectHelpers.FindGameObject(gameObject, "Title").GetComponent<Text>();
        _message = GameObjectHelpers.FindGameObject(gameObject, "Description").GetComponent<Text>();
        var banner = GameObjectHelpers.FindGameObject(gameObject, "Banner");
        _image = banner.GetComponent<RawImage>();
        _imageLayout = banner.GetComponent<LayoutElement>();
        var encycList = GameObjectHelpers.FindGameObject(gameObject, "EncyclopediaList");
        _listScrollRect = encycList.GetComponent<ScrollRect>();
        _listCanvas = encycList.FindChild("Viewport").FindChild("Content").GetComponent<RectTransform>();


        if (_itemPrefab is null)
        {
            QuickLogger.Error("Encyuclopedia List Item Prefab Returned Null");
            return;
        }


        foreach (string item in GetCategories())
        {
            var prefab = GameObject.Instantiate(_itemPrefab);
            var listItem = prefab.EnsureComponent<EncyclopediaListItem>();
            prefab.transform.SetParent(_listCanvas.transform, false);
            listItem.Initialize(item);
        }

        //foreach (Dictionary<string, List<EncyclopediaEntryData>> item in EncyclopediaService.EncyclopediaEntries)
        //{
        //    foreach (var entries in item.Values)
        //    {
        //        foreach (var entry in entries)
        //        {
        //            var prefab = GameObject.Instantiate(_itemPrefab);
        //            var listItem = prefab.EnsureComponent<EncyclopediaListItem>();
        //            prefab.transform.SetParent(_listCanvas.transform, false);
        //            listItem.Instantiate(entry.TabTitle);
        //        }
        //    }
        //}



        _isInitialize = true;
    }

    private HashSet<string> GetCategories()
    {
        var result = new HashSet<string>();
        foreach (Dictionary<string, List<EncyclopediaEntryData>> item in EncyclopediaService.EncyclopediaEntries)
        {
            foreach (var entries in item.Values)
            {
                foreach (var entry in entries)
                {
                    result.Add(entry.GetCategory());
                }
            }
        }

        return result;
    }

    internal void OpenEntry(TechType techType)
    {

    }

    internal void OpenEntry(string techType)
    {

    }

    internal void Clear()
    {
        _title.text = string.Empty;
    }

    internal void HoverTriggered(string text)
    {
        _title.text = text;
    }
}
