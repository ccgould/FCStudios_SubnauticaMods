using System;
using System.Collections;
using System.ComponentModel;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Managers.FCSAlterraHub;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mods.AlterraHubPod.Mono
{
    internal class AlterraHubPodController : SubRoot
    {
        private Image _keyPadModuleRing;
        private Button _keyPadModuleButton;
        private DoorController _chamberDoor02;
        private DoorController _chamberDoor01;
        private DoorController _slideUpDoor01;

        private Color buttonNColorDrain = new Color(255, 0, 0, 255);
        private Color buttonHColorDrain = new Color(182, 0, 0, 255);
        private Color buttonPColorDrain = new Color(255, 0, 0, 255);
        private Color buttonSColorDrain = new Color(182, 0, 0, 255);

        private Color buttonNColorFlood = new Color(0, 255, 255, 255);
        private Color buttonHColorFlood = new Color(0, 210, 255, 255);
        private Color buttonPColorFlood = new Color(0, 255, 255, 255);
        private Color buttonSColorFlood = new Color(0, 210, 255, 255);
        private Animator _screenAnimator;
        public static AlterraHubPodController main;

        public override void Awake()
        {
            main = this;

            _isFlashingHash = Animator.StringToHash("IsFlashing");

            this.LOD = GetComponent<BehaviourLOD>();
            this.rb = GetComponent<Rigidbody>();
            this.isBase = true;
            this.lightControl = GetComponentInChildren<LightingController>();
            this.modulesRoot = gameObject.transform;
            this.powerRelay = GetComponent<BasePowerRelay>();
            BaseManager.FindManager(this);

            //StartCoroutine(TryApplyMaterial());
        }

        private int _isFlashingHash;
        private Text _keyPadModuleText;
        private Text _screenStatusText;

        public override void Start()
        {
            base.Start();
            
            try
            {
                SetupDoors();

                LoadSave();

                FindScreens();

                SetupKeyPad();

                _screenAnimator = GameObjectHelpers.FindGameObject(gameObject, "ScreenStatus").GetComponent<Animator>();
                _screenStatusText = _screenAnimator.GetComponent<Text>();
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.StackTrace);
            }
        }

        private void SetupKeyPad()
        {
            var keyPadModule = GameObjectHelpers.FindGameObject(gameObject, "KeyPadModule");
            _keyPadModuleRing = GameObjectHelpers.FindGameObject(keyPadModule, "LockIcon").GetComponent<Image>();
            _keyPadModuleButton = keyPadModule.GetComponentInChildren<Button>();
            _keyPadModuleText = _keyPadModuleButton.GetComponentInChildren<Text>();
            _keyPadModuleButton.onClick.AddListener((() =>
            {
                if (Player.main.currentSub is null)
                {
                    StartCoroutine(WaterPumpSystem());
                }
                else
                {
                    StartCoroutine(WaterPumpSystem(false));
                }
            }));
        }

        private void FindScreens()
        {
            var canvas = gameObject.GetComponentsInChildren<Canvas>();

            foreach (Canvas canva in canvas)
            {
                if (canva.transform.parent.name.Equals("KeyPadModule", StringComparison.OrdinalIgnoreCase)) continue;
                var screen = canva.gameObject.EnsureComponent<FCSAlterraHubGUI>();
                screen.SetInstance(FCSAlterraHubGUISender.AlterraHub);
            }
        }

        private void LoadSave()
        {
            var settings = Mod.GamePlaySettings;
            _chamberDoor01.LoadSave(settings.AlterraHubDepotDoors.ChamberDoor01);
            _chamberDoor02.LoadSave(settings.AlterraHubDepotDoors.ChamberDoor02);
            _slideUpDoor01.LoadSave(settings.AlterraHubDepotDoors.SlideUpDoor01);
        }

        private IEnumerator WaterPumpSystem(bool isDraining = true)
        {
            _keyPadModuleButton.interactable = false;

            _slideUpDoor01.Close();
            _chamberDoor01.Close();
            _chamberDoor02.Close();


            _slideUpDoor01.LockDoor();
            _chamberDoor01.LockDoor();
            _chamberDoor02.LockDoor();

            if (isDraining)
            {
                _screenStatusText.text = "DRAINING...";
                _screenAnimator.SetBool(_isFlashingHash, true);


                yield return new WaitForSeconds(3);

                Player.main.SetCurrentSub(this);
                _slideUpDoor01.UnlockDoor();
                _slideUpDoor01.Open();
                _keyPadModuleRing.color = buttonNColorFlood;
                var colors = _keyPadModuleButton.colors;
                colors.normalColor = buttonNColorFlood;
                colors.highlightedColor = buttonHColorFlood;
                colors.pressedColor = buttonPColorFlood;
                colors.selectedColor = buttonSColorFlood;
                _keyPadModuleButton.colors = colors;
                _keyPadModuleText.text = "FLOOD";
            }
            else
            {
                _screenStatusText.text = "FLOODING...";
                _screenAnimator.SetBool(_isFlashingHash, true);
                yield return new WaitForSeconds(3);

                Player.main.SetCurrentSub(null);
                _chamberDoor01.UnlockDoor();
                _chamberDoor02.UnlockDoor();
                _chamberDoor01.Open();
                _chamberDoor02.Open();

                _keyPadModuleRing.color = buttonNColorDrain;
                var colors = _keyPadModuleButton.colors;
                colors.normalColor = buttonNColorDrain;
                colors.highlightedColor = buttonHColorDrain;
                colors.pressedColor = buttonPColorDrain;
                colors.selectedColor = buttonSColorDrain;
                _keyPadModuleButton.colors = colors;
                _keyPadModuleText.text = "DRAIN";
            }

            _screenAnimator.SetBool(_isFlashingHash, false);
            _keyPadModuleButton.interactable = true;

            yield break;
        }


        private void SetupDoors()
        {
            //var interiorTrigger = GameObjectHelpers.FindGameObject(gameObject, "InteriorTrigger").EnsureComponent<InteriorTrigger>();

            var externalDoorTrigger = GameObjectHelpers.FindGameObject(gameObject, "ExternalDoorTrigger").EnsureComponent<ExteriorTrigger>();

            _chamberDoor01 = GameObjectHelpers.FindGameObject(gameObject, "ChamberDoor01").AddComponent<DoorController>();
            _chamberDoor01.PositionOffset = 0.526f;
            _chamberDoor01.UnlockDoor();
            _chamberDoor01.SetDoorDirection(DoorController.DoorDirection.Right);
            externalDoorTrigger.Door1 = _chamberDoor01;


            _chamberDoor02 = GameObjectHelpers.FindGameObject(gameObject, "ChamberDoor02").AddComponent<DoorController>();
            _chamberDoor02.PositionOffset = 0.526f;
            _chamberDoor02.UnlockDoor();
            _chamberDoor02.SetDoorDirection(DoorController.DoorDirection.Left);
            externalDoorTrigger.Door2 = _chamberDoor02;

            _slideUpDoor01 = GameObjectHelpers.FindGameObject(gameObject, "SlideUpDoor01").AddComponent<DoorController>();
            _slideUpDoor01.PositionOffset = 2.074096f;
            _slideUpDoor01.SetDoorDirection(DoorController.DoorDirection.Up);
            _slideUpDoor01.UnlockDoor();
        }


        private IEnumerator TryApplyMaterial()
        {
            while (MaterialHelpers._waterMaterial == null)
            {
                yield return null;
            }
            
            var waterPlane = GameObjectHelpers.FindGameObject(gameObject, "water");
            waterPlane.SetActive(false);
            var gam = waterPlane.AddComponent<WaterPlane>();
            gam.material = MaterialHelpers._waterMaterial;
            gam.size = new Vector2(1, 1);
            waterPlane.SetActive(true);

            //var waterPlane = GameObjectHelpers.FindGameObject(gameObject, "WaterPlane");

            //QuickLogger.Info($"Water Plane Found {waterPlane is not null}");

            //MaterialHelpers.ApplyWaterShader(waterPlane);

            yield break;

        }

        internal void Save()
        {
            QuickLogger.Debug("Saving Station");
            //Mod.GamePlaySettings.FabStationBeaconColorIndex = GetPing().colorIndex;
            //Mod.GamePlaySettings.FabStationBeaconVisible = GetPing().visible;
            Mod.GamePlaySettings.AlterraHubDepotDoors.ChamberDoor02 = new(_chamberDoor01.doorLocked, _chamberDoor01.doorOpen);
            Mod.GamePlaySettings.AlterraHubDepotDoors.ChamberDoor02 = new(_chamberDoor02.doorLocked, _chamberDoor02.doorOpen);
            Mod.GamePlaySettings.AlterraHubDepotDoors.SlideUpDoor01 = new(_slideUpDoor01.doorLocked, _slideUpDoor01.doorOpen);
            //Mod.GamePlaySettings.AlterraHubDepotDoors.KeyPad4 = _keyPads[3].IsUnlocked();
            //Mod.GamePlaySettings.AlterraHubDepotPowercellSlot = _generator.Save().ToList();
            //Mod.GamePlaySettings.IsPDAUnlocked = DetermineIfUnlocked();
            //Mod.GamePlaySettings.CurrentOrder = _currentOrder;
            //Mod.GamePlaySettings.BreakerOn = _isBaseOn;
            //Mod.GamePlaySettings.FixedPowerBoxes = _antenna.Save().ToHashSet();
            //Mod.GamePlaySettings.TransDroneSpawned = _drones.Any();
            QuickLogger.Debug("Saving Station Complete");
        }

        internal class InteriorTrigger : MonoBehaviour
        {
            private SubRoot _subRoot;


            public  void Awake()
            {
                _subRoot = gameObject.GetComponentInParent<SubRoot>();
            }
            
            public void OnTriggerEnter(Collider collider)
            {
                if (collider.gameObject.layer != 19) return;

                if (_subRoot != null)
                {
                    Player.main.SetCurrentSub(_subRoot);
                }
            }
        }

        internal class ExteriorTrigger : MonoBehaviour
        {
            public DoorController Door1;
            public DoorController Door2;
            private SubRoot _subRoot;


            public void Awake()
            {
                _subRoot = gameObject.GetComponentInParent<SubRoot>();
            }

            private void OnTriggerEnter(Collider collider)
            {
                if (collider.gameObject.layer != 19 || !Door1.IsUnlocked || !Door2.IsUnlocked) return;
                Door1.Open();
                Door2.Open();
            }

            private void OnTriggerExit(Collider collider)
            {
                if (collider.gameObject.layer != 19 || !Door1.IsUnlocked || !Door2.IsUnlocked) return;
                Door1.Close();
                Door2.Close();
            }
        }
    }
}
