namespace FCSAIPowerCellSocket.Mono
{
    internal partial class AIPowerCellSocketController : HandTarget, IHandTarget
    {
        private bool _onInterfaceButton;

        public void OnHandClick(GUIHand hand)
        {
            if (_onInterfaceButton) return;
            if (Player.main == null) return;
            OpenStorage();
        }

        internal void OnInterfaceButton(bool value)
        {
            _onInterfaceButton = value;
        }

        public void OnHandHover(GUIHand hand)
        {
            if (_onInterfaceButton) return;
            HandReticle main = HandReticle.main;
            main.SetInteractText("Open Powercell Socket Slots Container.");
            main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
    }
}
