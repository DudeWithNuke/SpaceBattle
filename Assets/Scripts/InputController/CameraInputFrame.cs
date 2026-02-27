using UnityEngine;

namespace InputController
{
    public readonly struct CameraInputFrame
    {
        public readonly bool SwitchBattlefieldRequested;
        public readonly bool OrbitStarted;
        public readonly bool OrbitEnded;
        public readonly Vector2 OrbitDelta;
        public readonly Vector2 MoveInput;
        public readonly float ZoomDelta;

        public CameraInputFrame(
            bool switchBattlefieldRequested,
            bool orbitStarted,
            bool orbitEnded,
            Vector2 orbitDelta,
            Vector2 moveInput,
            float zoomDelta)
        {
            SwitchBattlefieldRequested = switchBattlefieldRequested;
            OrbitStarted = orbitStarted;
            OrbitEnded = orbitEnded;
            OrbitDelta = orbitDelta;
            MoveInput = moveInput;
            ZoomDelta = zoomDelta;
        }
    }
}
