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


        foreach (var item in EncyclopediaService.EncyclopediaEntries)
        {
            var prefab = GameObject.Instantiate(_itemPrefab);
            var listItem = prefab.EnsureComponent<EncyclopediaListItem>();
            prefab.transform.SetParent(_listCanvas.transform, false);
            listItem.Initialize(item.Value);
        }

        _isInitialize = true;
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

    internal void HoverTriggered(EncyclopediaData data)
    {
        if (data is null) return;
        _title.text = data.Title;
        SetImage(data);
        //FCSAssetBundlesService.PublicAPI.GetIconByName("HomeSolutionsIcon_W", PluginInfo.PLUGIN_NAME)
    }

    private void SetImage(EncyclopediaData entryData)
    {
        if (_image != null) return;

        var texture = EncyclopediaService.GetEncyclopediaTexture2D(entryData.ModPackID, ModRegistrationService.GetModPackData(entryData.ModPackID).GetBundleName());
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
