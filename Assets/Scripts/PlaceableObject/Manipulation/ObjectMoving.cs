using GameBoard;
using Reflex.Attributes;
using ScriptableObjects;
using UnityEngine;

namespace PlaceableObject.Manipulation
{
    public class ObjectMoving : MonoBehaviour
    {
        [Inject] private ObjectSelection _objectSelection;
        [Inject] private CellGrid _cellGrid;
        [Inject] private CursorPlane _cursorPlane;
        
        [SerializeField] private PlaceableObjectSettings placeableObjectSettings;

        public bool IsMoving { get; private set; }

        private Camera _camera;
        private PlaceableObject _placeableObject;
        private Vector3 _previousPosition;
        private Vector3 _targetWorldPosition;
        private Vector3 _lastCursorWorldPoint;
        private bool _hasLastCursorWorldPoint;
        private float _cursorWorldSpeed;
        private float _currentMoveSpeed;
        private int _groundLayerMask;
        private bool _isGroundLayerMaskCached;
        public bool HasValidTarget { get; private set; }

        private void Awake()
        {
            if (_objectSelection != null)
                _objectSelection.OnStateChanged += HandleSelectionChanged;
        }

        private void Start()
        {
            _camera = Camera.main;
        }

        private void OnDestroy()
        {
            if (_objectSelection != null)
                _objectSelection.OnStateChanged -= HandleSelectionChanged;
        }

        private void HandleSelectionChanged(PlaceableObject placeableObject)
        {
            _placeableObject = placeableObject;
            _previousPosition = placeableObject ? placeableObject.transform.position : Vector3.zero;
            _targetWorldPosition = placeableObject ? CellToWorldPosition(placeableObject.CurrentPosition) : Vector3.zero;
            _lastCursorWorldPoint = Vector3.zero;
            _hasLastCursorWorldPoint = false;
            _cursorWorldSpeed = 0f;
            _currentMoveSpeed = placeableObjectSettings.moveSpeed;
            HasValidTarget = false;
            IsMoving = false;
        }

        private void Update()
        {
            if (!_placeableObject || _placeableObject.State == PlaceableObjectState.Placed)
            {
                IsMoving = false;
                HasValidTarget = false;
                return;
            }

            UpdatePosition();
            UpdateMovingState();
        }

        public bool IsAtTargetPosition()
        {
            if (!_placeableObject)
                return false;

            return Vector3.Distance(_placeableObject.transform.position, _targetWorldPosition) <= placeableObjectSettings.positionThreshold;
        }

        private void UpdatePosition()
        {
            if (TryGetCursorHitPoint(out var hitPoint))
            {
                UpdateCursorWorldSpeed(hitPoint);
                UpdateTargetFromRaycast(hitPoint);
            }
            else
            {
                _hasLastCursorWorldPoint = false;
                _cursorWorldSpeed = 0f;
                HasValidTarget = false;
            }

            if (_cursorPlane.IsTransitioning)
                SyncWithCursorPlaneTransition();

            ClampTargetWorldPosition();
            ApplyMovementTowardsTarget();
        }

        private void UpdateCursorWorldSpeed(Vector3 hitPoint)
        {
            if (!_hasLastCursorWorldPoint)
            {
                _lastCursorWorldPoint = hitPoint;
                _hasLastCursorWorldPoint = true;
                _cursorWorldSpeed = 0f;
                return;
            }

            var deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
            _cursorWorldSpeed = Vector3.Distance(hitPoint, _lastCursorWorldPoint) / deltaTime;
            _lastCursorWorldPoint = hitPoint;
        }

        private bool TryGetCursorHitPoint(out Vector3 hitPoint)
        {
            EnsureGroundLayerMaskCached();

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, _groundLayerMask))
            {
                hitPoint = hit.point;
                return true;
            }

