using System;
using FCSTechFabricator.Components;

namespace FCSCommon.Interfaces
{
    internal interface IRenameable
    {
        void RenameDevice(string newName);
        string GetDeviceName();
        void SetNameControllerTag(object itemButton);
        Action<string, NameController> OnLabelChanged { get; set; }
    }
}
