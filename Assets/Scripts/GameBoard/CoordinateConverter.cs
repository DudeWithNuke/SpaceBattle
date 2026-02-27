using UnityEngine;

namespace GameBoard
{
    public static class CoordinateConverter
    {
        public static Vector3Int WorldToCellPosition(Vector3 worldPosition, Vector3 origin, int currentLayer, float roundingOffset)
        {
            var cellX = Mathf.RoundToInt(worldPosition.x - origin.x - roundingOffset);
            var cellY = Mathf.RoundToInt(currentLayer);
            var cellZ = Mathf.RoundToInt(worldPosition.z - origin.z - roundingOffset);
            
            return new Vector3Int(cellX, cellY, cellZ);
        }

        public static Vector3 CellToWorldPosition(Vector3Int cellPosition, Vector3 origin, float centerOffset)
        {
            return origin + new Vector3(
                cellPosition.x + centerOffset, 
                cellPosition.y + centerOffset, 
                cellPosition.z + centerOffset);
        }
    }
}
