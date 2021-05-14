using UnityEngine;
using UnityEngine.EventSystems;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    public class ScreenTrigger : uGUI_InputGroup, IEventSystemHandler, uGUI_IButtonReceiver, IPointerHoverHandler
    {
        private float terminationSqrDistance = 4f;
        private Player player;


        public override void Awake()
        {
            base.Awake();
            terminationSqrDistance = Mathf.Pow(3f, 2f);
        }


        public override void Update()
        {
            base.Update();

            if (base.focused && this.player != null && (this.player.transform.position - base.rt.position).sqrMagnitude >= this.terminationSqrDistance)
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
                HandReticle.main.SetInteractText("Use Terminal");
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