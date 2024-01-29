using FCS_AlterraHub.Core.Components;
using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.FCSPDA.ScriptableObjects;

[CreateAssetMenu(fileName = "FCSDevice Error", menuName = "FCSStudios/FCSDeviceError")]
public class FCSDeviceErrorSO : ScriptableObject
{
    public void SetData(FCSDeviceErrorHandler errorHander, Func<bool> func)
    {
        ErrorHandler = errorHander;
        Func = func;

    }
    public string errorMessage;
    public string errorCode = Guid.NewGuid().ToString();

    [HideInInspector] public Func<bool> Func { get; private set; }
    [HideInInspector] public FCSDeviceErrorHandler ErrorHandler { get; private set; }
    }