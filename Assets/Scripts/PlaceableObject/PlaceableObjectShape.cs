using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlaceableObject
{
    [System.Serializable]
    public struct CellOffset
    {
        public Vector3Int position;
        public CellType type;
        public CellIntegrity integrity;
        
        public CellOffset(Vector3Int position, CellType type = CellType.Normal, CellIntegrity integrity = CellIntegrity.Intact)
        {
            this.position = position;
            this.type = type;
            this.integrity = integrity;
        }
    }

    [CreateAssetMenu(fileName = "PlaceableObjectShape", menuName = "Game/PlaceableObject Shape")]
    public class PlaceableObjectShape : ScriptableObject
    {
        public Vector3Int rootPoint = Vector3Int.zero;
        public List<CellOffset> occupiedOffsets = new();
        
        private BoundsInt? _cachedBounds;

        private void OnValidate()
        { 
            var uniqueOffsets = new Dictionary<Vector3Int, CellOffset>();
            
            foreach (var offset in occupiedOffsets)
            {
                if (uniqueOffsets.TryAdd(offset.position, offset)) 
                    continue;
                
                var existing = uniqueOffsets[offset.position];
                if (offset.type == CellType.Core && existing.type == CellType.Normal)
                    uniqueOffsets[offset.position] = offset;
            }
            
            occupiedOffsets = uniqueOffsets.Values.ToList();
            _cachedBounds = null;
        }
        
        public Vector3Int[] GetOccupiedCells(Vector3Int originPosition)
        {
            var cells = new Vector3Int[occupiedOffsets.Count];
            for (var i = 0; i < occupiedOffsets.Count; i++)
                cells[i] = originPosition + occupiedOffsets[i].position;

            return cells;
        }
        
        public CellOffset[] GetOccupiedCellsWithTypes(Vector3Int originPosition)
        {
            var cells = new CellOffset[occupiedOffsets.Count];
            for (var i = 0; i < occupiedOffsets.Count; i++)
            {
                var offset = occupiedOffsets[i];
                cells[i] = new CellOffset(originPosition + offset.position, offset.type);
            }

            return cells;
        }

        public BoundsInt GetBounds()
        {
            if (_cachedBounds.HasValue)
                return _cachedBounds.Value;

            if (occupiedOffsets.Count == 0)
                return new BoundsInt(rootPoint, Vector3Int.one);

            var min = occupiedOffsets[0].position;
            var max = occupiedOffsets[0].position;

            foreach (var offset in occupiedOffsets)
            {
                min = Vector3Int.Min(min, offset.position);
                max = Vector3Int.Max(max, offset.position);
            }

            _cachedBounds = new BoundsInt(min, max - min + Vector3Int.one);
            return _cachedBounds.Value;
        }
        
        private void AddCell(Vector3Int offset, CellType type)
        {
            var cellOffset = new CellOffset(offset, type); 
            
            var existingIndex = occupiedOffsets.FindIndex(o => o.position == offset);
            if (existingIndex >= 0)
            {
                var existing = occupiedOffsets[existingIndex];
                if (type != CellType.Core || existing.type != CellType.Normal) 
                    return;
                
                occupiedOffsets[existingIndex] = cellOffset;
            }
            else occupiedOffsets.Add(cellOffset);

            _cachedBounds = null;
        }
        
        public void AddCell(int x, int y, int z)
        {
            AddCell(new Vector3Int(x, y, z), CellType.Normal);
        }
        
        public void AddCoreCell(int x, int y, int z)
        {
            AddCell(new Vector3Int(x, y, z), CellType.Core);
        }
        
        public void AddRectangle(int width, int height, int depth = 1)
        {
            occupiedOffsets.Clear();

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            for (var z = 0; z < depth; z++)
                AddCell(x, y, z);
        }
        
        public CellIntegrity GetCellIntegrity(Vector3Int position)
        {
            var cellOffset = occupiedOffsets.Find(o => o.position == position);
            return cellOffset.integrity;
        }
        
        public void SetCellIntegrity(Vector3Int position, CellIntegrity integrity)
        {
            var index = occupiedOffsets.FindIndex(o => o.position == position);
            if (index < 0) 
                return;
            
            var cellOffset = occupiedOffsets[index];
            cellOffset.integrity = integrity;
            occupiedOffsets[index] = cellOffset;
        }
        
        public int GetIntactCellsCount()
        {
            return occupiedOffsets.Count(o => o.integrity == CellIntegrity.Intact);
        }

        public int GetDestroyedCellsCount()
        {
            return occupiedOffsets.Count(o => o.integrity == CellIntegrity.Destroyed);
        }
    }
    
    public enum CellType
    {
        Normal = 0,
        Core = 1
    }

    public enum CellIntegrity
    {
        Intact = 0,
        Destroyed = 1
    }
}