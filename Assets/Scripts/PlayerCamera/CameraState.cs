using UnityEngine;

namespace PlayerCamera
{
    public sealed class CameraState
    {
        public Vector3 FocusPoint;
        public Vector3 LastValidPlanarForward = Vector3.forward;

        public float HorizontalAngle;
        public float VerticalAngle = 45f;
        public float ZoomDistance = 20f;

        public bool IsRotating;
        public bool IsPlayerBattlefieldActive = true;
        public bool IsSwitching;

        public Vector3 TargetFocusPoint;
    }
}
