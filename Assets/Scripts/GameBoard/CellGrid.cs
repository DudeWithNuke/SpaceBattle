using System.Collections.Generic;
using System.Linq;
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

        public Vector3 PlayerOrigin => ownOrigin;
        public Vector3 EnemyOrigin => enemyOrigin;

        public List<Cell> PlayerCells { get; private set; }

        public List<Cell> EnemyCells { get; private set; }

        private Dictionary<Vector3Int, Cell> _playerCellMap;
        private Dictionary<Vector3Int, Cell> _enemyCellMap;

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
            
            _playerCellMap = new Dictionary<Vector3Int, Cell>();
            _enemyCellMap = new Dictionary<Vector3Int, Cell>();
            
            Create(PlayerCells, ownOrigin, true, _playerCellMap);
            Create(EnemyCells, enemyOrigin, false, _enemyCellMap);
        }

        private void Create(List<Cell> cells, Vector3 origin, bool isPlayerCell, Dictionary<Vector3Int, Cell> cellMap)
        { 
            for (var x = 0; x < gridSize.x; x++)
            for (var y = 0; y < gridSize.y; y++)
            for (var z = 0; z < gridSize.z; z++)
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
        
        // Новые методы для координатной работы с клетками
        public Cell GetCell(Vector3Int position, bool isPlayerCell)
        {
            var cellMap = isPlayerCell ? _playerCellMap : _enemyCellMap;
            return cellMap.GetValueOrDefault(position);
        }

        public IEnumerable<Cell> GetCellsInPositions(IEnumerable<Vector3Int> positions, bool isPlayerCell)
        {
            var cellMap = isPlayerCell ? _playerCellMap : _enemyCellMap;
            return positions.Select(pos => cellMap.GetValueOrDefault(pos))
                           .Where(cell => cell);
        }

        public void UpdateCellsForPlaceableObject(IEnumerable<Vector3Int> positions, bool isPlayerCell, System.Action<Cell> updateAction)
        {
            var cells = GetCellsInPositions(positions, isPlayerCell);
            foreach (var cell in cells)
                updateAction(cell);
        }

        public void SetCursorHoverForCells(IEnumerable<Vector3Int> positions, bool isPlayerCell, bool isHovering)
        {
            UpdateCellsForPlaceableObject(positions, isPlayerCell, cell => cell.SetCursorHover(isHovering));
        }

        public void SetPlaceableHoverForCells(IEnumerable<Vector3Int> positions, bool isPlayerCell, bool isHovering)
        {
            UpdateCellsForPlaceableObject(positions, isPlayerCell, cell => cell.SetPlaceableHover(isHovering));
        }

        public void SetSelectedForCells(IEnumerable<Vector3Int> positions, bool isPlayerCell, bool isSelected)
        {
            UpdateCellsForPlaceableObject(positions, isPlayerCell, cell => cell.SetSelected(isSelected));
        }

        // Активация/деактивация клеток только определенного слоя
        public void SetLayerActive(int layerIndex, bool isPlayerCell, bool isActive)
        {
            var cells = isPlayerCell ? PlayerCells : EnemyCells;
            foreach (var cell in cells)
            {
                if (cell.Position.y == layerIndex)
                {
                    if (isActive)
                    {
                        cell.Activate();
                        // Выделяем активный слой белым
                        cell.SetCursorHover(true);
                    }
                    else
                    {
                        cell.Deactivate();
                        // Сбрасываем выделение
                        cell.SetCursorHover(false);
                    }
                }
                else
                {
                    cell.Deactivate();
                    // Сбрасываем выделение для неактивных слоев
                    cell.SetCursorHover(false);
                }
            }
        }
    }
}
