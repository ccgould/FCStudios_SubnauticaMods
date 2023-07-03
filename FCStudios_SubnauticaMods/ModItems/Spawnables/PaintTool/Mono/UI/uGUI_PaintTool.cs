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

namespace FCS_AlterraHub.ModItems.Spawnables.PaintTool.Mono.UI;

internal class uGUI_PaintTool : Page, IuGUIAdditionalPage
{
    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] private Transform _grid;
    [SerializeField] private Transform _colorItemTemplate;
    [SerializeField] private List<uGUI_ColorPickerTemplateItem> _colorItems = new();
    private PaintToolController _controller;
    public event Action<PDAPages> onBackClicked;
    public event Action<FCSDevice> onSettingsClicked;
    public static uGUI_PaintTool Main;
    private MenuController _menuController;

    public void UpdatePaintTool(bool value)
    {
        var selectedTemplate = GetSelectedTemplate();
        _controller.OnTemplateChanged(selectedTemplate.GetTemplate(), selectedTemplate.GetIndex());
    }

    private void Start()
    {
        _menuController = FCSPDAController.Main.GetGUI().GetMenuController();
    }

    public override void Enter(object arg = null)
    {
        base.Enter(arg);

        _controller = arg as PaintToolController;
        LoadTemplates(_controller.GetTemplates());
        SelectTemplate(_controller.GetCurrentSelectedTemplateIndex());
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

    private void LoadTemplates(List<ColorTemplate> colorTemplates)
    {
        
        if(colorTemplates.Count > _colorItems.Count())
        {
            var difference = colorTemplates.Count - _colorItems.Count();

            for (int i = 0; i < difference; i++)
            {
                AddNewTemplate();
            }
        }
        for (var index = 0; index < colorTemplates.Count; index++)
        {
            _colorItems.ElementAt(index).SetColors(colorTemplates[index]);
        }
    }

    private void AddNewTemplate()
    {
       var template = Instantiate(_colorItemTemplate, _grid);
        template.gameObject.SetActive(true);
        _colorItems.Add(template.gameObject.GetComponent<uGUI_ColorPickerTemplateItem>());
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
            UpdatePaintTool(false);
        }

        if(templateIndex.Equals(_colorItems.Last().GetIndex()))
        {
            //since this is the last Item in the list add a new item
            AddNewTemplate();
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
