namespace FCS_DeepDriller.Mono.Handlers
{
    internal class FCSDeepDrillerSolarController : HandTarget, IHandTarget
    {
        private FCSDeepDrillerController _mono;

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            main.SetIcon(HandReticle.IconType.Default);
            main.SetInteractText(_mono.PowerManager.GetSolarPowerData(), false, HandReticle.Hand.None);
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
