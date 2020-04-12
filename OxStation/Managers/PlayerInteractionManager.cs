using MAC.OxStation.Mono;

namespace MAC.OxStation.Managers
{
    internal class PlayerInteractionManager : HandTarget, IHandTarget
    {
        private OxStationController _mono;

        private bool ShowBeaconMessage()
        {
            if (_mono == null) return false;
            return (!_mono.IsBeaconAttached() && Inventory.main.GetHeldTool() && Inventory.main.GetHeldTool().pickupable.GetTechType() == TechType.Beacon);
        }

        internal void Initialize(OxStationController mono)
        {
            _mono = mono;
        }

        public void OnHandHover(GUIHand hand)
        {
            if (_mono == null) return;

            if(ShowBeaconMessage())
            {
                HandReticle main = HandReticle.main;
                main.SetIcon(HandReticle.IconType.Hand);
                main.SetInteractText($"Click to attach beacon",false, HandReticle.Hand.Right);
            }
        }

        public void OnHandClick(GUIHand hand)
        {
            //Not needed for this mod
        }
    }
}
