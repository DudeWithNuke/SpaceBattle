using UnityEngine;

namespace GameBoard
{
    public static class CoordinateConverter
    {
        public static Vector3Int WorldToCellPosition(Vector3 worldPosition, Vector3 origin, int currentLayer)
        {
            var cellX = Mathf.RoundToInt(worldPosition.x - origin.x - 0.5f);
            var cellY = currentLayer;
            var cellZ = Mathf.RoundToInt(worldPosition.z - origin.z - 0.5f);
            
            return new Vector3Int(cellX, cellY, cellZ);
        }

        public static Vector3 CellToWorldPosition(Vector3Int cellPosition, Vector3 origin)
        {
            return origin + new Vector3(cellPosition.x + 0.5f, cellPosition.y + 0.5f, cellPosition.z + 0.5f);
        }

        public static Vector3Int WorldToCellPosition(Vector3 worldPosition, Vector3 origin, int currentLayer, float roundingOffset)
        {
            var cellX = Mathf.RoundToInt(worldPosition.x - origin.x - roundingOffset);
            var cellY = currentLayer;
            var cellZ = Mathf.RoundToInt(worldPosition.z - origin.z - roundingOffset);
            
            return new Vector3Int(cellX, cellY, cellZ);
        }

        public static Vector3 CellToWorldPosition(Vector3Int cellPosition, Vector3 origin, float centerOffset)
        {
            return origin + new Vector3(cellPosition.x + centerOffset, cellPosition.y + centerOffset, cellPosition.z + centerOffset);
        }

        public static bool IsPositionValid(Vector3Int position, Vector3Int gridSize)
        {
            return position.x >= 0 && position.x < gridSize.x &&
                   position.y >= 0 && position.y < gridSize.y &&
                   position.z >= 0 && position.z < gridSize.z;
        }
    }
}
