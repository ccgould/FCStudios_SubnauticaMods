using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AIMarineTurbine.Mono
{
    internal class LadderController : HandTarget, IHandTarget
    {
        public void OnHandClick(GUIHand hand)
        {
            if (Player.main == null) return;
            QuickLogger.Debug("Clicked on ladder", true);
            DevConsole.SendConsoleCommand($"warp {Target.transform.position.x} {Target.transform.position.y} {Target.transform.position.z}");
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;

            main.SetInteractText("Climb Ladder");
            main.SetIcon(HandReticle.IconType.Hand, 1f);
        }

        public GameObject Target { get; set; }
    }
}
