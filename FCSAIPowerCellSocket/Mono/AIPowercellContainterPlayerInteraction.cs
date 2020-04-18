using FCSAIPowerCellSocket.Buildables;
using FCSCommon.Helpers;

namespace FCSAIPowerCellSocket.Mono
{
    internal partial class AIPowerCellSocketController : HandTarget, IHandTarget
    {
        public void OnHandClick(GUIHand hand)
        {
            if (Player.main == null || !IsConstructed) return;
            OpenStorage();
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
#if SUBNAUTICA
            main.SetInteractText(AIPowerCellSocketBuildable.OnHover());
#elif BELOWZERO
            main.SetText(HandReticle.TextType.Hand, AIPowerCellSocketBuildable.OnHover(), false);
#endif
            main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
    }
}
