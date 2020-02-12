using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSTechFabricator.Components;

namespace FCSTechFabricator.Interfaces
{
    public interface IRenameable
    {
        void RenameDevice(string newName);
        string GetDeviceName();
        void SetNameControllerTag(object itemButton);
        Action<string, NameController> OnLabelChanged { get; set; }
    }
}
