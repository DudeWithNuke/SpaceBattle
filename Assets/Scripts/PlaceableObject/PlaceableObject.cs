using System;
using GameBoard;
using Reflex.Attributes;
using UnityEngine;

namespace PlaceableObject
{
    public abstract class PlaceableObject : MonoBehaviour
    {
        private const float MaxPickedDistanceFromCursorPlaneCenter = 40f;

        public event Action<PlaceableObject> OnPlaced;
        public event Action<PlaceableObject> OnPicked;

        private Camera _camera;

        public PlaceableObjectState State { get; private set; }
        public PlaceableObjectShape Shape { get; private set; }
        public Vector3Int CurrentPosition { get; set; }
        public bool IsPlayerObject { get; set; }

        [Inject] private CellGrid _cellGrid;
        [Inject] private CursorPlane _cursorPlane;

        private Vector3Int[] _previousOccupiedCells;
        private bool _isHoveringCells;

        private Vector3 _targetWorldPosition;

        private void Awake()
        {
            Shape = ScriptableObject.CreateInstance<PlaceableObjectShape>();
            DefineShape(Shape);
        }

        private void Start()
        {
            _camera = Camera.main;
            State = PlaceableObjectState.Picked;
            _isHoveringCells = false;

            // Определяем принадлежность объекта на основе начальной позиции
            var startPos = transform.position;
            var distanceToPlayer = Vector3.Distance(startPos, _cellGrid.PlayerOrigin);
            var distanceToEnemy = Vector3.Distance(startPos, _cellGrid.EnemyOrigin);
            IsPlayerObject = distanceToPlayer <= distanceToEnemy;

            // Устанавливаем начальную позицию в координатах клетки
            CurrentPosition = WorldToCellPosition(startPos);
            _targetWorldPosition = CellToWorldPosition(CurrentPosition);
        }

        private void Update()
        {
            if (!Input.GetMouseButtonUp(0))
                return;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            var placeableObjectLayerMask = 1 << LayerMask.NameToLayer(PlaceableObjectConfig.PlaceableObjectLayerName);
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, placeableObjectLayerMask)
                || hit.collider.gameObject != gameObject)
                return;

