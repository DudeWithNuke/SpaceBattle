using System.Collections.Generic;
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
        private Vector3Int _previousHoveredCell;
        private bool _isHoveringCell;

        private void Start()
        {
            _camera = Camera.main;
            layersCount = _cellGrid.GridSize.y;
            
            // Используем конфигурацию для начального слоя
            currentLayer = CursorPlaneConfig.DefaultStartLayer >= 0 ? 
                CursorPlaneConfig.DefaultStartLayer : 
                layersCount / 2;

            gameObject.transform.position = GetActualPosition();
            Plane = new Plane(Vector3.up, GetActualPosition());
            Refresh();
            
            _isHoveringCell = false;
        }
        
        private void Update()
        {
            UpdateMode();
            UpdateCellHover();
            UpdateCameraPosition();
        }
        
        public void Activate()
        {
            enabled = true;
        }

        public void Deactivate()
        {
            enabled = false;
            // Сбрасываем hover состояние при деактивации
            if (_isHoveringCell)
            {
                _cellGrid.SetCursorHoverForCells(new[] { _previousHoveredCell }, true, false);
                _isHoveringCell = false;
            }
        }

        public void UpdateMode()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel") * CursorPlaneConfig.ScrollSensitivity;
            
            if (Mathf.Abs(scroll) > CursorPlaneConfig.ScrollThreshold)
            {
                if (scroll > 0f)
                {
                    Up();
                    IsCameraMovingVertically = true;
                }
                else if (scroll < 0f)
                {
                    Down();
                    IsCameraMovingVertically = true;
                }
            }
            else
            {
                // Если нет ввода, проверяем закончила ли камера движение
                var targetY = GetActualPosition().y + CursorPlaneConfig.CameraHeightOffset;
                if (Mathf.Abs(_camera.transform.position.y - targetY) < CursorPlaneConfig.CameraStopThreshold)
                {
                    IsCameraMovingVertically = false;
                }
            }
        }

        private void UpdateCameraPosition()
        {
            if (_camera != null)
            {
                var targetPosition = new Vector3(_camera.transform.position.x, 
                                                 GetActualPosition().y + CursorPlaneConfig.CameraHeightOffset, 
                                                 _camera.transform.position.z);
                _camera.transform.position = Vector3.Lerp(_camera.transform.position, 
                                                          targetPosition, 
                                                          CursorPlaneConfig.CameraFollowSpeed * Time.deltaTime);
            }
        }

        private void UpdateCellHover()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            
            if (!Plane.Raycast(ray, out var enterDistance))
            {
                // Курсор не над плоскостью - не сбрасываем hover если это активный слой
                if (_isHoveringCell)
                {
                    _cellGrid.SetCursorHoverForCells(new[] { _previousHoveredCell }, true, false);
                    // Восстанавливаем белый цвет для активного слоя
                    _cellGrid.SetLayerActive(currentLayer, true, true);
                    _isHoveringCell = false;
                }
                return;
            }

            var hitPoint = ray.GetPoint(enterDistance);
            var cellPosition = WorldToCellPosition(hitPoint);

            // Проверяем, изменилась ли позиция клетки
            if (!_isHoveringCell || cellPosition != _previousHoveredCell)
            {
                // Сбрасываем предыдущую клетку
                if (_isHoveringCell)
                {
                    _cellGrid.SetCursorHoverForCells(new[] { _previousHoveredCell }, true, false);
                    // Восстанавливаем белый цвет для активного слоя
                    _cellGrid.SetLayerActive(currentLayer, true, true);
                }

                // Проверяем, что новая позиция в пределах грида
                if (IsCellPositionValid(cellPosition))
                {
                    _cellGrid.SetCursorHoverForCells(new[] { cellPosition }, true, true);
                    _previousHoveredCell = cellPosition;
                    _isHoveringCell = true;
                }
                else
                {
                    _isHoveringCell = false;
                }
            }
        }

        private Vector3Int WorldToCellPosition(Vector3 worldPosition)
        {
            return CoordinateConverter.WorldToCellPosition(worldPosition, _cellGrid.PlayerOrigin, currentLayer);
        }

        private bool IsCellPositionValid(Vector3Int position)
        {
            return CoordinateConverter.IsPositionValid(position, _cellGrid.GridSize);
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
            
            // Сбрасываем hover состояние перед сменой слоя
            if (_isHoveringCell)
            {
                _cellGrid.SetCursorHoverForCells(new[] { _previousHoveredCell }, true, false);
                _isHoveringCell = false;
            }
            
            // Активируем только клетки текущего слоя и выделяем их белым
            _cellGrid.SetLayerActive(currentLayer, true, true);
        }

        private Vector3 GetActualPosition()
        {
            return new Vector3(0, currentLayer, 0);
        }
    }
}
