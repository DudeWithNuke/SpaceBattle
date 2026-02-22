using GameBoard;
using Reflex.Attributes;
using UnityEngine;

namespace CameraControls
{
    public class CameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float keyboardZoomMultiplier = 8f;
        [SerializeField] private float minZoomDistance = 5f;
        [SerializeField] private float maxZoomDistance = 50f;
        [SerializeField] private float boundaryMargin = 1f;

        [Header("Orbital Rotation")] 
        private float RotationSpeed { get; set; } = 2f;
        [SerializeField, Range(0f, 90f)] private float maxAngleAbovePlane = 90f;
        [SerializeField, Range(0f, 90f)] private float maxAngleBelowPlane = 90f;
        [SerializeField, Range(0.1f, 10f)] private float poleSafetyAngle = 1f;

        [Header("Battlefield Switching")]
        [SerializeField] private float switchFocusSpeed = 60f;

        [Inject] private CellGrid _cellGrid;
        [Inject] private CursorPlane _cursorPlane;

        private Transform _cameraTransform;

        private Vector3 _focusPoint;
        private float _currentZoomDistance = 20f;
        private float _cachedCursorY;
        private float _currentHorizontalAngle;
        private float _currentVerticalAngle = 45f;
        private bool _isRotating;

        private Vector3 _lastValidPlanarForward = Vector3.forward;
        private Vector3 _lastValidPlanarRight = Vector3.right;

        private BattlefieldSwitchController _battlefieldSwitchController;

        private void Start()
        {
            _cameraTransform = Camera.main.transform;

            _battlefieldSwitchController = new BattlefieldSwitchController(_cellGrid.PlayerOrigin, _cellGrid.EnemyOrigin);

            _focusPoint = GetBattlefieldCenter(_battlefieldSwitchController.IsPlayerBattlefieldActive);
            CursorPlaneVerticalController.UpdateCachedY(_cursorPlane, ref _cachedCursorY);

            UpdateCameraPosition();
        }

        private void Update()
        {
            HandleBattlefieldSwitchInput();

            if (_battlefieldSwitchController.IsSwitching)
                _battlefieldSwitchController.UpdateTransition(ref _focusPoint, switchFocusSpeed, Time.deltaTime);
            else
            {
                OrbitController.HandleOrbitInput(
                    ref _isRotating,
                    ref _currentHorizontalAngle,
                    ref _currentVerticalAngle,
                    RotationSpeed,
                    maxAngleAbovePlane,
                    maxAngleBelowPlane,
                    poleSafetyAngle
                );

                FocusPanController.HandleFocusMovement(
                    _cameraTransform,
                    ref _focusPoint,
                    ref _lastValidPlanarForward,
                    ref _lastValidPlanarRight,
                    moveSpeed,
                    Time.deltaTime,
                    ClampToGridBounds
                );

                ZoomController.HandleKeyboardZoom(
                    ref _currentZoomDistance,
                    zoomSpeed,
                    keyboardZoomMultiplier,
                    minZoomDistance,
                    maxZoomDistance
                );
            }

            CursorPlaneVerticalController.UpdateCachedY(_cursorPlane, ref _cachedCursorY);
            UpdateCameraPosition();
        }

        private void HandleBattlefieldSwitchInput()
        {
            if (!Input.GetKeyDown(KeyCode.Space))
                return;

            OrbitController.StopOrbit(ref _isRotating);
            _battlefieldSwitchController.StartSwitch(_focusPoint, ClampToGridBounds);
        }

        private Vector3 GetBattlefieldCenter(bool playerField)
        {
            var gridSize = _cellGrid.GridSize;
            var origin = playerField ? _cellGrid.PlayerOrigin : _cellGrid.EnemyOrigin;
            return origin + new Vector3(gridSize.x / 2f, 0f, gridSize.z / 2f);
        }

        private void UpdateCameraPosition()
        {
            var orbitCenter = new Vector3(_focusPoint.x, _cachedCursorY, _focusPoint.z);

            var verticalAngleRad = _currentVerticalAngle * Mathf.Deg2Rad;
            var horizontalAngleRad = _currentHorizontalAngle * Mathf.Deg2Rad;

            var x = _currentZoomDistance * Mathf.Sin(verticalAngleRad) * Mathf.Cos(horizontalAngleRad);
            var y = _currentZoomDistance * Mathf.Cos(verticalAngleRad);
            var z = _currentZoomDistance * Mathf.Sin(verticalAngleRad) * Mathf.Sin(horizontalAngleRad);

            var targetPosition = orbitCenter + new Vector3(x, y, z);

            _cameraTransform.position = targetPosition;
            _cameraTransform.LookAt(orbitCenter);
        }

        private Vector3 ClampToGridBounds(Vector3 point)
        {
            var gridSize = _cellGrid.GridSize;
            var origin = _battlefieldSwitchController.IsPlayerBattlefieldActive 
                ? _cellGrid.PlayerOrigin
                : _cellGrid.EnemyOrigin;

            var minX = origin.x + boundaryMargin;
            var maxX = origin.x + gridSize.x - 1 - boundaryMargin;
            var minZ = origin.z + boundaryMargin;
            var maxZ = origin.z + gridSize.z - 1 - boundaryMargin;

            return new Vector3(
                Mathf.Clamp(point.x, minX, maxX),
                0f,
                Mathf.Clamp(point.z, minZ, maxZ)
            );
        }
    }
}
