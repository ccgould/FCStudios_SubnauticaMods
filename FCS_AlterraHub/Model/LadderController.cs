using System.Collections;
using UnityEngine;

namespace FCS_AlterraHub.Model
{
    public class LadderController : HandTarget, IHandTarget
    {
        Transform _target;
        private PlayerCinematicController _cinematic;

        public void Set(GameObject target)
        {
            _target = target.transform;
        }

        public void OnHandHover(GUIHand hand)
        {
#if SUBNAUTICA
            HandReticle.main.SetInteractText(Language.main.Get("UseLadder"));
#else
            HandReticle.main.SetText(HandReticle.TextType.Hand,Language.main.Get("UseLadder"),true);
#endif
            HandReticle.main.SetIcon(HandReticle.IconType.Interact);
        }

        public void OnHandClick(GUIHand hand)
        {
            Player.main.SetPosition(_target.position);
        }
    }

    public class CinematicLadderController : HandTarget, IHandTarget
    {
        private PlayerCinematicController _cinematic;
        private float _animationTime;

        public void Set(Transform animatedTransform,string animParam,string playerViewAnimationName,float animationTime)
        {
            _animationTime = animationTime;
            var root = gameObject.transform.parent.parent.parent;
            _cinematic = gameObject.AddComponent<PlayerCinematicController>();
            _cinematic.animatedTransform = animatedTransform;
            _cinematic.animator = root.gameObject.GetComponent<Animator>();
            _cinematic.animParamReceivers = new GameObject[0];
            _cinematic.animParam = animParam;
            _cinematic.playerViewAnimationName = playerViewAnimationName;
        }

        public void OnHandHover(GUIHand hand)
        {
#if SUBNAUTICA
            HandReticle.main.SetInteractText(Language.main.Get("UseLadder"));
#else
            HandReticle.main.SetText(HandReticle.TextType.Hand, Language.main.Get("UseLadder"), true);
#endif
            HandReticle.main.SetIcon(HandReticle.IconType.Interact);
        }

        public void OnHandClick(GUIHand hand)
        {
            _cinematic.StartCinematicMode(Player.main);
            StartCoroutine(WaitForEndOfAnimation());
        }

        private IEnumerator WaitForEndOfAnimation()
        {
            yield return new WaitForSeconds(_animationTime);
            _cinematic.EndCinematicMode();
        }
    }
}