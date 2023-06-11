using FCS_AlterraHub.Core.Extensions;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Models.Mono;

public class ColorManager : MonoBehaviour
{
    [SerializeField] private FCSDevice controller;

    [SerializeField] private Color statusLightOff = Color.red;
    [SerializeField] private Color statusLightOn = Color.cyan;
    [SerializeField] private Color statusLightDisconnected = Color.yellow;

    [SerializeField][Range(0f, 1f)] private float lerpTime = .5f;
    [SerializeField] private Color[] lerpColors = new Color[] 
    {
        Color.cyan,
        Color.white
    };

    [SerializeField] private List<GameObject> _interiorMeshes;
    [SerializeField][Range(-10, 10)] private float _interiorLumIntensity;
    [SerializeField] private List<GameObject> _primaryMeshes;
    [SerializeField] private List<GameObject> _secondaryMeshes;

    private string _bodyMaterial;
    private GameObject _gameObject;
    private string _bodySecondary;
    private Color _currentColor = Color.white;
    private Color _currentSecondaryColor = Color.white;
    private Color _currentLumColor = Color.white;
    private string _lumMaterial;

    [SerializeField] private GameObject[] _statusLightMeshes;
    private List<Material> _statusLightMaterials = new();
    private ColorTemplate _currentTemplate = new ColorTemplate
    {
        PrimaryColor = Color.white,
        SecondaryColor = Color.white,
        EmissionColor = Color.cyan
    };
    private bool isConnectedToBase;
    private int colorIndex;
    private int len;

    float t = 0;

    private void Start()
    {
        len = lerpColors.Length;


        
    }
    private void Update()
    {
        if (controller is not null)
        {
            UpdateStatusLights();
        }
    }

    private void UpdateStatusLights()
    {
        if (_statusLightMaterials is null) return;

        foreach (Material material in _statusLightMaterials)
        {
            switch (controller.GetState())
            {
                default:
                case FCSDeviceState.Off:
                    material.SetVector("_GlowColor", statusLightOff);
                    break;
                case FCSDeviceState.Idle:
                    material.SetVector("_GlowColor", LerpLights(material));
                    break;
                case FCSDeviceState.Running:
                    material.SetVector("_GlowColor", statusLightOn);
                    break;
                case FCSDeviceState.NotConnected:
                    material.SetVector("_GlowColor", statusLightDisconnected);
                    break;

            }
        }
    }

    private Color LerpLights(Material material)
    {
        var time = lerpTime * Time.deltaTime;
        var color = Color.Lerp(material.GetVector("_GlowColor"), lerpColors[colorIndex], time);

        t = Mathf.Lerp(t, 1f, time);
        if (t > .9f)
        {
            t = 0f;
            colorIndex++;
            colorIndex = (colorIndex >= len) ? 0 : colorIndex;
        }

        return color;
    }

    public void Initialize(GameObject gameObject)
    {
        _gameObject = gameObject;
        _bodyMaterial = ModPrefabService.BasePrimaryCol;
        _bodySecondary = ModPrefabService.BaseSecondaryCol;
        _lumMaterial = ModPrefabService.TBaseEmission;

        if (_interiorMeshes != null)
        {
            foreach (GameObject mesh in _statusLightMeshes)
            {
                var material = MaterialHelpers.GetMaterial(mesh, _bodyMaterial);

                if (material != null)
                {
                    _statusLightMaterials.Add(material);
                }
            }
        }

        SetInterior();
    }

    private void SetInterior()
    {
        if(_interiorMeshes is null)
        {
            QuickLogger.DebugError($"Interior Meshes returned null on {gameObject.name} canceling operation.");
            return;
        }
        foreach (GameObject go in _interiorMeshes)
        {
            var material = go.GetComponent<Renderer>().material;

            MaterialHelpers.ChangeEmissionColor(material.name, go, Color.white);
            MaterialHelpers.ChangeEmissionStrength(material.name, go, _interiorLumIntensity);
            MaterialHelpers.ChangeEmissionTexture(go, material);
        }


        //if (_interiorMeshes is null) return;
        //foreach (var mesh in _interiorMeshes)
        //{
        //    foreach (MaterialMatch match in _replacementInteriorMaterials)
        //    {
        //        MaterialHelpers.ChangeEmissionColor(match.Material.name, mesh, Color.white);
        //        MaterialHelpers.ChangeEmissionStrength(match.Material.name, mesh, _interiorLumIntensity);
        //        MaterialHelpers.ChangeEmissionTexture(mesh, match.Material);
        //    }
        //}
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
            foreach (GameObject item in _primaryMeshes)
            {
                MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, template.PrimaryColor);
            }

            foreach (GameObject item in _secondaryMeshes)
            {
                MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, template.SecondaryColor);
            }

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

    internal void ChangeBaseConnectionStatusLights(bool isConnectedToBase)
    {
        this.isConnectedToBase = isConnectedToBase;
    }
}
