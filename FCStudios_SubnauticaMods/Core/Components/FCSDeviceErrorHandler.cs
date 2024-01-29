using FCS_AlterraHub.Models;
using FCS_AlterraHub.ModItems.FCSPDA.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCS_AlterraHub.Core.Components;

[DisallowMultipleComponent]
public class FCSDeviceErrorHandler : MonoBehaviour
{

    private Dictionary<string, DeviceErrorModel> _errorList = new();
    public Action OnErrorListChanged;


    private void Update()
    {

        for (int i = _errorList.Count - 1; i >= 0; i--)
        {
            var item = _errorList.ElementAt(i);

            if (item.Value.Func.Invoke())
            {
                RemoveError(item.Key);
            }
        }
    }

    public void TriggerError(DeviceErrorModel error)
    {
        if(!_errorList.ContainsKey(error.errorCode))
        {
            _errorList.Add(error.errorCode, error);
            OnErrorListChanged?.Invoke();
        }
    }

    public Dictionary<string, DeviceErrorModel> GetErrors()
    {
        return _errorList;
    }

    public void RemoveError(string errorCode)
    {
        _errorList.Remove(errorCode);
        OnErrorListChanged?.Invoke();
    }

    public void Purge()
    {
        _errorList.Clear();
        OnErrorListChanged?.Invoke();
    }

    public int GetErrorsCount()
    {
        return _errorList.Count;
    }
}
