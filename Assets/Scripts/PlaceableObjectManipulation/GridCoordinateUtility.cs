using GameBoard;
using UnityEngine;

namespace PlaceableObjectManipulation
{
    public static class GridCoordinateUtility
    {
        public static Vector3 GetOrigin(CellGrid cellGrid, bool isPlayerObject)
        {
            return isPlayerObject ? cellGrid.OwnOrigin : cellGrid.EnemyOrigin;
        }

        public static Vector3Int WorldToCellPosition(
            CellGrid cellGrid,
            bool isPlayerObject,
            Vector3 worldPosition,
            int currentLayer,
            float roundingOffset)
        {
            var origin = GetOrigin(cellGrid, isPlayerObject);
            return CoordinateConverter.WorldToCellPosition(worldPosition, origin, currentLayer, roundingOffset);
        }

        public static Vector3 CellToWorldPosition(
            CellGrid cellGrid,
            bool isPlayerObject,
            Vector3Int cellPosition,
            float centerOffset)
        {
            var origin = GetOrigin(cellGrid, isPlayerObject);
            return CoordinateConverter.CellToWorldPosition(cellPosition, origin, centerOffset);
        }

        public static bool IsWithinGridBounds(CellGrid cellGrid, Vector3Int cellPosition)
        {
            return cellPosition.x >= 0 && cellPosition.x < cellGrid.GridSize.x &&
                   cellPosition.y >= 0 && cellPosition.y < cellGrid.GridSize.y &&
                   cellPosition.z >= 0 && cellPosition.z < cellGrid.GridSize.z;
        }

        public static bool AreWithinGridBounds(CellGrid cellGrid, Vector3Int[] cellPositions)
        {
            foreach (var cellPosition in cellPositions)
                if (!IsWithinGridBounds(cellGrid, cellPosition))
                    return false;

            return true;
        }
    }
}
