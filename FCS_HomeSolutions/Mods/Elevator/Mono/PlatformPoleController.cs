using FCS_HomeSolutions.Patches;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Elevator.Mono
{
    internal class PlatformPoleController : MonoBehaviour
    {
        private Transform _trans;
        private const float Offset = -0.98f;
        private FCSElevatorController _controller;
        private const float PoleHeight = 1.5983f;

        internal void Initialize(FCSElevatorController controller)
        {
            _trans = gameObject.transform;
            _controller = controller;
            ElevatorHUD.Main.onFloorAdded += UpdatePole;
            ElevatorHUD.Main.onFloorRemoved += UpdatePole;
        }
        
        internal void UpdatePole()
        {
            if (_controller == null || _trans == null) return;

            var pos = _controller.GetHighestFloor()?.localPosition.y ?? 0f;
            _trans.localScale = new Vector3(1f, pos < 1f ? 1 : (pos / PoleHeight) - Offset, 1f);
        }

        private void OnDestroy()
        {
            ElevatorHUD.Main.onFloorAdded -= UpdatePole;
            ElevatorHUD.Main.onFloorRemoved -= UpdatePole;
        }
    }
}