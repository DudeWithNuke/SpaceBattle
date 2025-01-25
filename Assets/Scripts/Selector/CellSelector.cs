using System.Collections.Generic;
using System.Linq;
using GameBoard;
using UnityEngine;
using Utils;

// todo 1
// Включать магнит только при полном покрытии коллайдера курсора клеткой / при наведении курсора на клетку
// Ограничивать вертикальное перемещение при оффсете по Y

// todo 2
// Механизм переключения аллокейт коллайдеров по кнопкам
// 


// передвижение объекта по курсору
// проверка пересечений
// обновление состояния клеток
namespace Selector
{
    public class CellSelector
    {
        private readonly CursorPlane _cursorPlane;

        private readonly Collider[] _cellColliders;
        private readonly PlaceableObject.PlaceableObject _placeableObject;

        private readonly Camera _camera;
        private Vector3 _targetPosition;
        private readonly List<Cell> _currentHoveredCells;

        private bool _isMoving;
        private bool _isOverlappingCell;

        private const float MoveSpeed = 30f;
        private const float Offset = 0.001f;

        public CellSelector(List<Cell> cells, CursorPlane cursorPlane, PlaceableObject.PlaceableObject placeableObject)
        {
            _camera = Camera.main;
            _currentHoveredCells = new List<Cell>();

            _cellColliders = cells
                .Select(obj => obj.GetComponent<BoxCollider>())
                .ToArray<Collider>();

            _cursorPlane = cursorPlane;
            _placeableObject = placeableObject;
        }

        public void Run()
        {
            UpdateCursor();
            UpdateHoveredCells();
        }

        private void UpdateCursor()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!_cursorPlane.Plane.Raycast(ray, out var distance))
                return;

            _targetPosition = CalculateCursorTargetPosition(ray.GetPoint(distance));
            MoveCursor();
        }

        private Vector3 CalculateCursorTargetPosition(Vector3 mousePositionOnPlane)
        {
            var xOffset = 0f;
            var yOffset = 0f;
            var zOffset = 0f;

            foreach (var collider in _placeableObject.BoxColliders)
            {
                if (MathUtils.IsGreaterThanOdd(collider.size.x))
                    xOffset = 0.5f;
                if (MathUtils.IsGreaterThanOdd(collider.size.y))
                    yOffset = 0.5f;
                if (MathUtils.IsGreaterThanOdd(collider.size.z))
                    zOffset = 0.5f;
            }

            var x = CalculatePlanarComponent(mousePositionOnPlane.x, xOffset);
            var z = CalculatePlanarComponent(mousePositionOnPlane.z, zOffset);
            return new Vector3(x, _cursorPlane.currentLayer + yOffset, z);
        }

        private float CalculatePlanarComponent(float value, float offset)
        {
            return _isOverlappingCell
                ? Mathf.RoundToInt(value) + offset
                : value;
        }

        private void MoveCursor()
        {
            if (_placeableObject.transform.position == _targetPosition)
            {
                _isMoving = false;
                return;
            }

            _isMoving = true;

            _placeableObject.transform.position = _isOverlappingCell
                ? Vector3.MoveTowards(_placeableObject.transform.position, _targetPosition, MoveSpeed * Time.deltaTime)
                : _targetPosition;

            UpdateCellOverlapping();
            
        }
        
        private void UpdateCellOverlapping()
        {
            _isOverlappingCell = false;
            var overlappingCells = _placeableObject.GetOverlappingCells(_cellColliders);
               
            foreach (var unused in overlappingCells)
            {
                _isOverlappingCell = true;
                break;
            }
            
            /*GetCellsOverlappingCursor();
            foreach (var unused in overlappingCells.Where(IsCellOnCurrentLayer))
            {
                _isOverlappingCell = true;
                break;
            }*/
        }
        
        private List<Cell> GetCellsOverlappingCursor()
        {
            var overlappingCells = new List<Cell>();
            foreach (var boxCollider in _placeableObject.BoxColliders)
            {
                var hits = Physics.OverlapBoxNonAlloc(_placeableObject.transform.position, boxCollider.bounds.extents,
                    _cellColliders, _placeableObject.transform.rotation);

                for (var i = 0; i < hits; i++)
                    if (_cellColliders[i].TryGetComponent<Cell>(out var cell) && IsCellOnCurrentLayer(cell))
                        overlappingCells.Add(cell);
            }

            return overlappingCells;
        }

        private void UpdateHoveredCells()
        {
            if (_isMoving)
                return;

            foreach (var cell in _currentHoveredCells)
                ApplyCellHoverState(cell, false);
            _currentHoveredCells.Clear();

            if (!_isOverlappingCell)
                return;

            //var overlappingCells = 
                
                
                
            /*GetCellsOverlappingCursor();
            foreach (var cell in overlappingCells.Where(IsCellOnCurrentLayer))
            {
                _currentHoveredCells.Add(cell);
                ApplyCellHoverState(cell, true);
            }*/
        }

        private static void ApplyCellHoverState(Cell cell, bool isHovered)
        {
            switch (cell.State)
            {
                case CellState.ActiveLayer or CellState.DisabledLayer when isHovered:
                    cell.UpdateState(CellState.Hovered);
                    break;
                case CellState.Selected when isHovered:
                    cell.UpdateState(CellState.HoveredSelectedTarget);
                    break;
                case CellState.Hovered when !isHovered:
                    cell.UpdateState(CellState.ActiveLayer);
                    break;
                case CellState.HoveredSelectedTarget when !isHovered:
                    cell.UpdateState(CellState.Selected);
                    break;
            }
        }
        
        private bool IsCellOnCurrentLayer(Cell cell)
        {
            return Mathf.Abs(cell.Position.y - _cursorPlane.currentLayer) < Offset;
        }
    }
}