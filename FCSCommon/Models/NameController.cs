using System;
using FCSCommon.Interfaces;
using FCSCommon.Utilities;

namespace FCSCommon.Models
{
    public class NameController
    {
        public Action<string> OnLabelChanged;
        private IRenameNameTarget _target;
        private string _title;
        private string _name;
        private string _submitLabel;

        public void Initialize(IRenameNameTarget target, string submitLabel, string title)
        {
            QuickLogger.Debug("Initializing Name Controller");
            _target = target;
            _submitLabel = submitLabel;
            _title = title;
        }

        public void Show()
        {
            uGUI.main.userInput.RequestString(_title, _submitLabel, _name, 25, SetCurrentName);
        }

        public string GetCurrentName()
        {
            return _name;
        }

        public void SetCurrentName(string name)
        {
            QuickLogger.Debug($"Setting unit name to : {name}");
            _name = name;
            OnLabelChanged?.Invoke(name);
        }
    }
}
