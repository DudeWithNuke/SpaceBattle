using GameBoard;

namespace CameraControls
{
    internal static class CursorPlaneVerticalController
    {
        public static void UpdateCachedY(CursorPlane cursorPlane, ref float cachedY)
        {
            cachedY = cursorPlane.transform.position.y;
        }
    }
}
