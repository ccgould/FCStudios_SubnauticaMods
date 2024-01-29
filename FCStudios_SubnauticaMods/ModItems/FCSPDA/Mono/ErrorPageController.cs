using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Components.uGUIComponents;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono;

internal class ErrorPageController : Page
{
    [SerializeField]
    private Transform _content;
    [SerializeField]
    private GameObject _categoryPrefab;
    private FCSDeviceErrorHandler _errorHandler;

    public void OnShowAllToggleChanged()
    {
        QuickLogger.Debug("OnShowAllToggleChanged");
        Enter(null);
    }

    public override void Enter(object arg)
    {
        base.Enter();
                
        if (arg is not null)
        {
            ////Generate a list of devices

            var device = arg as FCSDevice;
            _errorHandler = device.GetDeviceErrorHandler();
            _errorHandler.OnErrorListChanged += OnErrorListChanged;
            
            LoadErrors();
        }
    }

    private void LoadErrors()
    {
        var errors = _errorHandler.GetErrors();


        if (errors is not null)
        {
            foreach (var error in errors)
            {
                var category = Instantiate(_categoryPrefab);
                category.transform.SetParent(_content, false);
                var deviceCat = category.GetComponent<uGUI_ErrorListItem>();
                deviceCat.SetMessage(error.Value);
            }
        }
    }

    private void OnErrorListChanged()
    {
        Purge();
        LoadErrors();
    }

    public override void Exit()
    {
        base.Exit();
        Purge();
        _errorHandler.OnErrorListChanged -= OnErrorListChanged;

    }

    private void Purge()
    {
        foreach (Transform child in _content.transform)
        {
            if (child.gameObject == _categoryPrefab) continue;
            Destroy(child.gameObject);
        }
    }
}