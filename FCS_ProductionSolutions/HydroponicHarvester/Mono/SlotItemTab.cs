using System;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.HydroponicHarvester.Models;
using FCSCommon.Helpers;
using UnityEngine;
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

            BtnName = "SlotButton";

            Slot = display._mono.GrowBedManager.GetSlot(slotIndex);
            _icon = gameObject.AddComponent<uGUI_Icon>();
            _icon.sprite = SpriteManager.defaultSprite;
            
            var addDnaBtn = GameObjectHelpers.FindGameObject(gameObject.transform.parent.gameObject, "AddDNABTN").AddComponent<InterfaceButton>();
            addDnaBtn.STARTING_COLOR = Color.gray;
            addDnaBtn.HOVER_COLOR = Color.white;
            addDnaBtn.OnButtonClick += (s, o) => { display.OpenDnaSamplesPage(this); };
            addDnaBtn.TextLineOne = AuxPatchers.HarvesterAddSample();
            addDnaBtn.TextLineTwo = AuxPatchers.HarvesterAddSampleDesc();
            
            var removeDnaBtn = GameObjectHelpers.FindGameObject(gameObject.transform.parent.gameObject, "RemoveDNABTN").AddComponent<InterfaceButton>();
            removeDnaBtn.STARTING_COLOR = Color.gray;
            removeDnaBtn.HOVER_COLOR = Color.white;
            removeDnaBtn.OnButtonClick += (s, o) =>
            {
                var result = Slot.TryClear();
                if (result)
                {
                    Clear();
                }
            };
            removeDnaBtn.TextLineOne = AuxPatchers.HarvesterDeleteSample();
            removeDnaBtn.TextLineTwo = AuxPatchers.HarvesterDeleteSampleDesc();

            _amount = InterfaceHelpers.FindGameObject(gameObject.transform.parent.gameObject, "Amount").GetComponent<Text>();
            UpdateCount();

            HOVER_COLOR = Color.white;
            STARTING_COLOR = new Color(.5f,.5f,.5f);
            OnButtonClick += onButtonClicked;

            Slot.TrackTab(this);
        }

        internal void SetIcon(DNASampleData sampleData)
        {
            if (Mod.IsHydroponicKnownTech(sampleData.TechType, out var data))
            {
                _iconTechType = sampleData.PickType;
                _icon.sprite = SpriteManager.Get(data.PickType);
                Slot.GrowBedManager.AddSample(sampleData.TechType, Slot.Id);
                UpdateCount();
                Tag = new SlotData(sampleData.PickType, Slot.Id);
            }
            else
            {
                _icon.sprite = SpriteManager.Get(TechType.None);
            }
        }

        internal void UpdateCount()
        {
            _amount.text = $"{Slot.GetCount()}/{Slot.GetMaxCapacity()}";
        }

        internal void Clear()
        {
            _icon.sprite = SpriteManager.Get(TechType.None);
            _iconTechType = TechType.None;
            Tag = null;

        }

        public void Load(TechType techType)
        {
            if (Mod.IsHydroponicKnownTech(techType, out var data))
            {
                SetIcon(data);
                UpdateCount();
            }
        }
    }
}