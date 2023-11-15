using FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents;
using System;

namespace FCS_AlterraHub.ModItems.FCSPDA.Struct;
internal struct MessageBoxData
{
    public MessageBoxData(string message, FCSMessageButton button, Action<FCSMessageResult> result)
    {
        Message = message;
        Button = button;
        Result = result;
    }

    public Action<FCSMessageResult> Result { get; }

    public FCSMessageButton Button { get; }

    public string Message { get; }
}
