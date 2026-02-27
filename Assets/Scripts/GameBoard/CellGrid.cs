using System.Collections.Generic;
using UnityEngine;

namespace GameBoard
{
    public class CellGrid : MonoBehaviour
    {
        [SerializeField] private Cell prefab;

        [field: SerializeField] public Vector3 PlayerOrigin { get; set; }
        [field: SerializeField] public Vector3 EnemyOrigin { get; set; }
        [field: SerializeField] public Vector3Int GridSize { get; set; }

        public List<Cell> PlayerCells { get; private set; }
        private Dictionary<Vector3Int, Cell> _playerCellMap;
        private HashSet<Vector3Int> _playerOccupiedCells;

        public List<Cell> EnemyCells { get; private set; }
        private Dictionary<Vector3Int, Cell> _enemyCellMap;
        private HashSet<Vector3Int> _enemyOccupiedCells;

        private void Awake()
        {
            GridSize = new Vector3Int(
                Mathf.Max(GridSize.x, 0),
                Mathf.Max(GridSize.y, 0),
                Mathf.Max(GridSize.z, 0)
            );

            Clear(PlayerCells);
            PlayerCells = new List<Cell>();
            _playerCellMap = new Dictionary<Vector3Int, Cell>();
            _playerOccupiedCells = new HashSet<Vector3Int>();
            Create(PlayerCells, PlayerOrigin, true, _playerCellMap);

            Clear(EnemyCells);
            EnemyCells = new List<Cell>();
            _enemyCellMap = new Dictionary<Vector3Int, Cell>();
            _enemyOccupiedCells = new HashSet<Vector3Int>();
            Create(EnemyCells, EnemyOrigin, false, _enemyCellMap);
        }

        private void Create(List<Cell> cells, Vector3 origin, bool isPlayerCell, Dictionary<Vector3Int, Cell> cellMap)
        {
            for (var x = 0; x < GridSize.x; x++)
            for (var y = 0; y < GridSize.y; y++)
            for (var z = 0; z < GridSize.z; z++)
            {
                var localPos = origin + new Vector3(x, y, z);
                var cell = Instantiate(prefab, localPos, Quaternion.identity, transform);
                var position = new Vector3Int(x, y, z);
                cell.Initialize(position, isPlayerCell);
                cells.Add(cell);
                cellMap[position] = cell;
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

        public Cell GetCell(Vector3Int position, bool isPlayerCell)
        {
            var cellMap = isPlayerCell ? _playerCellMap : _enemyCellMap;
            return cellMap.GetValueOrDefault(position);
        }

        public bool CanOccupyCells(IEnumerable<Vector3Int> positions, bool isPlayerCell)
        {
            var occupiedCells = isPlayerCell ? _playerOccupiedCells : _enemyOccupiedCells;
            foreach (var position in positions)
                if (occupiedCells.Contains(position))
                    return false;

            return true;
        }

        public void OccupyCells(IEnumerable<Vector3Int> positions, bool isPlayerCell)
        {
            var occupiedCells = isPlayerCell ? _playerOccupiedCells : _enemyOccupiedCells;
            foreach (var position in positions)
                occupiedCells.Add(position);
        }

        public void ReleaseCells(IEnumerable<Vector3Int> positions, bool isPlayerCell)
        {
            var occupiedCells = isPlayerCell ? _playerOccupiedCells : _enemyOccupiedCells;
            foreach (var position in positions)
                occupiedCells.Remove(position);
        }
    }
}