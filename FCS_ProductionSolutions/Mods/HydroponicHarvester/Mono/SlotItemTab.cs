using System;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Models;
using FCS_ProductionSolutions.Structs;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.Mods.HydroponicHarvester.Mono
{
    internal class SlotItemTab : InterfaceButton
    {
        private uGUI_Icon _icon;
        internal PlantSlot Slot;
        private Text _amount;
        private DisplayManager _display;
        private bool Initialized => _icon != null;

        internal void Initialize(DisplayManager display, Action<string,object> onButtonClicked,int slotIndex)
        {
            if (Initialized) return;

            _display = display;
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
            
            var removeBTN = GameObjectHelpers.FindGameObject(gameObject.transform.parent.gameObject, "RemoveDNABTN");
            var removeDnaBtn = removeBTN.AddComponent<FCSMultiClickButton>();
            removeDnaBtn.onSingleClick += () =>
            {
                Slot.TryClear();
            };

            removeDnaBtn.onLongPress += OnLongPress;
            removeDnaBtn.TextLineOne = AuxPatchers.HarvesterDeleteSample();
            removeDnaBtn.TextLineTwo = AuxPatchers.HarvesterDeleteSampleDesc();

            //var removeDnaBtnLongPressListener = removeBTN.AddComponent<ButtonLongPressListener>();
            //removeDnaBtnLongPressListener.onLongPress +=OnLongPress;

            _amount = InterfaceHelpers.FindGameObject(gameObject.transform.parent.gameObject, "Amount").GetComponent<Text>();
            UpdateCount();

            HOVER_COLOR = Color.white;
            STARTING_COLOR = new Color(.5f,.5f,.5f);
            OnButtonClick += onButtonClicked;

            Slot.TrackTab(this);
        }

        private void OnLongPress()
        {
            _display.ShowMessage(AuxPatchers.ClearSlotLongPress(),FCSMessageButton.YESNO,(result =>
            {
                if (result == FCSMessageResult.OKYES)
                {
                    Slot.TryClear(true,true);
                }
            }));
        }

        internal void SetIcon(FCSDNASampleData sampleData)
        {
            if (Mod.IsHydroponicKnownTech(sampleData.TechType, out var data))
            {
                _icon.sprite = SpriteManager.Get(data.PickType);
                Slot.GrowBedManager.AddSample(sampleData.TechType, Slot.Id);
                UpdateCount();
                Tag = new SlotData(sampleData.PickType, Slot.Id);
            }
            else
            {
                QuickLogger.DebugError($"Sample Not Known on Set Icon for SlotItem class:\n TechType: {sampleData.TechType} PickType: {sampleData.PickType} Is Land Plant: {sampleData.IsLandPlant}");
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
            Tag = null;
            UpdateCount();
            _display.UpdateUI();   
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