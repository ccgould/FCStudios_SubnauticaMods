using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Data.Models;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;
using static ErrorMessage;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

internal class EncyclopediaMainTabController : Page
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
    internal static EncyclopediaMainTabController Instance;

    public override PDAPages PageType => PDAPages.EncyclopediaMain;

    private void Awake()
    {
        QuickLogger.Debug("Set Encyclopedia Instance");
        Instance = this;
    }

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
       
        if (_itemPrefab is null)
        {
            QuickLogger.Error("Encyuclopedia List Item Prefab Returned Null");
            return;
        }


        foreach (var item in EncyclopediaService.GetEntries())
        {
            var prefab = GameObject.Instantiate(_itemPrefab);
            var listItem = prefab.GetComponent<EncyclopediaListItem>();
            prefab.transform.SetParent(_listCanvas.transform, false);
            listItem.Initialize(this,item.Value);
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
        _message.text = string.Empty;
        _image.gameObject.SetActive(false);
    }

    internal void HoverTriggered(EncyclopediaData data)
    {
        if (data is null) return;
        _title.text = data.Title;
        _message.text = data.Description;
        SetImage(data);
    }

    private void SetImage(EncyclopediaData entryData)
    {
        if (_image is null) return;

        var texture = EncyclopediaService.GetEncyclopediaTexture2D(entryData.Icon, ModRegistrationService.GetModPackData(entryData.ModPackID).GetBundleName());
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

    public override void OnBackButtonClicked()
    {
        FCSPDAController.Main.GetGUI().GoBackAPage();
    }
}
