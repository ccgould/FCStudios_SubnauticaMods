using FCS_LifeSupportSolutions.Configuration;
using UnityEngine;

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

            if (_mono == null || !_mono.IsInitialized || !_mono.IsConstructed || _mono.Container == null) return;


            if (_mono.Container.IsContainerFull)
            {
                main.SetIcon(HandReticle.IconType.Hand, 1f);
#if SUBNAUTICA
                main.SetInteractTextRaw(AuxPatchers.TakeMedKit(),"");
#else
                main.SetTextRaw(HandReticle.TextType.Hand, AuxPatchers.TakeMedKit());
#endif
            }
            else
            {
                if (_mono.Container.GetIsEmpty() == false)
                {
                    main.SetIcon(HandReticle.IconType.Hand, 1f);
#if SUBNAUTICA
                    main.SetInteractTextRaw(AuxPatchers.TakeMedKit(),
                        $"Progress: {Mathf.FloorToInt(_mono.Container.Progress * 100)}%");
#else
                    main.SetTextRaw(HandReticle.TextType.Hand, AuxPatchers.TakeMedKit());
                    main.SetTextRaw(HandReticle.TextType.HandSubscript,
                        $"Progress: {Mathf.FloorToInt(_mono.Container.Progress * 100)}%");
#endif
                }
                else
                {
                    main.SetProgress(_mono.Container.Progress);
                    main.SetIcon(HandReticle.IconType.Progress);
                }
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