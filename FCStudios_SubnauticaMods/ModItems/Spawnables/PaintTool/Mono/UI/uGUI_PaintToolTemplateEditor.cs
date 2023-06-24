using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models;
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
    [SerializeField]private Button _doneBTN;
    private uGUI_ColorPickerTemplateItem _template;
    public static uGUI_PaintToolTemplateEditor Main;


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

        _doneBTN.onClick.AddListener((() =>
        {
            try
            {
                _template.SetColors(new ColorTemplate
                {
                    PrimaryColor = _primary.GetColor(),
                    SecondaryColor = _secondary.GetColor(),
                    EmissionColor = _emission.GetColor()
                });
                uGUI_PaintTool.NotifyItemChanged(_template.GetIndex(), _template.GetTemplate());
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
    }

    public void Close()
    {
        _primary?.SetColors(Color.white);
        _secondary?.SetColors(Color.white);
        _emission?.SetColors(Color.white);
    }
}