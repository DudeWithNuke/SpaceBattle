using GameBoard;
using PlaceableObject;
using Reflex.Attributes;
using UnityEngine;

namespace PlaceableObjectManipulation
{
    public sealed class GridInteraction : MonoBehaviour
    {
        [Inject] private CellGrid _cellGrid;
        [Inject] private CursorPlane _cursorPlane;

        private PlaceableObjectShape _shape;
        private bool _usesCellOccupancy;
        private bool _isPlayerObject;

        private Vector3Int _cachedPosition;
        private Vector3Int[] _cachedOccupiedCells;
        private Vector3Int _lastHoverPosition;
        private bool _hasHoverPosition;
        private Vector3Int[] _previousHoverCells;
        private Vector3Int[] _placedOccupiedCells;

        public void Initialize(PlaceableObjectShape shape, bool usesCellOccupancy, bool isPlayerObject)
        {
            _shape = shape;
            _usesCellOccupancy = usesCellOccupancy;
            _isPlayerObject = isPlayerObject;
            _cachedOccupiedCells = null;
            _previousHoverCells = null;
            _placedOccupiedCells = null;
            _hasHoverPosition = false;
        }

        public bool CanPlace(Vector3Int position)
        {
            if (!IsWithinGridBounds(position))
                return false;

            var occupiedCells = GetOccupiedCells(position);
            return !_usesCellOccupancy || _cellGrid.CanOccupyCells(occupiedCells, _isPlayerObject);
        }

        public void ApplyPlacement(Vector3Int position)
        {
            var occupiedCells = GetOccupiedCells(position);
            _placedOccupiedCells = occupiedCells;

            if (_usesCellOccupancy)
                _cellGrid.OccupyCells(occupiedCells, _isPlayerObject);

            ClearHover();

            if (!_usesCellOccupancy)
                return;
            
            foreach (var cellPos in occupiedCells)
            {
                var cell = _cellGrid.GetCell(cellPos, _isPlayerObject);
                if (cell)
                    cell.SetSelected(true, cell.Position.y == _cursorPlane.currentLayer);
            }
        }

        public void ApplyPick(Vector3Int position)
        {
            if (_usesCellOccupancy && _placedOccupiedCells != null)
                _cellGrid.ReleaseCells(_placedOccupiedCells, _isPlayerObject);

            var occupiedCells = _placedOccupiedCells ?? GetOccupiedCells(position);
            foreach (var cellPos in occupiedCells)
            {
                var cell = _cellGrid.GetCell(cellPos, _isPlayerObject);
                if (cell && _usesCellOccupancy)
                    cell.SetSelected(false, cell.Position.y == _cursorPlane.currentLayer);
            }

            _placedOccupiedCells = null;
        }

        public void CleanupOnDestroy(Vector3Int position, bool isPlaced)
        {
            ClearHover();

            if (!isPlaced)
                return;

            var occupiedCells = _placedOccupiedCells ?? GetOccupiedCells(position);
            if (_usesCellOccupancy)
                _cellGrid.ReleaseCells(occupiedCells, _isPlayerObject);

            foreach (var cellPos in occupiedCells)
            {
                var cell = _cellGrid.GetCell(cellPos, _isPlayerObject);
                if (cell && _usesCellOccupancy)
                    cell.SetSelected(false, cell.Position.y == _cursorPlane.currentLayer);
            }
        }

        public void UpdateHover(Vector3Int position)
        {
            if (_hasHoverPosition && position == _lastHoverPosition)
                return;

            _lastHoverPosition = position;
            _hasHoverPosition = true;

            var currentOccupiedCells = GetOccupiedCells(position);
            ClearHover();

            if (!IsWithinGridBounds(position))
                return;
            
            foreach (var cellPos in currentOccupiedCells)
            {
                var cell = _cellGrid.GetCell(cellPos, _isPlayerObject);
                if (cell)
                    cell.SetPlaceableHover(true);
            }

            _previousHoverCells = currentOccupiedCells;
        }

        private void ClearHover()
        {
            if (_previousHoverCells == null)
                return;

            foreach (var cellPos in _previousHoverCells)
            {
                var cell = _cellGrid.GetCell(cellPos, _isPlayerObject);
                if (cell)
                    cell.SetPlaceableHover(false);
            }

            _previousHoverCells = null;
        }

        private bool IsWithinGridBounds(Vector3Int position)
        {
            var occupiedCells = GetOccupiedCells(position);
            foreach (var cellPos in occupiedCells)
                if (cellPos.x < 0 || cellPos.x >= _cellGrid.GridSize.x ||
                    cellPos.y < 0 || cellPos.y >= _cellGrid.GridSize.y ||
                    cellPos.z < 0 || cellPos.z >= _cellGrid.GridSize.z)
                    return false;

            return true;
        }

        private Vector3Int[] GetOccupiedCells(Vector3Int position)
        {
            if (_cachedOccupiedCells != null && position == _cachedPosition)
                return _cachedOccupiedCells;

            _cachedPosition = position;
            _cachedOccupiedCells = _shape.GetOccupiedCells(position);
            return _cachedOccupiedCells;
        }
    }
}
