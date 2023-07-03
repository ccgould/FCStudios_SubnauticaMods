using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using System.Collections;

namespace FCS_AlterraHub.ModItems.Spawnables.PaintTool.Mono.UI;

internal class uGUI_PaintTool : Page, IuGUIAdditionalPage
{
    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] private Transform _grid;
    [SerializeField] private Transform _colorItemTemplate;
    private List<uGUI_ColorPickerTemplateItem> _colorItems = new();
    private PaintToolController _controller;
    public event Action<PDAPages> onBackClicked;
    public event Action<FCSDevice> onSettingsClicked;
    public static uGUI_PaintTool Main;
    private MenuController _menuController;

    public void UpdatePaintTool()
    {
        var selectedTemplate = GetSelectedTemplate();
        _controller.OnTemplateChanged(selectedTemplate.GetTemplate(), GetIndex(selectedTemplate));
    }

    public int GetIndex(uGUI_ColorPickerTemplateItem item)
    {
        return _colorItems.IndexOf(item);
    }

    private void Start()
    {
        _menuController = FCSPDAController.Main.GetGUI().GetMenuController();
    }

    public override void Enter(object arg = null)
    {
        base.Enter(arg);

        _controller = arg as PaintToolController;
        LoadTemplates();
        SelectTemplate(_controller.GetCurrentSelectedTemplateIndex());
    }

    public override void Exit()
    {
        base.Exit();

        ClearuGUI();
    }

    public void RefreshuGUI()
    {
        ClearuGUI();
        LoadTemplates();
    }

    private void ClearuGUI()
    {
        foreach (Transform child in _grid.transform)
        {
            if (child == _colorItemTemplate) continue;
            Destroy(child.gameObject);
        }

        _colorItems.Clear();
    }

    public void Initialize(object obj)
    {
    }

    public override void Awake()
    {
        base.Awake();

        if (Main == null)
        {
            Main = this;
            DontDestroyOnLoad(this);
        }

        else if (Main != null)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void SelectTemplate(int index)
    {
        QuickLogger.Debug($"Color Picker selecting: {index}", true);
        _colorItems.ElementAt(index).Select();
    }

    private void LoadTemplates()
    {
        var colorTemplates = ColorManager.colorTemplates;

        for (var index = 0; index < colorTemplates.Count; index++)
        {
            AddNewTemplate(colorTemplates[index]);
        }
    }

    public void AddNewTemplate(ColorTemplate colorTemplate = null,bool addToColorList = false,bool isCopy = false)
    {
        if (isCopy)
        {
            int index = ColorManager.colorTemplates.Count - 1;

            if (index == -1)
            {
                index = 0;
            }

            ColorManager.colorTemplates.Insert(index, colorTemplate);
            RefreshuGUI();
            SelectTemplate(index);
        }
        else
        {
            var template = Instantiate(_colorItemTemplate, _grid);
            template.gameObject.SetActive(true);
            var uGUI_ColorPickerTemplateItem = template.gameObject.GetComponent<uGUI_ColorPickerTemplateItem>();

            if (colorTemplate is not null)
            {
                uGUI_ColorPickerTemplateItem.SetColors(colorTemplate);
            }

            _colorItems.Add(uGUI_ColorPickerTemplateItem);

            if (addToColorList)
            {
                ColorManager.colorTemplates.Add(colorTemplate);
            }
        }        
    }

    internal uGUI_ColorPickerTemplateItem GetSelectedTemplate()
    {
        return _toggleGroup.ActiveToggles().FirstOrDefault()?.gameObject.GetComponent<uGUI_ColorPickerTemplateItem>();
    }
       
    public void NotifyItemChanged(int templateIndex, ColorTemplate colorTemplate)
    {
        _controller.UpdateTemplates(templateIndex, colorTemplate);
        if(_controller.GetCurrentSelectedTemplateIndex() == templateIndex)
        {
            UpdatePaintTool();
        }

        if(templateIndex.Equals(_colorItems.Count - 1))
        {
            //since this is the last Item in the list add a new item
            AddNewTemplate(null,true);
        }
    }

    public IFCSObject GetController()
    {
        return _controller;
    }

    public void PushPage(Page page)
    {
        _menuController.PushPage(page);
    }

    public void PopPage()
    {
        _menuController.PopAndPeek();
    }
}
