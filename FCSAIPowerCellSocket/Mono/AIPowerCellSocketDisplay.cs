using FCSAIPowerCellSocket.Buildables;
using FCSAIPowerCellSocket.Model;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System;
using System.Collections;
using FCSCommon.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace FCSAIPowerCellSocket.Mono
{
    internal class AIPowerCellSocketDisplay : AIDisplay
    {
        public Color colorEmpty = new Color(1f, 0f, 0f, 1f);

        public Color colorHalf = new Color(1f, 1f, 0f, 1f);

        public Color colorFull = new Color(0f, 1f, 0f, 1f);

        private AIPowerCellSocketController _mono;
        private Text _s1Percent;
        private Image _s1Fill;
        private Text _s2Percent;
        private Image _s2Fill;
        private Text _s3Percent;
        private Image _s3Fill;
        private Text _s4Percent;
        private Image _s4Fill;
        private int _pageStateHash;
        private float BOOTING_ANIMATION_TIME = 2f;

        internal void Setup(AIPowerCellSocketController mono)
        {
            _mono = mono;

            _pageStateHash = UnityEngine.Animator.StringToHash("PageState");

            if (FindAllComponents())
            {
                StartCoroutine(CompleteSetup());
            }
            else
            {
                QuickLogger.Error("// ============== Error getting all Components ============== //");
            }
        }

        public override bool FindAllComponents()
        {
            #region Canvas  
            var canvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

            if (canvasGameObject == null)
            {
                QuickLogger.Error("Canvas cannot be found");
                return false;
            }
            #endregion

            #region Main
            var main = canvasGameObject.FindChild("Main")?.gameObject;
            if (main == null)
            {
                QuickLogger.Error("Main cannot be found");
                return false;
            }
            #endregion

            #region Status_LBL
            var statuslbl = main.FindChild("Status_LBL")?.gameObject.GetComponent<Text>();

            if (statuslbl == null)
            {
                QuickLogger.Error("Status_LBL cannot be found");
                return false;
            }

            statuslbl.text = LanguageHelpers.GetLanguage(AIPowerCellSocketBuildable.StatusKey);

            #endregion

            #region Slot1
            var slot1 = main.FindChild("Battery_1")?.gameObject;
            if (slot1 == null)
            {
                QuickLogger.Error("Battery_1 cannot be found");
                return false;
            }

            _s1Percent = slot1.FindChild("percentage").GetComponent<Text>();
            _s1Percent.text = Language.main.Get("ChargerSlotEmpty");
            _s1Fill = slot1.FindChild("Fill").GetComponent<Image>();
            _s1Fill.color = colorEmpty;
            _s1Fill.fillAmount = 0f;
            #endregion

            #region Slot2
            var slot2 = main.FindChild("Battery_2")?.gameObject;
            if (slot2 == null)
            {
                QuickLogger.Error("Battery_2 cannot be found");
                return false;
            }

            _s2Percent = slot2.FindChild("percentage").GetComponent<Text>();
            _s2Percent.text = Language.main.Get("ChargerSlotEmpty");
            _s2Fill = slot2.FindChild("Fill").GetComponent<Image>();
            _s2Fill.color = colorEmpty;
            _s2Fill.fillAmount = 0f;
            #endregion

            #region Slot3
            var slot3 = main.FindChild("Battery_3")?.gameObject;
            if (slot3 == null)
            {
                QuickLogger.Error("Battery_3 cannot be found");
                return false;
            }

            _s3Percent = slot3.FindChild("percentage").GetComponent<Text>();
            _s3Percent.text = Language.main.Get("ChargerSlotEmpty");
            _s3Fill = slot3.FindChild("Fill").GetComponent<Image>();
            _s3Fill.color = colorEmpty;
            _s3Fill.fillAmount = 0f;
            #endregion

            #region Slot4
            var slot4 = main.FindChild("Battery_4")?.gameObject;
            if (slot4 == null)
            {
                QuickLogger.Error("Battery_4 cannot be found");
                return false;
            }

            _s4Percent = slot4.FindChild("percentage").GetComponent<Text>();
            _s4Percent.text = Language.main.Get("ChargerSlotEmpty");
            _s4Fill = slot4.FindChild("Fill").GetComponent<Image>();
            _s4Fill.color = colorEmpty;
            _s4Fill.fillAmount = 0f;
            #endregion

            return true;
        }

        internal void UpdateVisuals(PowercellData data, int slot)
        {
            Text text = null;
            Image bar = null;

            var charge = data.Battery.charge < 1 ? 0f : data.Battery.charge;

            float percent = charge / data.Battery.capacity;

            QuickLogger.Debug($"P: {percent} | S: {slot}");

            switch (slot)
            {
                case 1:
                    text = _s1Percent;
                    bar = _s1Fill;
                    break;
                case 2:
                    text = _s2Percent;
                    bar = _s2Fill;
                    break;
                case 3:
                    text = _s3Percent;
                    bar = _s3Fill;
                    break;
                case 4:
                    text = _s4Percent;
                    bar = _s4Fill;
                    break;
            }

            if (text != null)
            {
                text.text = ((data.Battery.charge < 0f) ? Language.main.Get("ChargerSlotEmpty") : $"{Mathf.CeilToInt(percent * 100)}%");
            }

            if (bar != null)
            {
                if (data.Battery.charge >= 0f)
                {
                    Color value = (percent >= 0.5f) ? Color.Lerp(this.colorHalf, this.colorFull, 2f * percent - 1f) : Color.Lerp(this.colorEmpty, this.colorHalf, 2f * percent);
                    bar.color = value;
                    bar.fillAmount = percent;
                }
                else
                {
                    bar.color = colorEmpty;
                    bar.fillAmount = 0f;
                }
            }
        }

        internal void EmptyBatteryVisual(int slot)
        {
            Text text = null;
            Image bar = null;

            switch (slot)
            {
                case 1:
                    text = _s1Percent;
                    bar = _s1Fill;
                    break;
                case 2:
                    text = _s2Percent;
                    bar = _s2Fill;
                    break;
                case 3:
                    text = _s3Percent;
                    bar = _s3Fill;
                    break;
                case 4:
                    text = _s4Percent;
                    bar = _s4Fill;
                    break;
            }

            if (text != null)
            {
                text.text = Language.main.Get("ChargerSlotEmpty");
            }

            if (bar != null)
            {
                bar.color = colorEmpty;
                bar.fillAmount = 0f;
            }
        }

        #region Overrides   

        public override void ClearPage()
        {
            throw new NotImplementedException();
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            throw new NotImplementedException();
        }

        public override void ItemModified(TechType item, int newAmount)
        {
            throw new NotImplementedException();
        }
        public override IEnumerator PowerOff()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator PowerOn()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator ShutDown()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator CompleteSetup()
        {
            QuickLogger.Debug("In Power On", true);
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.SetIntHash(_pageStateHash, 1);
            yield return new WaitForSeconds(BOOTING_ANIMATION_TIME);
            _mono.AnimationManager.SetIntHash(_pageStateHash, 2);
            yield return new WaitForEndOfFrame();
            QuickLogger.Debug("In Power On Done", true);
        }

        public override void DrawPage(int page)
        {

        }
        #endregion
    }
}
