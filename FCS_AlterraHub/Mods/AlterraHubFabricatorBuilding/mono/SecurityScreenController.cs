using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCSCommon.Helpers;
using UnityEngine;
using UnityEngine.UI;
using WorldHelpers = FCS_AlterraHub.Helpers.WorldHelpers;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    internal class SecurityScreenController : MonoBehaviour
    {
        private Text _clock;
        private GameObject _ancTextEditorDialog;
        private GameObject _frcTextEditorDialog;
        private FCSMessageBox _messageBox;
        private Button[] _buttons;
        private bool _initialized;
        private GameObject _noPower;

        private void Awake()
        {
            _clock = GameObjectHelpers.FindGameObject(gameObject, "Clock").GetComponent<Text>();
            _messageBox = GameObjectHelpers.FindGameObject(gameObject, "MessageBox").AddComponent<FCSMessageBox>();
            _ancTextEditorDialog = GameObjectHelpers.FindGameObject(gameObject, "ANCTextEditorDialog");
            _frcTextEditorDialog = GameObjectHelpers.FindGameObject(gameObject, "FRCTextEditorDialog");
            _noPower = GameObjectHelpers.FindGameObject(gameObject, "NoPowerPage");
            _buttons = gameObject.GetComponentsInChildren<Button>();
        }

        private void Update()
        {
            if(_clock != null)
                _clock.text = WorldHelpers.GetGameTimeFormat();
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

        private void OnFRCClick()
        {
            _frcTextEditorDialog.SetActive(true);
        }

        private void Close()
        {
            _ancTextEditorDialog?.SetActive(false);
            _frcTextEditorDialog?.SetActive(false);
        }

        internal void TurnOn()
        {
            _noPower.SetActive(false);
        }
    }
}