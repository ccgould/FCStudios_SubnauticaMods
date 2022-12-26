﻿using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace FCS_AlterraHub.Model
{
    public abstract class UpgradeFunction
    {
        private bool _isEnabled;
        public virtual Text Label { get; set; }
        public string Function => GetFunction();

        public virtual string GetFunction()
        {
            return string.Empty;
        }

        public abstract float PowerUsage { get; }
        public abstract float Damage { get; }
        public abstract UpgradeFunctions UpgradeType { get; }
        public abstract string FriendlyName { get; }
        [JsonIgnore] public FcsDevice Mono { get; set; }
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
            
            UpdateLabel();
            QuickLogger.Debug($"{FriendlyName} has been {status}.");
        }

        public virtual void UpdateLabel()
        {
            if(Label != null)
            {
                Label.text = Format();
            }
        }
    }
}
