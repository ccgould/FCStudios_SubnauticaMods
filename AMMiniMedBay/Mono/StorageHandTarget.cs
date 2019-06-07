using FCSCommon.Utilities;

namespace AMMiniMedBay.Mono
{
    internal class StorageHandTarget : HandTarget, IHandTarget
    {
        private AMMiniMedBayController _mono;

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            main.SetInteractText("Open MedBay Storage.");
            main.SetIcon(HandReticle.IconType.Hand, 1f);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (Player.main == null) return;
            QuickLogger.Debug($"Clicked on Storage Container", true);
            _mono.Container.OpenStorage();
        }

        public void Initialize(AMMiniMedBayController mono)
        {
            _mono = mono;
        }
    }
}
