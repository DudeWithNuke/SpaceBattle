using GameBoard;
using UnityEngine;

namespace PlaceableObjectManipulation
{
    [System.Serializable]
    public class SpawnPositionResolver
    {
        public Vector3 ResolveSpawnPosition(CursorPlane cursorPlane, Camera targetCamera)
        {
            var ray = targetCamera.ScreenPointToRay(Input.mousePosition);
            return cursorPlane.Plane.Raycast(ray, out var distance)
                ? ray.GetPoint(distance)
                : new Vector3(0f, cursorPlane.currentLayer, 0f);
        }
    }
}
