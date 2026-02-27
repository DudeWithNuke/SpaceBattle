using System;
using GameBoard;
using Reflex.Attributes;
using UnityEngine;

namespace PlaceableObject
{
    public abstract class PlaceableObject : MonoBehaviour
    {
        private const float MaxPickedDistanceFromCursorPlaneCenter = 40f;
        private static PlaceableObject _currentlyPickedObject;

        public event Action<PlaceableObject> OnPlaced;
        public event Action<PlaceableObject> OnPicked;
        public event Action<PlaceableObject> OnDestroyed;

        private Camera _camera;

        public PlaceableObjectState State { get; private set; }
        public PlaceableObjectShape Shape { get; private set; }
        public Vector3Int CurrentPosition { get; set; }
        public bool IsPlayerObject { get; set; }
        public static PlaceableObject CurrentPickedObject => _currentlyPickedObject;

        [Inject] private CellGrid _cellGrid;
        [Inject] private CursorPlane _cursorPlane;

        private Vector3Int[] _previousOccupiedCells;
        private Vector3Int[] _placedOccupiedCells;
        private bool _isHoveringCells;
        private Vector3 _previousFramePosition;
        private bool _isCurrentlyMoving;

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
            _currentlyPickedObject = this;
            _isHoveringCells = false;

            var startPos = transform.position;
            var distanceToPlayer = Vector3.Distance(startPos, _cellGrid.PlayerOrigin);
            var distanceToEnemy = Vector3.Distance(startPos, _cellGrid.EnemyOrigin);
            IsPlayerObject = distanceToPlayer <= distanceToEnemy;

            CurrentPosition = WorldToCellPosition(startPos);
            _targetWorldPosition = CellToWorldPosition(CurrentPosition);
            _previousFramePosition = transform.position;
            _isCurrentlyMoving = false;
        }

        private void LateUpdate()
        {
            if (State != PlaceableObjectState.Picked)
                return;

            UpdatePosition();
            UpdateCellHover();
            UpdateMovingState();
        }

