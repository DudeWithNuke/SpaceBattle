using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "Settings/Camera Settings")]
    public sealed class CameraSettings : ScriptableObject
    {
        [Header("Movement")]
        public float moveSpeed = 10f;

        [Header("Zoom")]
        public float zoomSpeed = 15f;
        public float minZoomDistance = 5f;
        public float maxZoomDistance = 50f;

        [Header("Orbit")]
        public float rotationSpeed = 2f;
        [Range(0f, 90f)] public float maxAngleAbovePlane = 90f;
        [Range(0f, 90f)] public float maxAngleBelowPlane = 90f;
        [Range(0.1f, 10f)] public float poleSafetyAngle = 1f;

        [Header("Battlefield Switching")]
        public float switchFocusSpeed = 500f;
        
        [Header("Bounds")]
        public float boundaryMargin = 1f;
    }
}
