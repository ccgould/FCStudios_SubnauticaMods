using FCSCommon.Utilities;

namespace ARS_SeaBreezeFCS32.Mono
{
    public partial class ARSolutionsSeaBreezeController : HandTarget, IHandTarget
    {
        private bool _onInterfaceButton;

        public void OnHandClick(GUIHand hand)
        {
            if (_onInterfaceButton) return;
            if (Player.main == null) return;
            QuickLogger.Debug($"Clicked on Seabreeze {PrefabId}", true);
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
            main.SetInteractText("Open Seabreeze");
            main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
    }
}
