using GameBoard;
using InputController;
using PlayerCamera.Movement;
using Reflex.Attributes;
using UnityEngine;

namespace PlayerCamera
{
    public class CameraMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private CameraSettings cameraSettings;

        [Inject] private CellGrid _cellGrid;
        [Inject] private CursorPlane _cursorPlane;

        private Transform _cameraTransform;

        private Switch _switch;
        private Orbiting _orbiting;
        private FocusPan _focusPan;
        private Zoom _zoom;
        private CameraState _cameraState;

        private void Start()
        {
            _cameraTransform = Camera.main.transform;

            _orbiting = new Orbiting(cameraSettings);
            _focusPan = new FocusPan(cameraSettings);
            _zoom = new Zoom(cameraSettings);
            _switch = new Switch(cameraSettings, _cellGrid.PlayerOrigin, _cellGrid.EnemyOrigin);

            _cameraState = new CameraState();
            _cameraState.FocusPoint = GetBattlefieldCenter(_cameraState.IsPlayerBattlefieldActive);
            _cameraState.FocusPoint = ClampToGridBounds(_cameraState.FocusPoint);

            UpdateCameraPosition();
        }

        public void Tick(CameraInputFrame inputFrame, float deltaTime)
        {
            if (inputFrame.SwitchBattlefieldRequested)
            {
                Orbiting.StopOrbit(_cameraState);
                _switch.StartSwitch(_cameraState, ClampToGridBounds);
            }

            if (_cameraState.IsSwitching)
                _switch.UpdateTransition(_cameraState, deltaTime);
            else
            {
                _focusPan.Handle(_cameraState, _cameraTransform, inputFrame.MoveInput, deltaTime, ClampToGridBounds);
                _orbiting.Handle(_cameraState, inputFrame.OrbitStarted, inputFrame.OrbitEnded, inputFrame.OrbitDelta);
                _zoom.Handle(_cameraState, inputFrame.ZoomDelta, deltaTime);
            }

            UpdateCameraPosition();
        }

        private Vector3 GetBattlefieldCenter(bool playerField)
        {
            var gridSize = _cellGrid.GridSize;
            var origin = playerField ? _cellGrid.PlayerOrigin : _cellGrid.EnemyOrigin;
            return origin + new Vector3(gridSize.x / 2f, 0f, gridSize.z / 2f);
        }

        private void UpdateCameraPosition()
        {
            var orbitCenter = new Vector3(_cameraState.FocusPoint.x, _cursorPlane.transform.position.y, _cameraState.FocusPoint.z);

            var verticalAngleRad = _cameraState.VerticalAngle * Mathf.Deg2Rad;
            var horizontalAngleRad = _cameraState.HorizontalAngle * Mathf.Deg2Rad;

            var x = _cameraState.ZoomDistance * Mathf.Sin(verticalAngleRad) * Mathf.Cos(horizontalAngleRad);
            var y = _cameraState.ZoomDistance * Mathf.Cos(verticalAngleRad);
            var z = _cameraState.ZoomDistance * Mathf.Sin(verticalAngleRad) * Mathf.Sin(horizontalAngleRad);

            _cameraTransform.position = orbitCenter + new Vector3(x, y, z);
            _cameraTransform.LookAt(orbitCenter);
        }

        private Vector3 ClampToGridBounds(Vector3 point)
        {
            var gridSize = _cellGrid.GridSize;
            var origin = _cameraState.IsPlayerBattlefieldActive
                ? _cellGrid.PlayerOrigin
                : _cellGrid.EnemyOrigin;

            var minX = origin.x + cameraSettings.boundaryMargin;
            var maxX = origin.x + gridSize.x - 1 - cameraSettings.boundaryMargin;
            var minZ = origin.z + cameraSettings.boundaryMargin;
            var maxZ = origin.z + gridSize.z - 1 - cameraSettings.boundaryMargin;

            return new Vector3(
                Mathf.Clamp(point.x, minX, maxX),
                0f,
                Mathf.Clamp(point.z, minZ, maxZ)
            );
        }
    }
}