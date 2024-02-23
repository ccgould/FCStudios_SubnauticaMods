using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Enumerators;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Model;
using FCSCommon.Utilities;
using System;
using System.Drawing.Printing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Mono.UGUI;
internal class uGUI_TelepowerManagerItem : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Toggle _toggleBtn;
    private BaseTelepowerPylonManager _targetManager;
    private BaseTelepowerPylonManager _parentManager;

    internal void Set(BaseTelepowerPylonManager manager, BaseTelepowerPylonManager parentManager)
    {
        _targetManager = manager;
        _parentManager = parentManager;
        text.text = manager.GetBaseName();
        _toggleBtn?.SetIsOnWithoutNotify(_targetManager.GetIsConnected(_parentManager.GetBaseID()) || _parentManager.GetIsConnected(_targetManager.GetBaseID()));
    }

    public void OnToggleStatechanged(bool value)
    {
        if (value)
        {
            QuickLogger.Debug($"Trying to Enable pull mode: {_parentManager.GetCurrentMode()}");

            if (_parentManager.GetCurrentMode() == TelepowerPylonMode.PULL)
            {
                if (_targetManager.IsConnectionAllowed(_parentManager))
                {
                    _parentManager.AddConnection(_targetManager);
                }
                else
                {
                    QuickLogger.Debug("Loop Detection", true);
                }
            }
            else
            {
                if (_parentManager.IsConnectionAllowed(_targetManager))
                {
                    _targetManager.AddConnection(_parentManager, true);
                }
                else
                {
                    QuickLogger.Debug("Loop Detection", true);
                }
            }
        }
        else
        {
            if (_parentManager.GetCurrentMode() == TelepowerPylonMode.PULL)
            {
                _parentManager.RemoveConnection(_targetManager);
            }
            else
            {
                _targetManager.RemoveConnection(_parentManager, true);
            }
        }
    }

    internal bool IsChecked()
    {
        return _toggleBtn.isOn;
    }
}
