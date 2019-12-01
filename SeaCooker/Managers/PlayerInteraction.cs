using AE.SeaCooker.Buildable;
using AE.SeaCooker.Mono;
using FCSCommon.Enums;
using FCSCommon.Utilities;

namespace AE.SeaCooker.Managers
{
    internal class PlayerInteraction : HandTarget, IHandTarget
    {
        private SeaCookerController _mono;

        public void OnHandHover(GUIHand hand)
        {
            if (_mono == null) return;

            var state = _mono.PowerManager.GetPowerState();

            QuickLogger.Debug($"PowerState: {state}");

            if (state == FCSPowerStates.Unpowered)
            {
                HandReticle main = HandReticle.main;
                main.SetIcon(HandReticle.IconType.Default);
                main.SetInteractText(SeaCookerBuildable.NoPowerAvailable());
            }

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
