﻿using FCS_AlterraHub.Mods.AlterraHubDepot.Mono;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem;
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

        internal void Initialize(AlterraDronePortController depot,ToggleGroup toggleGroup, Transform list)
        {
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
        }

        public void UnRegisterAndDestroy()
        {
            _toggleGroup.UnregisterToggle(_toggle);
            Destroy(gameObject);
        }
    }
}