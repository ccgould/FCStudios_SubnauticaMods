using MAC.OxStation.Mono;

namespace MAC.OxStation.Managers
{
    internal class PlayerInteractionManager : HandTarget, IHandTarget
    {
        private OxStationController _mono;


        internal void Initialize(OxStationController mono)
        {
            _mono = mono;
        }
        public void OnHandHover(GUIHand hand)
        {
            if (_mono == null) return;

            if(ShowBeaonMessage())
            {
                HandReticle main = HandReticle.main;

                //var f = uGUI.FormatButton(GameInput.Button.RightHand, false, " / ", false);

                main.SetIcon(HandReticle.IconType.Hand);
                main.SetInteractText($"Click to attach beacon",false, HandReticle.Hand.Right); 

                //main.SetInteractText($"{Mod.FriendlyName} cannot operate without being placed on a platform.", false, HandReticle.Hand.None);
            }
        }

        public void OnHandClick(GUIHand hand)
        {
            //Not needed for this mod
        }

        private bool ShowBeaonMessage()
        {
            if (_mono == null) return false;
            return (!_mono.IsBeaconAttached() && Inventory.main.GetHeldTool() && Inventory.main.GetHeldTool().pickupable.GetTechType() == TechType.Beacon);
        }
    }
}
