using UnityEngine;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    internal class LadderController : HandTarget, IHandTarget
    {
        Transform _target;

        public void Set(GameObject target)
        {
            _target = target.transform;
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetInteractText(Language.main.Get("UseLadder"));
            HandReticle.main.SetIcon(HandReticle.IconType.Interact);
        }

        public void OnHandClick(GUIHand hand)
        {
            Player.main.SetPosition(_target.position);
        }
    }
}