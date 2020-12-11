using FCS_LifeSupportSolutions.Configuration;

namespace FCS_LifeSupportSolutions.Mods.MiniMedBay.mono
{
    internal class MedKitDispenser : HandTarget, IHandTarget
    {
        private MiniMedBayController _mono;

        internal void Initialize(MiniMedBayController mono)
        {
            _mono = mono;
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;

            if (!_mono.IsInitialized || !_mono.IsConstructed) return;


            if (_mono.Container.IsContainerFull)
            {
                main.SetIcon(HandReticle.IconType.Hand, 1f);
                main.SetInteractTextRaw(AuxPatchers.TakeMedKit(),"");
            }
            else
            {
                main.SetProgress(_mono.Container.Progress);
                main.SetIcon(HandReticle.IconType.Progress, 1f);
            }
        }

        public void OnHandClick(GUIHand hand)
        {
            if (_mono != null)
            {
                _mono.Container.RemoveSingleKit();
            }
        }
    }
}
