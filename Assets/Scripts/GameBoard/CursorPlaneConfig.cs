using UnityEngine;

namespace GameBoard
{
    public static class CursorPlaneConfig
    {
        [Header("Camera Movement")]
        public static float CameraFollowSpeed = 15f;
        public static float CameraHeightOffset = 5f;
        public static float CameraStopThreshold = 0.1f;
        
        [Header("Scroll Control")]
        public static float ScrollSensitivity = 0.5f;
        public static float ScrollThreshold = 0.01f;
        
        [Header("Layer Control")]
        public static int DefaultStartLayer = -1; // -1 = middle
    }
}
