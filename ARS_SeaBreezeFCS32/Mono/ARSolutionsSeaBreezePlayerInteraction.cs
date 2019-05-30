using FCSCommon.Utilities;

namespace ARS_SeaBreezeFCS32.Mono
{
    internal partial class ARSolutionsSeaBreezeController : HandTarget, IHandTarget
    {
        public void OnHandClick(GUIHand hand)
        {
            if (Player.main == null) return;
            QuickLogger.Debug("Clicked on Seabreeze {}", true);
            OpenStorage();
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;

            main.SetInteractText("Climb Ladder");
            main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
    }
}
