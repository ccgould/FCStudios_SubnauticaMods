using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.FCSPDA.ScriptableObjects;

[CreateAssetMenu(fileName = "FCSDevice Error", menuName = "FCSStudios/FCSDeviceError")]
public class FCSDeviceErrorSO : ScriptableObject
{
    public string errorMessage;
    public string errorCode;
}