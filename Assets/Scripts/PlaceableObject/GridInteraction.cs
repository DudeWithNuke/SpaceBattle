using GameBoard;
using Reflex.Attributes;
using UnityEngine;

namespace PlaceableObject
{
    public sealed class GridInteraction : MonoBehaviour
    {
        [Inject] private CellGrid _cellGrid;
        [Inject] private CursorPlane _cursorPlane;

        private Shape _shape;
        private bool _usesCellOccupancy;
        private bool _isPlayerObject;

        private Vector3Int _cachedPosition;
        private Vector3Int[] _cachedOccupiedCells;
        private Vector3Int _lastHoverPosition;
        private bool _hasHoverPosition;
        private Vector3Int[] _previousHoverCells;
        private Vector3Int[] _placedOccupiedCells;

        public void Initialize(Shape shape, bool usesCellOccupancy, bool isPlayerObject)
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
            ReleaseOccupiedCellsAndClearSelection(position);
            _placedOccupiedCells = null;
        }

        public void CleanupOnDestroy(Vector3Int position, bool isPlaced)
        {
            ClearHover();

            if (!isPlaced)
                return;

            ReleaseOccupiedCellsAndClearSelection(position);
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
            return CoordinateUtility.AreWithinGridBounds(_cellGrid, occupiedCells);
        }

        private Vector3Int[] GetOccupiedCells(Vector3Int position)
        {
            if (_cachedOccupiedCells != null && position == _cachedPosition)
                return _cachedOccupiedCells;

            _cachedPosition = position;
            _cachedOccupiedCells = _shape.GetOccupiedCells(position);
            return _cachedOccupiedCells;
        }

        private void ReleaseOccupiedCellsAndClearSelection(Vector3Int position)
        {
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
    }
}
