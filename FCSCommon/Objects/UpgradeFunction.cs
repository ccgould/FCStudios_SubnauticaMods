using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechFabricator.Abstract;
using Oculus.Newtonsoft.Json;

namespace FCSCommon.Objects
{
    internal abstract class UpgradeFunction
    {
        private bool _isEnabled;
        public string Function { get; set; }
        public abstract float PowerUsage { get; }
        public abstract float Damage { get; }
        public abstract UpgradeFunctions UpgradeType { get; }
        public abstract string FriendlyName { get; }
        [JsonIgnore] public FCSController Mono { get; set; }
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                if (value)
                {
                    ActivateUpdate();
                }
                else
                {
                    DeActivateUpdate();
                }
            }
        }
        public abstract void ActivateUpdate();
        public abstract void DeActivateUpdate();
        public abstract void TriggerUpdate();
        public abstract string Format();
        public void ToggleUpdate()
        {
            IsEnabled = !IsEnabled;

            var status = IsEnabled ? "enabled" : "disabled";

            QuickLogger.Debug($"{FriendlyName} has been {status}.");
        }
    }
}