            switch (State)
            {
                case PlaceableObjectState.Picked:
                    Place();
                    break;
                case PlaceableObjectState.Placed:
                    Pick();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LateUpdate()
        {
            if (State != PlaceableObjectState.Picked)
                return;

            UpdatePosition();
            UpdateCellHover();
        }

        private void UpdatePosition()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            var groundLayerMask = (1 << LayerMask.NameToLayer(PlaceableObjectConfig.GroundLayerName)) |
                                  (1 << LayerMask.NameToLayer(PlaceableObjectConfig.DefaultLayerName));
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayerMask))
            {
                var newPosition = WorldToCellPosition(hit.point);
                newPosition = _cursorPlane.IsTransitioning 
                    ? new Vector3Int(CurrentPosition.x, _cursorPlane.currentLayer, CurrentPosition.z)
                    : new Vector3Int(newPosition.x, _cursorPlane.currentLayer, newPosition.z);

                newPosition = ClampPickedPositionToCursorPlaneRange(newPosition);

                if (newPosition != CurrentPosition)
                {
                    CurrentPosition = newPosition;
                    _targetWorldPosition = CellToWorldPosition(CurrentPosition);
                }
            }

            if (_cursorPlane.IsTransitioning)
            {
                var origin = IsPlayerObject ? _cellGrid.PlayerOrigin : _cellGrid.EnemyOrigin;
                var syncedY = origin.y + _cursorPlane.transform.position.y + PlaceableObjectConfig.CellCenterOffset;
                _targetWorldPosition = new Vector3(
                    transform.position.x,
                    syncedY,
                    transform.position.z
                );
                transform.position = new Vector3(transform.position.x, syncedY, transform.position.z);
            }

            _targetWorldPosition = ClampWorldPositionToCursorPlaneRange(_targetWorldPosition);

            if (Vector3.Distance(transform.position, _targetWorldPosition) > PlaceableObjectConfig.PositionThreshold)
                transform.position = Vector3.MoveTowards(transform.position, _targetWorldPosition,
                    PlaceableObjectConfig.MoveSpeed * Time.deltaTime);
        }

        private Vector3Int ClampPickedPositionToCursorPlaneRange(Vector3Int cellPosition)
        {
            var worldPosition = CellToWorldPosition(cellPosition);
            var clampedWorldPosition = ClampWorldPositionToCursorPlaneRange(worldPosition);
            return WorldToCellPosition(clampedWorldPosition);
        }

        private Vector3 ClampWorldPositionToCursorPlaneRange(Vector3 worldPosition)
        {
            var center = _cursorPlane.transform.position;
            return new Vector3(
                Mathf.Clamp(worldPosition.x, center.x - MaxPickedDistanceFromCursorPlaneCenter,
                    center.x + MaxPickedDistanceFromCursorPlaneCenter),
                worldPosition.y,
                Mathf.Clamp(worldPosition.z, center.z - MaxPickedDistanceFromCursorPlaneCenter,
                    center.z + MaxPickedDistanceFromCursorPlaneCenter)
            );
        }

        private Vector3Int WorldToCellPosition(Vector3 worldPosition)
        {
            var origin = IsPlayerObject ? _cellGrid.PlayerOrigin : _cellGrid.EnemyOrigin;
            return CoordinateConverter.WorldToCellPosition(worldPosition, origin, _cursorPlane.currentLayer,
                PlaceableObjectConfig.CoordinateRoundingOffset);
        }

        private Vector3 CellToWorldPosition(Vector3Int cellPosition)
        {
            var origin = IsPlayerObject ? _cellGrid.PlayerOrigin : _cellGrid.EnemyOrigin;
            return CoordinateConverter.CellToWorldPosition(cellPosition, origin,
                PlaceableObjectConfig.CellCenterOffset);
        }

        private void UpdateCellHover()
        {
            var currentOccupiedCells = Shape.GetOccupiedCells(CurrentPosition);

            // Сбрасываем предыдущие клетки
            if (_previousOccupiedCells != null)
            {
                foreach (var cellPos in _previousOccupiedCells)
                {
                    var cell = _cellGrid.GetCell(cellPos, IsPlayerObject);
                    if (cell)
                        cell.SetPlaceableHover(false);
                }
            }

            // Устанавливаем новые клетки
            if (IsWithinGridBounds())
            {
                foreach (var cellPos in currentOccupiedCells)
                {
                    var cell = _cellGrid.GetCell(cellPos, IsPlayerObject);
                    if (cell)
                        cell.SetPlaceableHover(true);
                }

                _isHoveringCells = true;
            }
            else if (_isHoveringCells)
            {
                foreach (var cellPos in _previousOccupiedCells)
                {
                    var cell = _cellGrid.GetCell(cellPos, IsPlayerObject);
                    if (cell)
                        cell.SetPlaceableHover(false);
                }

                _isHoveringCells = false;
            }

            _previousOccupiedCells = currentOccupiedCells;
        }

        private bool IsWithinGridBounds()
        {
            var occupiedCells = Shape.GetOccupiedCells(CurrentPosition);

            foreach (var cellPos in occupiedCells)
                if (cellPos.x < 0 || cellPos.x >= _cellGrid.GridSize.x ||
                    cellPos.y < 0 || cellPos.y >= _cellGrid.GridSize.y ||
                    cellPos.z < 0 || cellPos.z >= _cellGrid.GridSize.z)
                    return false;

            return true;
        }

        private void Place()
        {
            if (!IsWithinGridBounds())
                return;

            State = PlaceableObjectState.Placed;

            // Сбрасываем hover состояние перед размещением
            if (_previousOccupiedCells != null)
            {
                foreach (var cellPos in _previousOccupiedCells)
                {
                    var cell = _cellGrid.GetCell(cellPos, IsPlayerObject);
                    if (cell)
                        cell.SetPlaceableHover(false);
                }
            }

            // Устанавливаем выбранное состояние для занятых клеток
            var occupiedCells = Shape.GetOccupiedCells(CurrentPosition);
            foreach (var cellPos in occupiedCells)
            {
                var cell = _cellGrid.GetCell(cellPos, IsPlayerObject);
                if (cell)
                    cell.SetSelected(true);
            }

            OnPlaced?.Invoke(this);
        }

        private void Pick()
        {
            State = PlaceableObjectState.Picked;

            var occupiedCells = Shape.GetOccupiedCells(CurrentPosition);
            foreach (var cellPos in occupiedCells)
            {
                var cell = _cellGrid.GetCell(cellPos, IsPlayerObject);
                if (cell)
                    cell.SetSelected(false);
            }

            OnPicked?.Invoke(this);
        }

        private void OnDestroy()
        {
            // Очищаем состояние клеток при уничтожении объекта
            if (_previousOccupiedCells != null)
                foreach (var cellPos in _previousOccupiedCells)
                {
                    var cell = _cellGrid.GetCell(cellPos, IsPlayerObject);
                    if (cell != null)
                        cell.SetPlaceableHover(false);
                }


            if (State != PlaceableObjectState.Placed)
                return;

            var occupiedCells = Shape.GetOccupiedCells(CurrentPosition);
            foreach (var cellPos in occupiedCells)
            {
                var cell = _cellGrid.GetCell(cellPos, IsPlayerObject);
                if (cell != null)
                    cell.SetSelected(false);
            }
        }

        protected abstract void DefineShape(PlaceableObjectShape shape);
    }
}