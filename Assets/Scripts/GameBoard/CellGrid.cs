using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

namespace GameBoard
{
    public class CellGrid : CustomMonoBehaviour<CellGrid>, IInteractionMode
    {
        [SerializeField] private Cell prefab;
        [SerializeField] private Vector3Int gridSize;

        [SerializeField] private Vector3 ownOrigin;
        [SerializeField] private Vector3 enemyOrigin;
        
        private List<Cell> _ownCells;
        private List<Cell> _enemyCells;

        protected override void SetUp()
        {
            gridSize = new Vector3Int(
                Mathf.Max(gridSize.x, 0),
                Mathf.Max(gridSize.y, 0),
                Mathf.Max(gridSize.z, 0)
            );

            Clear(_ownCells);
            Clear(_enemyCells);
            
            _ownCells = new List<Cell>();
            _enemyCells = new List<Cell>();
            
            Create(_ownCells, ownOrigin);
            Create(_enemyCells, enemyOrigin);
        }

        private void Create(List<Cell> cells, Vector3 origin)
        { 
            for (var x = 0; x < gridSize.x; x++)
            for (var y = 0; y < gridSize.y; y++)
            for (var z = 0; z < gridSize.z; z++)
            {
                var localPos = origin + new Vector3(x, y, z);
                var cell = Instantiate(prefab, localPos, Quaternion.identity, transform);
                cell.Initialize(new Vector3Int(x, y, z));
                cells.Add(cell);
            }
        }
        
        

        private static void Clear(List<Cell> cells)
        {
            if (cells == null || cells.Count == 0)
                return;

            foreach (var cell in cells)
                cell.DestroySelf();
            cells.Clear();
        }
        
        public void Activate()
        {
            foreach (var cell in _ownCells)
                cell.Activate();
        }

        public void Deactivate()
        {
            foreach (var cell in _ownCells)
                cell.Deactivate();
        }

        public void UpdateMode()
        {
            // Пока пустой, возможно в будущем
        }

        public IReadOnlyList<Cell> GetCells() => _ownCells;
    }
}