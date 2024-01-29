using FCS_AlterraHub.Core.Components;
using System;

namespace FCS_AlterraHub.Models;
public class DeviceErrorModel
{
    public DeviceErrorModel(string error, FCSDeviceErrorHandler fCSDeviceErrorHandler, Func<bool> func)
    {
        errorMessage = error;
        Func = func;
        ErrorHandler = fCSDeviceErrorHandler;
        this.errorCode = Guid.NewGuid().ToString();
    }


    public string errorMessage { get; }
    public Func<bool> Func { get; }
    public FCSDeviceErrorHandler ErrorHandler { get; }
    public string errorCode { get; }
}
