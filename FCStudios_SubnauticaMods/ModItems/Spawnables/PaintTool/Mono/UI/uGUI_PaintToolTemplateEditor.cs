using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents;
using FCSCommon.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.Spawnables.PaintTool.Mono.UI;
internal class uGUI_PaintToolTemplateEditor : Page
{
    [SerializeField]private uGUI_PaintTool uGUI_PaintTool;
    [SerializeField]private uGUI_HSVControl _primary;
    [SerializeField]private uGUI_HSVControl _secondary;
    [SerializeField]private uGUI_HSVControl _emission;
    [SerializeField]private uGUI_InputField _templateNameInputField;
    [SerializeField]private Button _doneBTN;
    private uGUI_ColorPickerTemplateItem _template;
    public static uGUI_PaintToolTemplateEditor Main;
    private bool _nameIsDirty;

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

        _templateNameInputField.onEndEdit.AddListener((value) =>
        {
            _nameIsDirty = true;
        });

        _doneBTN.onClick.AddListener((() =>
        {
            try
            {
                if(_primary.IsDirty() || _secondary.IsDirty() || _emission.IsDirty() || _nameIsDirty)
                {
                    _template.SetColors(new ColorTemplate
                    {
                        PrimaryColor = _primary.GetColor(),
                        SecondaryColor = _secondary.GetColor(),
                        EmissionColor = _emission.GetColor(),
                        TemplateName = _templateNameInputField.text
                    });
                    uGUI_PaintTool.NotifyItemChanged(uGUI_PaintTool.GetIndex(_template), _template.GetTemplate());
                }

                Close();
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Error(e.Message);
                QuickLogger.Message("Attempting to fix in final block");
            }
            finally
            {
                Close();
            }
        }));
    }

    public void Open()
    {
        _template = uGUI_PaintTool.GetSelectedTemplate();
        var colors = _template.GetTemplate();
        _primary.SetColors(colors.PrimaryColor);
        _secondary.SetColors(colors.SecondaryColor);
        _emission.SetColors(colors.EmissionColor);
        _templateNameInputField.SetTextWithoutNotify(colors.TemplateName);
    }

    public void Close()
    {
        _primary?.SetColors(Color.white);
        _secondary?.SetColors(Color.white);
        _emission?.SetColors(Color.white);
        _nameIsDirty = false;
    }

    public void OnCopyBTNClicked()
    {
        uGUI_PaintTool.AddNewTemplate(new ColorTemplate(_template.GetTemplate()),true,true);
        Open();
    }

    public void OnDeleteBTNClicked()
    {
        uGUI_MessageBoxHandler.Instance.ShowMessage($"Are you sure you want to delete {_templateNameInputField.text}?", FCSMessageButton.YESNO,(result) =>
        {
            if(result == FCSMessageResult.OKYES)
            {
                ColorManager.colorTemplates.Remove(_template.GetTemplate());
                uGUI_PaintTool.RefreshuGUI();
                uGUI_PaintTool.PopPage();
                Close();
                
            }
        });
    }
}