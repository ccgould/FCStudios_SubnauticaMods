using System;
using FCS_ProductionSolutions.HydroponicHarvester.Models;
using FCSCommon.Components;
using FCSCommon.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.HydroponicHarvester.Mono
{
    internal class SlotItemTab : InterfaceButton
    {
        private uGUI_Icon _icon;
        private TechType _iconTechType;
        internal PlantSlot Slot;
        private Text _amount;
        private bool Initialized => _icon != null;

        internal void Initialize(DisplayManager display, Action<string,object> onButtonClicked,int slotIndex)
        {
            if (Initialized) return;
            
            Slot = display._mono.GrowBedManager.GetSlot(slotIndex);
            _icon = gameObject.FindChild("Icon").AddComponent<uGUI_Icon>();
            _icon.sprite = SpriteManager.defaultSprite;
            
            var addDnaBtn = GameObjectHelpers.FindGameObject(gameObject, "AddDNABTN").AddComponent<InterfaceButton>();
            addDnaBtn.STARTING_COLOR = Color.gray;
            addDnaBtn.HOVER_COLOR = Color.white;
            addDnaBtn.OnButtonClick += (s, o) => { display.OpenDnaSamplesPage(this); };
            
            var removeDnaBtn = GameObjectHelpers.FindGameObject(gameObject, "RemoveDNABTN").AddComponent<InterfaceButton>();
            removeDnaBtn.STARTING_COLOR = Color.gray;
            removeDnaBtn.HOVER_COLOR = Color.white;
            removeDnaBtn.OnButtonClick += (s, o) => { display.ClearDNASample(this); };

            _amount = InterfaceHelpers.FindGameObject(gameObject, "Amount").GetComponent<Text>();
            UpdateCount();

            HOVER_COLOR = Color.white;
            STARTING_COLOR = new Color(.5f,.5f,.5f);
            OnButtonClick += onButtonClicked;

            Slot.TrackTab(this);
        }

        internal void SetIcon(TechType techType)
        {
            _iconTechType = techType;
            _icon.sprite = SpriteManager.Get(techType);
            Slot.GrowBedManager.AddSample(techType, Slot.Id);
            Tag = techType;
        }

        internal void UpdateCount()
        {
            _amount.text = $"{Slot.GetCount()}/{Slot.GetMaxCapacity()}";
        }

        internal void Clear()
        {
            _icon.sprite = SpriteManager.Get(TechType.None);
            SetIcon(TechType.None);
        }
    }
}