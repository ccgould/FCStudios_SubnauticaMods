using FCS_AlterraHub.Models.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Core.Components.uGUIComponents;
internal class uGUI_ErrorButton : MonoBehaviour
{
    [SerializeField] private Text _countLBL;
    private FCSDeviceErrorHandler _errorHandler;

    public void SetDevice(FCSDevice device)
    {
        _errorHandler = device.GetDeviceErrorHandler();
        
        if(_errorHandler != null)
        {
            _errorHandler.OnErrorListChanged += OnErrorListChanged;
            _countLBL.text = _errorHandler.GetErrorsCount().ToString();
        }
    }

    private void OnErrorListChanged()
    {
        _countLBL.text = _errorHandler.GetErrorsCount().ToString();
    }

    public void Purge()
    {
        if(_errorHandler is not null)
        {
            _errorHandler.OnErrorListChanged -= OnErrorListChanged;
        }

        if(_countLBL is not null) 
        {
            _countLBL.text = "0";
        }
    }
}
