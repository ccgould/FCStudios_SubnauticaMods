using ExStorageDepot.Buildable;
using System;

namespace ExStorageDepot.Mono.Managers
{
    internal class ExStorageDepotNameManager
    {
        internal Action<string> OnLabelChanged;
        private ExStorageDepotController _mono;
        private string _currentName = "Ex-Storage Depot Unit";

        internal void Initialize(ExStorageDepotController mono)
        {
            _mono = mono;

        }

        internal void Show()
        {
            uGUI.main.userInput.RequestString(ExStorageDepotBuildable.RenameStorage(), ExStorageDepotBuildable.Submit(), _currentName, 25, new uGUI_UserInput.UserInputCallback(SetLabel));
        }

        internal void SetLabel(string newLabel)
        {
            _currentName = newLabel;
            OnLabelChanged?.Invoke(newLabel);
        }

        internal string GetCurrentName()
        {
            return _currentName;
        }

        internal void SetCurrentName(string name)
        {
            _currentName = name;
            OnLabelChanged?.Invoke(name);
        }
    }
}