            hitPoint = default;
            return false;
        }

        private void EnsureGroundLayerMaskCached()
        {
            if (_isGroundLayerMaskCached)
                return;

            _groundLayerMask = 1 << LayerMask.NameToLayer(placeableObjectSettings.groundLayerName) |
                               1 << LayerMask.NameToLayer(placeableObjectSettings.defaultLayerName);
            _isGroundLayerMaskCached = true;
        }

        private void UpdateTargetFromRaycast(Vector3 hitPoint)
        {
            var cursorCellPosition = WorldToCellPosition(hitPoint);
            var currentPosition = _placeableObject.CurrentPosition;
            var newPosition = _cursorPlane.IsTransitioning
                ? new Vector3Int(currentPosition.x, _cursorPlane.currentLayer, currentPosition.z)
                : new Vector3Int(cursorCellPosition.x, _cursorPlane.currentLayer, cursorCellPosition.z);

            newPosition = ClampPickedPositionToCursorPlaneRange(newPosition);
            if (newPosition != currentPosition)
            {
                _placeableObject.CurrentPosition = newPosition;
                _targetWorldPosition = CellToWorldPosition(newPosition);
            }

            HasValidTarget = IsWithinGridBounds(_placeableObject.CurrentPosition);
        }

        private void SyncWithCursorPlaneTransition()
        {
            var currentPosition = _placeableObject.CurrentPosition;
            _placeableObject.CurrentPosition = new Vector3Int(currentPosition.x, _cursorPlane.currentLayer, currentPosition.z);

            var syncedPosition = CellToWorldPosition(_placeableObject.CurrentPosition);
            _targetWorldPosition = new Vector3(_placeableObject.transform.position.x, syncedPosition.y, _placeableObject.transform.position.z);
            _placeableObject.transform.position =
                new Vector3(_placeableObject.transform.position.x, syncedPosition.y, _placeableObject.transform.position.z);

            HasValidTarget = IsWithinGridBounds(_placeableObject.CurrentPosition);
        }

        private void ClampTargetWorldPosition()
        {
            _targetWorldPosition = ClampWorldPositionToCursorPlaneRange(_targetWorldPosition);
        }

        private void ApplyMovementTowardsTarget()
        {
            var distanceToTarget = Vector3.Distance(_placeableObject.transform.position, _targetWorldPosition);
            if (distanceToTarget <= placeableObjectSettings.positionThreshold)
                return;

            _currentMoveSpeed = Mathf.Lerp(
                _currentMoveSpeed,
                CalculateDynamicMoveSpeed(distanceToTarget),
                placeableObjectSettings.dynamicSpeedSmoothing * Time.deltaTime);

            _placeableObject.transform.position = Vector3.MoveTowards(
                _placeableObject.transform.position,
                _targetWorldPosition,
                _currentMoveSpeed * Time.deltaTime);
        }

        private float CalculateDynamicMoveSpeed(float distanceToTarget)
        {
            var cursorFactor = Mathf.Clamp01(_cursorWorldSpeed / placeableObjectSettings.cursorSpeedForMaxFactor);
            var catchUpFactor = Mathf.Clamp01(distanceToTarget / placeableObjectSettings.catchUpDistanceForMaxFactor);
            var blend = Mathf.Max(cursorFactor, catchUpFactor);
            var factor = Mathf.Lerp(placeableObjectSettings.minDynamicSpeedFactor, placeableObjectSettings.maxDynamicSpeedFactor, blend);
            return placeableObjectSettings.moveSpeed * factor;
        }

        private Vector3Int ClampPickedPositionToCursorPlaneRange(Vector3Int cellPosition)
        {
            var worldPosition = CellToWorldPosition(cellPosition);
            var clampedWorldPosition = ClampWorldPositionToCursorPlaneRange(worldPosition);
            return WorldToCellPosition(clampedWorldPosition);
        }

        private Vector3 ClampWorldPositionToCursorPlaneRange(Vector3 worldPosition)
        {
            var origin = _placeableObject.IsPlayerObject ? _cellGrid.OwnOrigin : _cellGrid.EnemyOrigin;
            var additionalRange = placeableObjectSettings.additionalMovementRange;
            var halfAdditionalRange = additionalRange * 0.5f;
            var minX = origin.x - halfAdditionalRange;
            var maxX = origin.x + _cellGrid.GridSize.x + halfAdditionalRange;
            var minZ = origin.z - halfAdditionalRange;
            var maxZ = origin.z + _cellGrid.GridSize.z + halfAdditionalRange;

            return new Vector3(
                Mathf.Clamp(worldPosition.x, minX, maxX),
                worldPosition.y,
                Mathf.Clamp(worldPosition.z, minZ, maxZ)
            );
        }

        private Vector3Int WorldToCellPosition(Vector3 worldPosition)
        {
            var origin = _placeableObject.IsPlayerObject ? _cellGrid.OwnOrigin : _cellGrid.EnemyOrigin;
            return CoordinateConverter.WorldToCellPosition(worldPosition, origin, _cursorPlane.currentLayer,
                placeableObjectSettings.coordinateRoundingOffset);
        }

        private Vector3 CellToWorldPosition(Vector3Int cellPosition)
        {
            var origin = _placeableObject.IsPlayerObject ? _cellGrid.OwnOrigin : _cellGrid.EnemyOrigin;
            return CoordinateConverter.CellToWorldPosition(cellPosition, origin, placeableObjectSettings.cellCenterOffset);
        }

        private bool IsWithinGridBounds(Vector3Int cellPosition)
        {
            return cellPosition.x >= 0 && cellPosition.x < _cellGrid.GridSize.x &&
                   cellPosition.y >= 0 && cellPosition.y < _cellGrid.GridSize.y &&
                   cellPosition.z >= 0 && cellPosition.z < _cellGrid.GridSize.z;
        }

        private void UpdateMovingState()
        {
            var currentPosition = _placeableObject.transform.position;
            IsMoving = Vector3.Distance(_previousPosition, currentPosition) >= placeableObjectSettings.positionThreshold;
            _previousPosition = currentPosition;
        }
    }
}
