using UnityEngine;
using UnityEngine.EventSystems;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    public class ScreenTrigger : uGUI_InputGroup, IEventSystemHandler, uGUI_IButtonReceiver, IPointerHoverHandler
    {
        private float terminationSqrDistance = 4f;
        private Player player;
#if BELOWZERO
        private RectTransform rt;
#endif


        public override void Awake()
        {
            base.Awake();
#if BELOWZERO
            rt = GetComponent<RectTransform>();
#endif
            terminationSqrDistance = Mathf.Pow(3f, 2f);
        }


        public override void Update()
        {
            base.Update();

            if (base.focused && this.player != null && (this.player.transform.position - rt.position).sqrMagnitude >=
                this.terminationSqrDistance)
            {
                base.Deselect(null);
            }
        }

        public override void OnSelect(bool lockMovement)
        {
            base.OnSelect(lockMovement);
            this.player = Player.main;
        }

        public override void OnDeselect()
        {
            base.OnDeselect();
            this.player = null;
        }

        public void OnPointerHover(PointerEventData eventData)
        {
            if (base.enabled && !base.selected)
            {
#if SUBNAUTICA
                HandReticle.main.SetInteractText("Use Terminal");
#else
                HandReticle.main.SetTextRaw(HandReticle.TextType.Use, "Use Terminal");
#endif
                HandReticle.main.SetIcon(HandReticle.IconType.Interact);
            }
        }

        public bool OnButtonDown(GameInput.Button button)
        {
            if (button == GameInput.Button.UICancel)
            {
                base.Deselect(null);
                return true;
            }

            return false;
        }
    }
}