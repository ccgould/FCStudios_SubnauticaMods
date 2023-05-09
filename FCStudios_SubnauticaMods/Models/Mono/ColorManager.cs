using FCS_AlterraHub.Core.Extensions;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Enumerators;
using FCSCommon.Utilities;
using System;
using UnityEngine;

namespace FCS_AlterraHub.Models.Mono;

public class ColorManager : MonoBehaviour
{
    private string _bodyMaterial;
    private GameObject _gameObject;
    private string _bodySecondary;
    private Color _currentColor = Color.white;
    private Color _currentSecondaryColor = Color.white;
    private Color _currentLumColor = Color.white;
    private string _lumMaterial;
    private ColorTemplate _currentTemplate = new ColorTemplate
    {
        PrimaryColor = Color.white,
        SecondaryColor = Color.white,
        EmissionColor = Color.cyan
    };


    public void Initialize(GameObject gameObject)
    {
        _gameObject = gameObject;
        _bodyMaterial = ModPrefabService.BasePrimaryCol;
        _bodySecondary = ModPrefabService.BaseSecondaryCol;
        _lumMaterial = ModPrefabService.TBaseEmission;
    }

    public Color GetColor(ColorTargetMode mode)
    {
        switch (mode)
        {
            case ColorTargetMode.Primary:
            case ColorTargetMode.Both:
                return _currentTemplate?.PrimaryColor ?? Color.white;
            case ColorTargetMode.Secondary:
                return _currentTemplate?.SecondaryColor ?? Color.white;
            case ColorTargetMode.Emission:
                return _currentTemplate?.EmissionColor ?? Color.white;
        }
        return _currentColor;
    }

    public bool ChangeColor(ColorTemplate template)
    {

        bool result;

        if (template == null) return false;

        try
        {
            _currentTemplate = template;
            MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, template.PrimaryColor);
            MaterialHelpers.ChangeMaterialColor(_bodySecondary, _gameObject, template.SecondaryColor);
            MaterialHelpers.ChangeEmissionColor(_lumMaterial, _gameObject, template.EmissionColor);
            result = true;
        }
        catch (Exception e)
        {
            QuickLogger.Error(e.StackTrace);
            QuickLogger.Error(e.Message);
            result = false;
        }
        return result;
    }


    public ColorTemplate GetTemplate()
    {
        return _currentTemplate;
    }

    public void LoadTemplate(ColorTemplateSave colorTemplate)
    {
        ChangeColor(colorTemplate.ToColorTemplate());
    }

    public ColorTemplateSave SaveTemplate()
    {
        return _currentTemplate.ToColorTemplate();
    }
}
