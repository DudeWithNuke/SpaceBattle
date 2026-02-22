using System;
using UnityEngine;

namespace CameraControls
{
    internal static class FocusPanController
    {
        public static void HandleFocusMovement(
            Transform cameraTransform,
            ref Vector3 focusPoint, ref Vector3 lastValidPlanarForward, ref Vector3 lastValidPlanarRight,
            float moveSpeed, float deltaTime,
            Func<Vector3, Vector3> clampToBounds)
        {
            var moveDirection = Vector3.zero;

            var (forward, right) = GetScreenPlanarBasis(cameraTransform, ref lastValidPlanarForward, ref lastValidPlanarRight);

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                moveDirection += forward;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                moveDirection -= forward;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                moveDirection -= right;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                moveDirection += right;

            if (moveDirection == Vector3.zero)
                return;

            moveDirection.Normalize();
            focusPoint += moveSpeed * deltaTime * moveDirection;
            focusPoint = clampToBounds(focusPoint);
        }

        private static (Vector3 forward, Vector3 right) GetScreenPlanarBasis(Transform cameraTransform, ref Vector3 lastValidPlanarForward, ref Vector3 lastValidPlanarRight)
        {
            var forward = Vector3.ProjectOnPlane(cameraTransform.up, Vector3.up);
            var right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up);

            if (forward.sqrMagnitude > 0.0001f)
            {
                forward.Normalize();
                lastValidPlanarForward = forward;
            }
            else forward = lastValidPlanarForward;


            if (right.sqrMagnitude > 0.0001f)
            {
                right.Normalize();
                lastValidPlanarRight = right;
            }
            
            right = Vector3.Cross(Vector3.up, forward).normalized;
            forward = Vector3.Cross(right, Vector3.up).normalized;

            return (forward, right);
        }
    }
}
