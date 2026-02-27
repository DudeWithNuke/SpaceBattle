using UnityEngine;

namespace PlayerCamera.Movement
{
    internal sealed class FocusPan
    {
        private readonly CameraSettings _cameraSettings;

        public FocusPan(CameraSettings cameraSettings)
        {
            _cameraSettings = cameraSettings;
        }

        public void Handle(CameraState state, Transform cameraTransform, Vector2 moveInput, float deltaTime, System.Func<Vector3, Vector3> clampToBounds)
        {
            var moveDirection = Vector3.zero;
            var (forward, right) = GetScreenPlanarBasis(state, cameraTransform);

            if (moveInput.y > 0f)
                moveDirection += forward;
            if (moveInput.y < 0f)
                moveDirection -= forward;
            if (moveInput.x < 0f)
                moveDirection -= right;
            if (moveInput.x > 0f)
                moveDirection += right;
            
            if (moveDirection == Vector3.zero)
                return;

            moveDirection.Normalize();
            state.FocusPoint += _cameraSettings.moveSpeed * deltaTime * moveDirection;
            state.FocusPoint = clampToBounds(state.FocusPoint);
        }

        private static (Vector3 forward, Vector3 right) GetScreenPlanarBasis(CameraState state, Transform cameraTransform)
        {
            var forward = Vector3.ProjectOnPlane(cameraTransform.up, Vector3.up);
            var right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up);

            if (forward.sqrMagnitude > 0.0001f)
            {
                forward.Normalize();
                state.LastValidPlanarForward = forward;
            }
            else forward = state.LastValidPlanarForward;
            
            if (right.sqrMagnitude > 0.0001f)
                right.Normalize();
            
            right = Vector3.Cross(Vector3.up, forward).normalized;
            forward = Vector3.Cross(right, Vector3.up).normalized;

            return (forward, right);
        }
    }
}
