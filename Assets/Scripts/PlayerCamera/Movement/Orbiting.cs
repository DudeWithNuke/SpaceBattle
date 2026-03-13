using ScriptableObjects;
using UnityEngine;

namespace PlayerCamera.Movement
{
    internal class Orbiting
    {
        private readonly CameraSettings _cameraSettings;

        public Orbiting(CameraSettings cameraSettings)
        {
            _cameraSettings = cameraSettings;
        }

        public void Handle(CameraState state, bool orbitStarted, bool orbitEnded, Vector2 orbitDelta)
        {
            if (orbitStarted)
            {
                state.IsRotating = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (state.IsRotating)
            {
                var mouseX = orbitDelta.x;
                var mouseY = orbitDelta.y;

                state.HorizontalAngle += mouseX * _cameraSettings.rotationSpeed;
                state.VerticalAngle += -mouseY * _cameraSettings.rotationSpeed;
                state.VerticalAngle = ClampVerticalAngle(state.VerticalAngle);
            }

            if (orbitEnded)
                StopOrbit(state);
        }

        public static void StopOrbit(CameraState state)
        {
            if (!state.IsRotating)
                return;

            state.IsRotating = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private float ClampVerticalAngle(float angle)
        {
            var minVerticalAngle = Mathf.Max(90f - _cameraSettings.maxAngleAbovePlane, _cameraSettings.poleSafetyAngle);
            var maxVerticalAngle = Mathf.Min(90f + _cameraSettings.maxAngleBelowPlane, 180f - _cameraSettings.poleSafetyAngle);

            if (!(minVerticalAngle > maxVerticalAngle))
                return Mathf.Clamp(angle, minVerticalAngle, maxVerticalAngle);

            var mid = (minVerticalAngle + maxVerticalAngle) * 0.5f;
            minVerticalAngle = mid;
            maxVerticalAngle = mid;

            return Mathf.Clamp(angle, minVerticalAngle, maxVerticalAngle);
        }
    }
}
