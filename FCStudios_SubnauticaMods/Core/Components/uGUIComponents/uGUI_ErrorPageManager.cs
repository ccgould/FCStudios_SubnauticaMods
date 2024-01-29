using FCS_AlterraHub.ModItems.FCSPDA.ScriptableObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Core.Components.uGUIComponents;
public class uGUI_ErrorPageManager : MonoBehaviour
{
    public static uGUI_ErrorPageManager Instance;
    [SerializeField] Transform errorGrid;
    [SerializeField] GameObject template;

    private void Awake()
    {
        Instance = this;
    }

    public void Enter(FCSDeviceErrorHandler errorHandler)
    {
        errorHandler.OnErrorListChanged += OnErrorListChanged; 
    }

    private void OnErrorListChanged()
    {
        PurgeErrorList();
        LoadErrors();
    }

    private void LoadErrors()
    {
        throw new NotImplementedException();
    }

    private void PurgeErrorList()
    {
        throw new NotImplementedException();
    }
}