        private void UpdatePosition()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            var groundLayerMask = (1 << LayerMask.NameToLayer(PlaceableObjectConfig.GroundLayerName)) |
                                  (1 << LayerMask.NameToLayer(PlaceableObjectConfig.DefaultLayerName));
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayerMask))
            {
                var cursorCellPosition = WorldToCellPosition(hit.point);
                var newPosition = _cursorPlane.IsTransitioning
                    ? new Vector3Int(CurrentPosition.x, _cursorPlane.currentLayer, CurrentPosition.z)
                    : new Vector3Int(cursorCellPosition.x, _cursorPlane.currentLayer, cursorCellPosition.z);

                newPosition = ClampPickedPositionToCursorPlaneRange(newPosition);

                if (newPosition != CurrentPosition)
                {
                    CurrentPosition = newPosition;
                    _targetWorldPosition = CellToWorldPosition(CurrentPosition);
                }
            }

            if (_cursorPlane.IsTransitioning)
            {
                CurrentPosition = new Vector3Int(CurrentPosition.x, _cursorPlane.currentLayer, CurrentPosition.z);
                var syncedPosition = CellToWorldPosition(CurrentPosition);
                _targetWorldPosition = new Vector3(transform.position.x, syncedPosition.y, transform.position.z);
                transform.position = new Vector3(transform.position.x, syncedPosition.y, transform.position.z);
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

            if (_previousOccupiedCells != null)
            {
                foreach (var cellPos in _previousOccupiedCells)
                {
                    var cell = _cellGrid.GetCell(cellPos, IsPlayerObject);
                    if (cell)
                        cell.SetPlaceableHover(false);
                }
            }

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
            {
                if (cellPos.x < 0 || cellPos.x >= _cellGrid.GridSize.x ||
                    cellPos.y < 0 || cellPos.y >= _cellGrid.GridSize.y ||
                    cellPos.z < 0 || cellPos.z >= _cellGrid.GridSize.z)
                    return false;
            }

            return true;
        }

        private void Place()
        {
            var occupiedCells = Shape.GetOccupiedCells(CurrentPosition);
            if (!CanBePlaced(occupiedCells))
                return;

            State = PlaceableObjectState.Placed;
            if (_currentlyPickedObject == this)
                _currentlyPickedObject = null;

            _placedOccupiedCells = occupiedCells;
            _cellGrid.OccupyCells(occupiedCells, IsPlayerObject);

            if (_previousOccupiedCells != null)
            {
                foreach (var cellPos in _previousOccupiedCells)
                {
                    var cell = _cellGrid.GetCell(cellPos, IsPlayerObject);
                    if (cell)
                        cell.SetPlaceableHover(false);
                }
            }

            foreach (var cellPos in occupiedCells)
            {
                var cell = _cellGrid.GetCell(cellPos, IsPlayerObject);
                if (cell)
                    cell.SetSelected(true);
            }

            OnPlaced?.Invoke(this);
        }

        public bool TryPlaceFromInput()
        {
            if (State != PlaceableObjectState.Picked)
                return false;

            if (_isCurrentlyMoving || _cursorPlane.IsTransitioning)
                return false;

            if (Vector3.Distance(transform.position, _targetWorldPosition) > PlaceableObjectConfig.PositionThreshold)
                return false;

            Place();
            return true;
        }

        public bool TryPickFromRay(Ray ray)
        {
            if (State != PlaceableObjectState.Placed)
                return false;

            if (_currentlyPickedObject && _currentlyPickedObject != this)
                return false;

            var hits = Physics.RaycastAll(ray, Mathf.Infinity);
            foreach (var raycastHit in hits)
            {
                var pickedObject = raycastHit.collider.GetComponentInParent<PlaceableObject>();
                if (pickedObject && pickedObject == this)
                {
                    Pick();
                    return true;
                }
            }

            return false;
        }

        public bool TryPick()
        {
            if (State != PlaceableObjectState.Placed)
                return false;

            Pick();
            return true;
        }

        private void Pick()
        {
            State = PlaceableObjectState.Picked;
            _currentlyPickedObject = this;

            if (_placedOccupiedCells != null)
                _cellGrid.ReleaseCells(_placedOccupiedCells, IsPlayerObject);

            var occupiedCells = _placedOccupiedCells ?? Shape.GetOccupiedCells(CurrentPosition);
            foreach (var cellPos in occupiedCells)
            {
                var cell = _cellGrid.GetCell(cellPos, IsPlayerObject);
                if (cell)
                    cell.SetSelected(false);
            }

            _placedOccupiedCells = null;
            OnPicked?.Invoke(this);
        }

        private void OnDestroy()
        {
            if (_previousOccupiedCells != null)
            {
                foreach (var cellPos in _previousOccupiedCells)
                {
                    var cell = _cellGrid.GetCell(cellPos, IsPlayerObject);
                    if (cell != null)
                        cell.SetPlaceableHover(false);
                }
            }

            if (State == PlaceableObjectState.Placed)
            {
                var occupiedCells = _placedOccupiedCells ?? Shape.GetOccupiedCells(CurrentPosition);
                _cellGrid.ReleaseCells(occupiedCells, IsPlayerObject);

                foreach (var cellPos in occupiedCells)
                {
                    var cell = _cellGrid.GetCell(cellPos, IsPlayerObject);
                    if (cell != null)
                        cell.SetSelected(false);
                }
            }

            if (_currentlyPickedObject == this)
                _currentlyPickedObject = null;

            OnDestroyed?.Invoke(this);
        }

        private bool CanBePlaced(Vector3Int[] occupiedCells)
        {
            return IsWithinGridBounds() && _cellGrid.CanOccupyCells(occupiedCells, IsPlayerObject);
        }

        private void UpdateMovingState()
        {
            _isCurrentlyMoving = Vector3.Distance(_previousFramePosition, transform.position) > PlaceableObjectConfig.PositionThreshold;
            _previousFramePosition = transform.position;
        }

        protected abstract void DefineShape(PlaceableObjectShape shape);
    }
}
