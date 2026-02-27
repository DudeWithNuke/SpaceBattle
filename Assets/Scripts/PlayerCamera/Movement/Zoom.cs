using UnityEngine;

namespace PlayerCamera.Movement
{
    internal class Zoom
    {
        private readonly CameraSettings _cameraSettings;

        public Zoom(CameraSettings cameraSettings)
        {
            _cameraSettings = cameraSettings;
        }

        public void Handle(CameraState state, float zoomDelta, float deltaTime)
        {
            if (Mathf.Abs(zoomDelta) < 0.001f)
                return;

            state.ZoomDistance -= _cameraSettings.zoomSpeed * zoomDelta * deltaTime;
            state.ZoomDistance = Mathf.Clamp(state.ZoomDistance, _cameraSettings.minZoomDistance, _cameraSettings.maxZoomDistance);
        }
    }
}
