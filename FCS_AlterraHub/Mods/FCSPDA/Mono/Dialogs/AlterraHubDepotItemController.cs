using System;
using FCS_AlterraHub.Mods.Common.DroneSystem;
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
                gameObject.FindChild("ItemName").GetComponent<Text>().text = $"Name: {depot.Manager.GetBaseName()}\nStatus: {depot.GetStatus()}";
                _toggleGroup = toggleGroup;
                _toggle = gameObject.GetComponentInChildren<Toggle>();
                _toggle.group = toggleGroup;
                if (depot.IsFull)
                {
                    _toggle.enabled = false;
                    _toggle.isOn = false;
                }
                gameObject.transform.localScale = Vector3.one;
                gameObject.transform.SetParent(list,false);
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

        internal void Refresh()
        {
            gameObject.FindChild("ItemName").GetComponent<Text>().text = $"Name: {Destination.Manager.GetBaseName()}\nStatus: {Destination.GetStatus()}";

        }
    }
}