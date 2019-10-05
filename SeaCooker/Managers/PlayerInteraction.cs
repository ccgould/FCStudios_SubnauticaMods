using AE.SeaCooker.Buildable;
using AE.SeaCooker.Mono;
using FCSCommon.Utilities;

namespace AE.SeaCooker.Managers
{
    internal class PlayerInteraction : HandTarget, IHandTarget
    {
        private SeaCookerController _mono;

        public void OnHandHover(GUIHand hand)
        {
            if (_mono == null) return;

            HandReticle main = HandReticle.main;
            main.SetIcon(HandReticle.IconType.Interact);
            main.SetInteractText(SeaCookerBuildable.OnHover(), false, HandReticle.Hand.Left);
        }

        public void OnHandClick(GUIHand hand)
        {
            QuickLogger.Debug("Clicked SeaCooker");
            //_mono.StorageManager.OpenStorage();
        }

        internal void Initialize(SeaCookerController mono)
        {
            _mono = mono;
        }
    }
}
