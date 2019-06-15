using FCSAIPowerCellSocket.Buildables;
using FCSCommon.Helpers;

namespace FCSAIPowerCellSocket.Mono
{
    internal partial class AIPowerCellSocketController : HandTarget, IHandTarget
    {
        public void OnHandClick(GUIHand hand)
        {
            if (Player.main == null) return;
            OpenStorage();
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            main.SetInteractText(LanguageHelpers.GetLanguage(AIPowerCellSocketBuildable.OnHandOverKey));
            main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
    }
}
