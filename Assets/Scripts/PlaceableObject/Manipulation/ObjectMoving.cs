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

        private const float MinimalSpeed = 0.01f;
        public bool IsMoving { get; private set; }

        private Camera _camera;
        private PlaceableObject _placeableObject;
        private Vector3 _previousPosition;
        private Vector3 _targetWorldPosition;
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

            var threshold = placeableObjectSettings != null ? placeableObjectSettings.positionThreshold : MinimalSpeed;
            return Vector3.Distance(_placeableObject.transform.position, _targetWorldPosition) <= threshold;
        }

        private void UpdatePosition()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var groundLayerMask = 1 << LayerMask.NameToLayer(placeableObjectSettings.groundLayerName) |
                                  1 << LayerMask.NameToLayer(placeableObjectSettings.defaultLayerName);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayerMask))
            {
                var cursorCellPosition = WorldToCellPosition(hit.point);
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
            else
            {
                HasValidTarget = false;
            }

            if (_cursorPlane.IsTransitioning)
            {
                var currentPosition = _placeableObject.CurrentPosition;
                _placeableObject.CurrentPosition = new Vector3Int(currentPosition.x, _cursorPlane.currentLayer, currentPosition.z);
                var syncedPosition = CellToWorldPosition(_placeableObject.CurrentPosition);
                _targetWorldPosition = new Vector3(_placeableObject.transform.position.x, syncedPosition.y, _placeableObject.transform.position.z);
                _placeableObject.transform.position = new Vector3(_placeableObject.transform.position.x, syncedPosition.y, _placeableObject.transform.position.z);
                HasValidTarget = IsWithinGridBounds(_placeableObject.CurrentPosition);
            }

            _targetWorldPosition = ClampWorldPositionToCursorPlaneRange(_targetWorldPosition);

            var positionThreshold = placeableObjectSettings != null ? placeableObjectSettings.positionThreshold : MinimalSpeed;
            if (Vector3.Distance(_placeableObject.transform.position, _targetWorldPosition) > positionThreshold)
            {
                _placeableObject.transform.position = Vector3.MoveTowards(_placeableObject.transform.position, _targetWorldPosition,
                    (placeableObjectSettings != null ? placeableObjectSettings.moveSpeed : 50f) * Time.deltaTime);
            }
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
            var additionalRange = placeableObjectSettings != null ? placeableObjectSettings.additionalMovementRange : 40f;
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
                placeableObjectSettings != null ? placeableObjectSettings.coordinateRoundingOffset : 0.5f);
        }

        private Vector3 CellToWorldPosition(Vector3Int cellPosition)
        {
            var origin = _placeableObject.IsPlayerObject ? _cellGrid.OwnOrigin : _cellGrid.EnemyOrigin;
            return CoordinateConverter.CellToWorldPosition(cellPosition, origin,
                placeableObjectSettings != null ? placeableObjectSettings.cellCenterOffset : 0.5f);
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
            var threshold = placeableObjectSettings != null ? placeableObjectSettings.positionThreshold : MinimalSpeed;
            IsMoving = Vector3.Distance(_previousPosition, currentPosition) >= threshold;
            _previousPosition = currentPosition;
        }
    }
}
