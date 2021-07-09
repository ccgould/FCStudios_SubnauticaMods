using System;
using FCS_AlterraHub.Mono;
using FCS_EnergySolutions.Configuration;

namespace FCS_EnergySolutions.Mods.PowerStorage.Mono
{
    internal class PowerStorageHandTargetController: HandTarget, IHandTarget
    {
        public FCSStorage storage;
        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            main.SetInteractText(AuxPatchers.PowerStorageClickToAddPowercells());
            main.SetIcon(HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {
            storage?.Open(Player.main.transform);
        }
    }
}
