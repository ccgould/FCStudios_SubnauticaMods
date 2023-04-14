using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Extensions;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Data.Models;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

internal class EncyclopediaTabController : Page
{
    private GameObject _itemPrefab;
    private Text _title;
    private Text _message;
    private RawImage _image;
    private LayoutElement _imageLayout;
    private ScrollRect _listScrollRect;
    private RectTransform _listCanvas;
    private bool _isInitialize;
    internal static EncyclopediaTabController Instance;
    public override void Enter(object arg = null)
    {
        base.Enter(arg);               
        Initialize();
        RefreshList((EncyclopediaData)arg);
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

        _isInitialize = true;
    }

    private void RefreshList(EncyclopediaData data)
    {
        foreach (Transform item in _listCanvas.transform)
        {
            Destroy(item.gameObject);
        }

        foreach (var entry in data.Data)
        {
            var prefab = GameObject.Instantiate(_itemPrefab);
            var listItem = prefab.EnsureComponent<EncyclopediaListItem>();
            prefab.transform.SetParent(_listCanvas.transform, false);
            listItem.Initialize(entry);
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
        var texture = EncyclopediaService.GetEncyclopediaTexture2D(entryData.ImageName, ModRegistrationService.GetModPackData(EncyclopediaService.GetModPackID(entryData)).GetBundleName());
        _image.texture = texture;
        if (texture != null)
        {
            float num = (float)texture.height / (float)texture.width;
            float num2 = _image.rectTransform.rect.width * num;
            _imageLayout.minHeight = num2;
            _imageLayout.preferredHeight = num2;
            _image.gameObject.SetActive(true);
            return;
        }

        _image.gameObject.SetActive(false);
    }
}
