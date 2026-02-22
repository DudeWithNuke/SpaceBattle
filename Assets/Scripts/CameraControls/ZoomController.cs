using UnityEngine;

namespace CameraControls
{
    internal static class ZoomController
    {
        public static void HandleKeyboardZoom(ref float currentZoomDistance,
            float zoomSpeed, float keyboardMultiplier, float minZoomDistance, float maxZoomDistance)
        {
            var zoomDelta = 0f;

            if (Input.GetKey(KeyCode.Equals))
                zoomDelta += 1f;

            if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
                zoomDelta -= 1f;

            if (Mathf.Abs(zoomDelta) < 0.001f)
                return;

            currentZoomDistance -= zoomDelta * zoomSpeed * Time.deltaTime * keyboardMultiplier;
            currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoomDistance, maxZoomDistance);
        }
    }
}
