using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WorldHelpers = FCS_AlterraHub.Helpers.WorldHelpers;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    internal class SecurityScreenController : uGUI_InputGroup, IPointerHoverHandler
    {
        private Text _clock;
        private GameObject _ancTextEditorDialog;
        private GameObject _frcTextEditorDialog;
        private GameObject _grcTextEditorDialog;
        private FCSMessageBox _messageBox;
        private Button[] _buttons;
        private bool _initialized;
        private GameObject _noPower;
        private float _terminationSqrDistance = 4f;
        private Player _player;
        private bool _previousBobbingSetting;
        private SecurityBoxTrigger _securityBoxTrigger;
        private RectTransform _rt;


        public override void Awake()
        {
            base.Awake();
            _rt = GetComponentInParent<RectTransform>();
            _clock = GameObjectHelpers.FindGameObject(gameObject, "Clock").GetComponent<Text>();
            _messageBox = GameObjectHelpers.FindGameObject(gameObject, "MessageBox").AddComponent<FCSMessageBox>();
            _ancTextEditorDialog = GameObjectHelpers.FindGameObject(gameObject, "ANCTextEditorDialog");
            _frcTextEditorDialog = GameObjectHelpers.FindGameObject(gameObject, "FRCTextEditorDialog");
            _grcTextEditorDialog = GameObjectHelpers.FindGameObject(gameObject, "GRCTextEditorDialog");
            _noPower = GameObjectHelpers.FindGameObject(gameObject, "NoPowerPage");
            var noPowerText = GameObjectHelpers.FindGameObject(_noPower, "Text")?.GetComponent<Text>();
            if (noPowerText != null) noPowerText.text = AlterraHub.NoPowerEmergencyMode();
            
            _buttons = gameObject.GetComponentsInChildren<Button>(true);
            _terminationSqrDistance = Mathf.Pow(3f, 2f);
        }


        public override void Update()
        {
            base.Update();

            if (_clock != null)
                _clock.text = WorldHelpers.GetGameTimeFormat();

            if (focused && _player != null && (_player.transform.position - _rt.position).sqrMagnitude >= _terminationSqrDistance)
            {
                Deselect();
            }
        }

        public override void OnSelect(bool lockMovement)
        {
            if (!_securityBoxTrigger.IsPlayerInRange)
            {
                base.Deselect();
                return;
            }
            _previousBobbingSetting = MiscSettings.cameraBobbing;
            base.OnSelect(lockMovement);
            _player = Player.main;
            MiscSettings.cameraBobbing = false;
        }



        public void Initialize(AlterraFabricatorStationController controller)
        {
            _securityBoxTrigger = controller.GetSecurityBoxTrigger();
        }

        public override void OnDeselect()
        {
            base.OnDeselect();
            _player = null;
            MiscSettings.cameraBobbing = _previousBobbingSetting;
        }

        private void Start()
        {
            if (_initialized) return;
            foreach (Button button in _buttons)
            {
                if (button.name.StartsWith("FRC"))
                {
                    button.onClick.AddListener(OnFRCClick);
                }
                else if (button.name.StartsWith("ARC"))
                {
                    button.onClick.AddListener(OnARCClick);
                }
                else if (button.name.StartsWith("GRC"))
                {
                    button.onClick.AddListener(OnGRCClick);
                }
                else if (button.name.StartsWith("Close"))
                {
                    button.onClick.AddListener(Close);
                }
                else
                {
                    button.onClick.AddListener(ShowMessage);
                }
            }

            _initialized = true;
        }

        private void ShowMessage()
        {
            _messageBox.Show("File/Directory is Corrupted",FCSMessageButton.OK,null);
        }

        private void OnARCClick()
        {
            _ancTextEditorDialog.SetActive(true);
        }

        private void OnGRCClick()
        {
            _grcTextEditorDialog.SetActive(true);
        }

        private void OnFRCClick()
        {
            _frcTextEditorDialog.SetActive(true);
        }

        private void Close()
        {
            _ancTextEditorDialog?.SetActive(false);
            _frcTextEditorDialog?.SetActive(false);
            _grcTextEditorDialog?.SetActive(false);
        }

        internal void TurnOn()
        {
            _noPower.SetActive(false);
        }

        public void OnPointerHover(PointerEventData eventData)
        {
            if (!_securityBoxTrigger.IsPlayerInRange)
            {
                HandReticle.main.SetIcon(HandReticle.IconType.Default);
                return;
            }

            if (enabled && !selected)
            {
#if SUBNAUTICA
                HandReticle.main.SetInteractText("Click to interact");
#else
                HandReticle.main.SetText(HandReticle.TextType.Hand, "Click to interact", false);
#endif
                HandReticle.main.SetIcon(HandReticle.IconType.Interact);
            }
        }
    }
}