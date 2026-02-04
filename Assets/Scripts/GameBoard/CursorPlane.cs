using UnityEngine;
using Reflex.Attributes;

namespace GameBoard
{
    public class CursorPlane : MonoBehaviour
    {
        [Inject] 
        private CellGrid _cellGrid;

        public Plane Plane { get; private set; }

        public int layersCount;
        public int currentLayer;
        public bool IsCameraMovingVertically { get; private set; }
        
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
            layersCount = _cellGrid.GridSize.y;
            
            currentLayer = CursorPlaneConfig.DefaultStartLayer >= 0 ? 
                CursorPlaneConfig.DefaultStartLayer : 
                layersCount / 2;

            gameObject.transform.position = GetActualPosition();
            Plane = new Plane(Vector3.up, GetActualPosition());
            Refresh();
        }
        
        private void Update()
        {
            UpdateMode();
            UpdateCameraPosition();
        }
        
        public void UpdateMode()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel") * CursorPlaneConfig.ScrollSensitivity;
            
            if (Mathf.Abs(scroll) > CursorPlaneConfig.ScrollThreshold)
                switch (scroll)
                {
                    case > 0f:
                        Up();
                        IsCameraMovingVertically = true;
                        break;
                    case < 0f:
                        Down();
                        IsCameraMovingVertically = true;
                        break;
                }
            else
            {
                var targetY = GetActualPosition().y + CursorPlaneConfig.CameraHeightOffset;
                if (Mathf.Abs(_camera.transform.position.y - targetY) < CursorPlaneConfig.CameraStopThreshold)
                    IsCameraMovingVertically = false;
            }
        }

        private void UpdateCameraPosition()
        {
            if (!_camera) 
                return;
            
            var targetPosition = new Vector3(_camera.transform.position.x, 
                GetActualPosition().y + CursorPlaneConfig.CameraHeightOffset, 
                _camera.transform.position.z);
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, 
                targetPosition, 
                CursorPlaneConfig.CameraFollowSpeed * Time.deltaTime);
        }

        private void Up()
        {
            if (currentLayer >= layersCount - 1)
                return;

            currentLayer++;
            Refresh();
        }

        private void Down()
        {
            if (currentLayer <= 0)
                return;

            currentLayer--;
            Refresh();
        }

        private void Refresh()
        {
            gameObject.transform.position = GetActualPosition();
            
            var newPlane = Plane;
            newPlane.SetNormalAndPosition(Vector3.up, GetActualPosition());
            Plane = newPlane;
            
            SetLayerActive(currentLayer, true, true);
        }

        private Vector3 GetActualPosition()
        {
            return new Vector3(0, currentLayer, 0);
        }

        private void SetLayerActive(int layerIndex, bool isPlayerCell, bool isActive)
        {
            var cells = isPlayerCell ? _cellGrid.PlayerCells : _cellGrid.EnemyCells;
            foreach (var cell in cells)
            {
                var isTargetLayer = cell.Position.y == layerIndex;
                if (isTargetLayer && isActive)
                {
                    cell.ActivateLayer();
                }
                else
                {
                    cell.DisableLayer();
                }
            }
        }
    }
}
