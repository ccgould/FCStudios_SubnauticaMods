using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Model.Effects;
using FCSCommon.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Patches
{
    internal class MissionHUD : MonoBehaviour
    {
        private Text _header;
        private Text _subTitle;
        private bool _isInitialized;
        private EffectBuilder _scaleEffect;
        private Text _buttonPressLbl;

        private void Initialize()
        {
            if (_isInitialized) return;

            _buttonPressLbl = GameObjectHelpers.FindGameObject(gameObject, "ButtonPressLBL").GetComponent<Text>();
            UpdateButtonPressLabel();
            _scaleEffect = new EffectBuilder(this);
            _scaleEffect
                .AddEffect(new SlideRectEffect(GetComponent<RectTransform>(), new Vector3(1400.00f, 340.00f, 0f),
                    new Vector3(950.00f, 340.00f, 0f), 5f, new WaitForSeconds(5), OnEffectComplete));
            //.AddEffect(new ScaleRectEffect(GetComponent<RectTransform>(), Vector3.one, 1f, new WaitForSeconds(5), OnEffectComplete));

            _header = GameObjectHelpers.FindGameObject(gameObject, "HeaderText")?.GetComponent<Text>();
            _subTitle = GameObjectHelpers.FindGameObject(gameObject, "SubTitleText")?.GetComponent<Text>();
            _isInitialized = true;
        }

        public void UpdateButtonPressLabel()
        {
            _buttonPressLbl.text = AlterraHub.MissionButtonPressFormat(QPatch.Configuration.FCSPDAKeyCode);
        }

        private void OnEffectComplete(IUiEffect effect)
        {
            //print($"Completed {effect}!");
        }

        public void ShowMessage(string title, string subTitle)
        {
            Initialize();
            _header.text = $"<color=#ffa500ff>{title}</color>";
            _subTitle.text = $"<color=#add8e6ff>{subTitle}</color>";
            TweenIn();
        }

        private void TweenIn()
        {
            _scaleEffect.ExecuteEffects();
        }

        public void ShowNewMessagePopUp(string contactName)
        {
            ShowMessage("New Message", $"Message from {contactName}");
        }
    }
}