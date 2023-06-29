using FCS_AlterraHub.Core.Extensions;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Data.Models;
using FCSCommon.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

internal class EncyclopediaTabController : Page
{
    [SerializeField]
    private GameObject _itemPrefab;
    [SerializeField]
    private Text _title;
    [SerializeField]
    private Text _message;
    [SerializeField]
    private RawImage _image;
    [SerializeField]
    private LayoutElement _imageLayout;
    [SerializeField]
    private ScrollRect _listScrollRect;
    [SerializeField]
    private RectTransform _listCanvas;
    private bool _isInitialize;
    private readonly List<EncyclopediaListItem> _trackedEntries = new();

    public override void Enter(object arg = null)
    {
        base.Enter(arg);               
        Initialize();
        RefreshList((EncyclopediaData)arg);
        SelectActiveEntry();
    }

    private void SelectActiveEntry()
    {
        foreach (var item in _trackedEntries)
        {
            if(item.GetData().IsSame(EncyclopediaService.GetSelectedEntry()))
            {
                item.onClick();
                break;
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        _image.gameObject.SetActive(false);
        _title.text = string.Empty;
        _message.text = string.Empty;
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

        if (_itemPrefab is null)
        {
            QuickLogger.Error("Encyclopedia List Item Prefab Returned Null");
            return;
        }
        _isInitialize = true;
    }

    private void RefreshList(EncyclopediaData data)
    {
        _trackedEntries?.Clear();

        foreach (Transform item in _listCanvas.transform)
        {
            Destroy(item.gameObject);
        }

        if (data?.Data is null)
        {
            QuickLogger.Debug("Receieved Data was null");
            return;
        }

        foreach (var entry in data?.Data)
        {
            var prefab = Instantiate(_itemPrefab);
            var listItem = prefab.GetComponent<EncyclopediaListItem>();
            prefab.transform.SetParent(_listCanvas.transform, false);
            listItem.Initialize(entry);
            _trackedEntries?.Add(listItem);
        }
    }

    internal void OpenEntry(TechType techType)
    {
        Enter(EncyclopediaService.GetEntryByTechType(techType));
    }

    internal void OpenEntry(string techType)
    {
        Enter(EncyclopediaService.GetEntryByTechType(techType.ToTechType()));
    }

    internal void SetData(EncyclopediaEntryData entryData)
    {
        if (entryData != null)
        {
            _title.text = entryData.Title;
            _message.text = entryData.Body;
            SetImage(entryData);
            return;
        }
    }

    private void SetImage(EncyclopediaEntryData entryData)
    {
        if(entryData is not null)
        {
            QuickLogger.Debug("Entry Data is not null");
            var texture = EncyclopediaService.GetEncyclopediaTexture2D(entryData.ImageName, ModRegistrationService.GetModPackData(EncyclopediaService.GetModPackID(entryData))?.GetBundleName());
            QuickLogger.Debug($"Texture Returned Texture: {texture}");
            _image.texture = texture;
            QuickLogger.Debug($"Setting Texture");
            if (texture != null)
            {
                QuickLogger.Debug("Has Image");
                float num = (float)texture.height / (float)texture.width;
                float num2 = _image.rectTransform.rect.width * num;
                _imageLayout.minHeight = num2;
                _imageLayout.preferredHeight = num2;
                _image.gameObject.SetActive(true);
                return;
            }
        }

        QuickLogger.Debug("Doesnt Have Image");
        _image.gameObject.SetActive(false);
    }
}