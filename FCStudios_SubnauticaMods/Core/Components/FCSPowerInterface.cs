using FCS_AlterraHub.Models.Interfaces;
using System;
using UnityEngine;

namespace FCS_AlterraHub.Core.Components;
internal class FCSPowerInterface : MonoBehaviour, IPowerConsumer
{
    private string _name;
    private Func<float> _callback;
    private Func<bool> _isRunning;

    public string PrefabID { get; set; }
    //public bool IsRunning { get; internal set; }
    public float PowerPerSec { get; internal set; }
    public string DeviceFriendlyName { get; internal set; }

    public void Initialize(Func<float> callback, Func<bool> isRunning)
    {
        _callback = callback;
        _isRunning = isRunning;
    }

    public string GetDeviceName()
    {
        //var techType = UWE.Utils.GetComponentInHierarchy<TechTag>(gameObject)?.type ?? TechType.None;
        //return techType.AsString();

        return _name;
    }

    public void SetName(string name)
    {
        _name = name;
    }

    public float GetPowerUsage()
    {
        return _callback?.Invoke() ?? 0f;
    }

    public bool IsRunning()
    {
        return _isRunning?.Invoke() ?? false;
    }
}
