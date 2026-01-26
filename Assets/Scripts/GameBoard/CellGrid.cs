using System.Collections.Generic;
using UnityEngine;

namespace GameBoard
{
    public class CellGrid : MonoBehaviour
    {
        [SerializeField] private Cell prefab;
        [SerializeField] private Vector3Int gridSize;
        public Vector3Int GridSize => gridSize;

        [SerializeField] private Vector3 ownOrigin;
        [SerializeField] private Vector3 enemyOrigin;

        public List<Cell> PlayerCells { get; private set; }

        public List<Cell> EnemyCells { get; private set; }

        private void Awake()
        {
            gridSize = new Vector3Int(
                Mathf.Max(gridSize.x, 0),
                Mathf.Max(gridSize.y, 0),
                Mathf.Max(gridSize.z, 0)
            );

            Clear(PlayerCells);
            Clear(EnemyCells);
            
            PlayerCells = new List<Cell>();
            EnemyCells = new List<Cell>();
            
            Create(PlayerCells, ownOrigin, true);
            Create(EnemyCells, enemyOrigin, false);
        }

        private void Create(List<Cell> cells, Vector3 origin, bool isPlayerCell)
        { 
            for (var x = 0; x < gridSize.x; x++)
            for (var y = 0; y < gridSize.y; y++)
            for (var z = 0; z < gridSize.z; z++)
            {
                var localPos = origin + new Vector3(x, y, z);
                var cell = Instantiate(prefab, localPos, Quaternion.identity, transform);
                cell.Initialize(new Vector3Int(x, y, z), isPlayerCell);
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
            foreach (var cell in PlayerCells)
                cell.Activate();
        }

        public void Deactivate()
        {
            foreach (var cell in PlayerCells)
                cell.Deactivate();
        }

        public void UpdateMode()
        {
            // Пока пустой, возможно в будущем
        }
    }
}