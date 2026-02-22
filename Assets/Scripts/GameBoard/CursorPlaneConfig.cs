using UnityEngine;

namespace GameBoard
{
    public static class CursorPlaneConfig
    {
        [Header("Scroll Control")] 
        public const float ScrollSensitivity = 0.5f;
        public const float ScrollThreshold = 0.01f;

        [Header("Layer Control")] 
        public const int DefaultStartLayer = -1;
        public const float VerticalTransitionSpeed = 35f;
    }
}
