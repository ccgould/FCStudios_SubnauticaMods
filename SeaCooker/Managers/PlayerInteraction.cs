using AE.SeaCooker.Buildable;
using AE.SeaCooker.Mono;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;

namespace AE.SeaCooker.Managers
{
    internal class PlayerInteraction : HandTarget, IHandTarget
    {
        private SeaCookerController _mono;

        public void OnHandHover(GUIHand hand)
        {
            if (_mono == null) return;

            var state = _mono.PowerManager.GetPowerState();
            HandReticle main = HandReticle.main;
#if DEBUG
            main.SetInteractText(SeaCookerBuildable.NoPowerAvailable());
#endif
            //QuickLogger.Debug($"PowerState: {state}");

            if (state == FCSPowerStates.Unpowered)
            {

                main.SetIcon(HandReticle.IconType.Default);
#if SUBNAUTICA
                main.SetInteractText(SeaCookerBuildable.NoPowerAvailable());
#elif BELOWZERO
                main.SetText(HandReticle.TextType.Hand, SeaCookerBuildable.NoPowerAvailable(), false);
#endif
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
