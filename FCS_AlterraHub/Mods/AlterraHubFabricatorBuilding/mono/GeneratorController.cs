using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCSCommon.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    internal class GeneratorController : MonoBehaviour
    {
        private AlterraFabricatorStationController _controller;
        private readonly List<PowercellSlot> _slots = new List<PowercellSlot>();
        private Text _status;
        private Text _message;
        private Button _activateBtn;
        private bool _hasBeenActivated;
        private FMOD_CustomLoopingEmitter _machineSound;

        internal void UpdateScreen()
        {
            if (!_controller.IsPowerOn)
            {
                _message.text = $"Generator offline please replace powercells. Amount need {Mod.GamePlaySettings.AlterraHubDepotPowercellSlot.Count}/5";
            }

            if (Mod.GamePlaySettings.AlterraHubDepotPowercellSlot.Count >= 5)
            {
                _activateBtn.interactable = true;
            }

            if (_controller.IsPowerOn && !_hasBeenActivated)
            {
                _message.text = "System Activated";
                _message.color = Color.green;
                _status.text = "System OK";
                _status.color = Color.green;
                _activateBtn.gameObject.SetActive(false);
                _hasBeenActivated = true;
            }
        }

        internal void Initialize(AlterraFabricatorStationController controller)
        {
            _controller = controller;
            var generator = GameObjectHelpers.FindGameObject(controller.gameObject, "Generator");
            var slots = GameObjectHelpers.FindGameObjects(generator, "BatterySlot_", SearchOption.StartsWith);
            _status = GameObjectHelpers.FindGameObject(generator, "EmergencyMode").GetComponent<Text>();
            _message = GameObjectHelpers.FindGameObject(generator, "Message").GetComponent<Text>();
            _activateBtn = GameObjectHelpers.FindGameObject(generator, "ActivateButton").GetComponent<Button>();
            _activateBtn.onClick.AddListener(()=>
            {
                controller.TurnOnBase();
                UpdateScreen();
                _machineSound.Play();
            });

            for (int i = 0; i < slots.Count(); i++)
            {
                var slotId = $"slot_{i}";
                var pSlot = slots.ElementAt(i).gameObject.AddComponent<PowercellSlot>();
                pSlot.Initialize(this,_controller,slotId);
                _slots.Add(pSlot);
            }

            _machineSound = FModHelpers.CreateCustomLoopingEmitter(gameObject, "water_filter_loop", "event:/sub/base/water_filter_loop");
        }

        public void LoadSave()
        {
            foreach (PowercellSlot powercellSlot in _slots)
            {
                if (Mod.GamePlaySettings.AlterraHubDepotPowercellSlot.Contains(powercellSlot.GetID()))
                {
                    powercellSlot.AllowBatteryReplacement = false;
                }
            }

            UpdateScreen();
        }
    }
}