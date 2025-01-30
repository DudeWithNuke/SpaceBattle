using UnityEngine;

namespace GameBoard
{
    public class Cell : MonoBehaviour
    {
        private const int LeftMouseButton = 0; 
        private const int RightMouseButton = 1;
        
        private GameObject _cellPrefab;
        private Renderer _cellRenderer;

        private Vector3Int _position;
        private CellState _currentState;

        private CellState _previousState;

        public void Initialize(Vector3Int position)
        {
            _cellPrefab = gameObject;
            _cellRenderer = GetComponent<Renderer>();

            _cellPrefab.gameObject.layer = _position.y;
            _cellPrefab.name = $"X: {_position.x}, Y: {_position.y}, Z: {_position.z}";

            _position = position;

            UpdateState(CellState.DisabledLayer);
        }

        public void DestroyPrefab()
        {
            DestroyImmediate(_cellPrefab);
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(LeftMouseButton)) return;
            
            if(_currentState == CellState.Hovered)
                UpdateState(CellState.Selected);

            if (_currentState == CellState.HoveredSelected)
                UpdateState(_previousState);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            CursorPlaneCollideEnter(other);
        }

        private void OnTriggerStay(Collider other)
        {
            PlaceableObjectStay(other);
        }

        private void OnTriggerExit(Collider other)
        {
            CursorPlaneCollideExit(other);
            PlaceableObjectExit(other);
        }

        private void UpdateState(CellState newState)
        {
            _currentState = newState;
            _cellRenderer.material.color = _currentState switch
            {
                CellState.DisabledLayer => Color.black,
                CellState.ActiveLayer => Color.white,
                CellState.Hovered => Color.yellow,
                CellState.Selected => Color.green,
                CellState.HoveredSelected => Color.cyan,
                _ => _cellRenderer.material.color
            };
        }
        
        // todo четко структурировать логику смены состояний
        // todo решить проблему множественного селекта/деселекта при пересечениях
        
        private void CursorPlaneCollideEnter(Collider other)
        {
            if (other.GetComponent<CursorPlane>() && _currentState != CellState.Selected)
                UpdateState(CellState.ActiveLayer);
        }

        private void CursorPlaneCollideExit(Collider other)
        {
            if (other.GetComponent<CursorPlane>() && _currentState != CellState.Selected)
                UpdateState(CellState.DisabledLayer);
        }

        private void PlaceableObjectStay(Collider other)
        {
            var placeableObject = other.GetComponent<PlaceableObject.PlaceableObject>();

            if (!placeableObject)
                return;

            if (placeableObject.IsMoving)
                return;

            if (_currentState is CellState.DisabledLayer or CellState.ActiveLayer)
            {
                _previousState = _currentState;
                UpdateState(CellState.Hovered);
            }

            if (_currentState == CellState.Selected)
                UpdateState(CellState.HoveredSelected);
        }

        private void PlaceableObjectExit(Collider other)
        {
            if (!other.GetComponent<PlaceableObject.PlaceableObject>())
                return;

            if (_currentState is CellState.Hovered)
                UpdateState(_previousState);
            
            if(_currentState is CellState.HoveredSelected)
                UpdateState(CellState.Selected);
        }
    }
}