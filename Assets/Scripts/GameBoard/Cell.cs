using PlaceableObject;
using UnityEngine;

namespace GameBoard
{
    public class Cell : MonoBehaviour
    {
        private GameObject _cellPrefab;
        private Renderer _cellRenderer;

        private Vector3Int _position;
        private CellState _state;
        private CellState _previousState;

        private void OnStateChanged(PlaceableObjectState placeableObjectState)
        {
            _state = placeableObjectState switch
            {
                PlaceableObjectState.Placed when _state == CellState.Hovered => CellState.Selected,
                PlaceableObjectState.Picked when _state == CellState.Selected => _previousState,
                _ => _state
            };
        }

        public void Initialize(Vector3Int position)//, PlaceableObject.PlaceableObject placeableObject)
        {
            //placeableObject.OnStateChanged += OnStateChanged;

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
        
        // todo решить проблему множественного селекта/деселекта при пересечениях

        private void CursorPlaneCollideEnter(Collider other)
        {
            if (other.GetComponent<CursorPlane>() && _state != CellState.Selected)
                UpdateState(CellState.ActiveLayer);
        }

        private void CursorPlaneCollideExit(Collider other)
        {
            if (other.GetComponent<CursorPlane>() && _state != CellState.Selected)
                UpdateState(CellState.DisabledLayer);
        }

        private void PlaceableObjectStay(Collider other)
        {
            var placeableObject = other.GetComponent<PlaceableObject.PlaceableObject>();

            if (!placeableObject)
                return;

            if (placeableObject.IsMoving)
                return;

            if (_state is CellState.DisabledLayer or CellState.ActiveLayer)
            {
                _previousState = _state;
                UpdateState(CellState.Hovered);
            }

            if (_state == CellState.Selected)
                UpdateState(CellState.HoveredSelected);
        }

        private void PlaceableObjectExit(Collider other)
        {
            if (!other.GetComponent<PlaceableObject.PlaceableObject>())
                return;

            if (_state is CellState.Hovered)
                UpdateState(_previousState);

            if (_state is CellState.HoveredSelected)
                UpdateState(CellState.Selected);
        }
        
        private void UpdateState(CellState newState)
        {
            _state = newState;
            _cellRenderer.material.color = _state switch
            {
                CellState.DisabledLayer => Color.black,
                CellState.ActiveLayer => Color.white,
                CellState.Hovered => Color.yellow,
                CellState.Selected => Color.green,
                CellState.HoveredSelected => Color.cyan,
                _ => _cellRenderer.material.color
            };
        }
    }
}