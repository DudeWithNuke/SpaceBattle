using UnityEngine;

namespace CameraControls
{
    internal static class OrbitController
    {
        public static void HandleOrbitInput(
            ref bool isRotating,
            ref float horizontalAngle,
            ref float verticalAngle,
            float rotationSpeed,
            float maxAngleAbovePlane,
            float maxAngleBelowPlane,
            float poleSafetyAngle)
        {
            if (Input.GetMouseButtonDown(1))
            {
                isRotating = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (isRotating)
            {
                var mouseX = Input.GetAxisRaw("Mouse X");
                var mouseY = Input.GetAxisRaw("Mouse Y");

                horizontalAngle += mouseX * rotationSpeed;
                verticalAngle += -mouseY * rotationSpeed;
                verticalAngle = ClampVerticalAngle(verticalAngle, maxAngleAbovePlane, maxAngleBelowPlane, poleSafetyAngle);
            }

            if (Input.GetMouseButtonUp(1))
                StopOrbit(ref isRotating);
        }

        public static void StopOrbit(ref bool isRotating)
        {
            if (!isRotating)
                return;

            isRotating = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private static float ClampVerticalAngle(float angle, float maxAngleAbovePlane, float maxAngleBelowPlane, float poleSafetyAngle)
        {
            var minVerticalAngle = Mathf.Max(90f - maxAngleAbovePlane, poleSafetyAngle);
            var maxVerticalAngle = Mathf.Min(90f + maxAngleBelowPlane, 180f - poleSafetyAngle);

            if (!(minVerticalAngle > maxVerticalAngle))
                return Mathf.Clamp(angle, minVerticalAngle, maxVerticalAngle);
        
            var mid = (minVerticalAngle + maxVerticalAngle) * 0.5f;
            minVerticalAngle = mid;
            maxVerticalAngle = mid;

            return Mathf.Clamp(angle, minVerticalAngle, maxVerticalAngle);
        }
    }
}
