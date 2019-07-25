namespace FCS_DeepDriller.Mono.Handlers
{
    internal class FCSDeepDrillerSolarController : HandTarget, IHandTarget
    {
        private FCSDeepDrillerController _mono;

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            main.SetInteractText(_mono.PowerManager.GetSolarPowerData());
            main.SetIcon(HandReticle.IconType.Hand, 1f);
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
