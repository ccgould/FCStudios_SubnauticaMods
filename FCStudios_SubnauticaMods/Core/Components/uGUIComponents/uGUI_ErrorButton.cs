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
    [SerializeField] private CanvasGroup canvasGroup;
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
        var count = _errorHandler.GetErrorsCount();
        _countLBL.text = count.ToString();

        if(count > 0)
        {
            canvasGroup.alpha = 1;
        }
        else
        {
            canvasGroup.alpha = 0;
        }
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
