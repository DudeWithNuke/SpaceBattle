using UnityEngine;
using Reflex.Attributes;

namespace GameBoard
{
    public class CursorPlane : MonoBehaviour
    {
        [Inject] 
        private CellGrid _cellGrid;

        public Plane Plane { get; private set; }
        public bool IsTransitioning { get; private set; }

        public int layersCount;
        public int currentLayer;
        
        private float _targetY;

        private void Start()
        {
            layersCount = _cellGrid.GridSize.y;
            
            currentLayer = CursorPlaneConfig.DefaultStartLayer >= 0 ? 
                CursorPlaneConfig.DefaultStartLayer : 
                layersCount / 2;

            _targetY = currentLayer;
            gameObject.transform.position = GetActualPosition(_targetY);
            Plane = new Plane(Vector3.up, gameObject.transform.position);
            RefreshLayerState();
        }
        
        private void Update()
        {
            UpdateMode();
            UpdateVerticalTransition();
        }

        private void UpdateMode()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel") * CursorPlaneConfig.ScrollSensitivity;

            if (!(Mathf.Abs(scroll) > CursorPlaneConfig.ScrollThreshold)) return;
            switch (scroll)
            {
                case > 0f:
                    Up();
                    break;
                case < 0f:
                    Down();
                    break;
            }
        }


        private void Up()
        {
            if (currentLayer >= layersCount - 1)
                return;

            currentLayer++;
            _targetY = currentLayer;
            RefreshLayerState();
        }

        private void Down()
        {
            if (currentLayer <= 0)
                return;

            currentLayer--;
            _targetY = currentLayer;
            RefreshLayerState();
        }

        public void SetLayer(int layerIndex, bool immediate = false)
        {
            var clampedLayer = Mathf.Clamp(layerIndex, 0, Mathf.Max(0, layersCount - 1));
            currentLayer = clampedLayer;
            _targetY = currentLayer;
            RefreshLayerState();

            if (!immediate)
                return;

            gameObject.transform.position = GetActualPosition(_targetY);
            IsTransitioning = false;
            var newPlane = Plane;
            newPlane.SetNormalAndPosition(Vector3.up, gameObject.transform.position);
            Plane = newPlane;
        }

        private void UpdateVerticalTransition()
        {
            var currentPos = gameObject.transform.position;
            var nextY = Mathf.MoveTowards(currentPos.y, _targetY, CursorPlaneConfig.VerticalTransitionSpeed * Time.deltaTime);
            gameObject.transform.position = GetActualPosition(nextY);

            IsTransitioning = !Mathf.Approximately(nextY, _targetY);

            var newPlane = Plane;
            newPlane.SetNormalAndPosition(Vector3.up, gameObject.transform.position);
            Plane = newPlane;
        }

        private void RefreshLayerState()
        {
            SetLayerActive(currentLayer, true, true);
        }

        private static Vector3 GetActualPosition(float y)
        {
            return new Vector3(0, y, 0);
        }

        private void SetLayerActive(int layerIndex, bool isPlayerCell, bool isActive)
        {
            var cells = isPlayerCell ? _cellGrid.PlayerCells : _cellGrid.EnemyCells;
            foreach (var cell in cells)
                if (cell.Position.y == layerIndex && isActive)
                    cell.ActivateLayer();
                else
                    cell.DisableLayer();
        }
    }
}
