namespace FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono
{
    internal class FCSDeepDrillerSolarController : HandTarget, IHandTarget
    {
        private FCSDeepDrillerController _mono;

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            main.SetIcon(HandReticle.IconType.Default);
            main.SetText(HandReticle.TextType.Hand, _mono.DeepDrillerPowerManager.GetSolarPowerData(), false);
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
