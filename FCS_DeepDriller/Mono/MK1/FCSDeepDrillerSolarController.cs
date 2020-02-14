namespace FCS_DeepDriller.Mono.MK1
{
    internal class FCSDeepDrillerSolarController : HandTarget, IHandTarget
    {
        private FCSDeepDrillerController _mono;

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            main.SetIcon(HandReticle.IconType.Default);
#if SUBNAUTICA
            main.SetInteractText(_mono.PowerManager.GetSolarPowerData(), false, HandReticle.Hand.None);
#elif BELOWZERO
            main.SetText(HandReticle.TextType.Hand, _mono.PowerManager.GetSolarPowerData(), false);
#endif
        }

        public void OnHandClick(GUIHand hand)
        {
            //Not needed for the solar panel
        }

        public void Setup(FCSDeepDrillerController mono)
        {
            _mono = mono;
        }
    }
}
