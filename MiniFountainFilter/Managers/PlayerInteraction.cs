using AE.MiniFountainFilter.Configuration;
using AE.MiniFountainFilter.Mono;
using FCSCommon.Utilities;

namespace AE.MiniFountainFilter.Managers
{
    internal class PlayerInteraction : HandTarget, IHandTarget
    {
        private MiniFountainFilterController _mono;

        public void OnHandHover(GUIHand hand)
        {
            if (_mono == null || _mono.DisplayManager.IsOnInterfaceButton() || !QPatch.Configuration.Config.AutoGenerateMode) return;

            HandReticle main = HandReticle.main;

            if (_mono.StorageManager.IsFull()) return;


            main.SetProgress(_mono.StorageManager.GetProgress());
            main.SetIcon(HandReticle.IconType.Progress, 1f);

        }

        public void OnHandClick(GUIHand hand)
        {
            QuickLogger.Debug($"Clicked {Mod.ModName}");
        }

        internal void Initialize(MiniFountainFilterController mono)
        {
            _mono = mono;
        }
    }
}
