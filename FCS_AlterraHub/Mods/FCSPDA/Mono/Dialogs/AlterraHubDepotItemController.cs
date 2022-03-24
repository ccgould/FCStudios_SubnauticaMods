using System;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs
{
    internal class AlterraHubDepotItemController : MonoBehaviour
    {
        internal AlterraDronePortController Destination { get; set; }
        internal bool IsChecked => _toggle.isOn;

        private Toggle _toggle;
        private ToggleGroup _toggleGroup;

        internal bool Initialize(AlterraDronePortController depot,ToggleGroup toggleGroup, Transform list)
        {
            try
            {
                if (depot?.Manager == null || toggleGroup == null || list == null) return false;


                Destination = depot;
                QuickLogger.Debug("1");
                gameObject.FindChild("ItemName").GetComponent<Text>().text = $"Name: {depot.Manager.GetBaseName()}\nStatus: {depot.GetStatus()}";
                QuickLogger.Debug("2");
                _toggleGroup = toggleGroup;
                _toggle = gameObject.GetComponentInChildren<Toggle>();
                QuickLogger.Debug("3");
                _toggle.group = toggleGroup;
                QuickLogger.Debug("4");
                if (depot.IsFull)
                {
                    _toggle.enabled = false;
                    _toggle.isOn = false;
                }
                QuickLogger.Debug("5");
                gameObject.transform.localScale = Vector3.one;
                QuickLogger.Debug("6");
                gameObject.transform.SetParent(list,false);
                QuickLogger.Debug("7");
                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }
        }

        public void UnRegisterAndDestroy()
        {
            _toggleGroup.UnregisterToggle(_toggle);
            Destroy(gameObject);
        }
    }
}