using System;
using UnityEngine;

namespace GameBoard
{
    public class Cell : MonoBehaviour
    {
        private Renderer _cellRenderer;
        private Vector3Int _position;

        private CellState _state;
        private CellState _previousState;

        public bool IsPlayerCell { get; set; }

        public Collider CellCollider { get; private set; }

        private void Awake()
        {
            CellCollider = GetComponent<Collider>();
            _cellRenderer = GetComponent<Renderer>();
        }

        public void Initialize(Vector3Int position, bool isPlayerCell)
        {
            _position = position;
            gameObject.layer = _position.y;
            
            IsPlayerCell = isPlayerCell;
            gameObject.name = $" {(IsPlayerCell ? "Player" : "Enemy")} " +
                              $"X: {_position.x}, " +
                              $"Y: {_position.y}, " +
                              $"Z: {_position.z}";

            SetState(CellState.DisabledLayer);
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out CursorPlane _))
                OnCursorEnter();
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent(out PlaceableObject.PlaceableObject _))
                OnPlaceableStay();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out CursorPlane _))
                OnCursorExit();

            if (other.TryGetComponent(out PlaceableObject.PlaceableObject _))
                OnPlaceableExit();
        }

        private void OnCursorEnter()
        {
            if (_state != CellState.Selected)
                SetState(CellState.ActiveLayer);
        }

        private void OnCursorExit()
        {
            if (_state != CellState.Selected)
                SetState(CellState.DisabledLayer);
        }

        private void OnPlaceableStay()
        {
            switch (_state)
            {
                case CellState.DisabledLayer or CellState.ActiveLayer:
                    _previousState = _state;
                    SetState(CellState.Hovered);
                    break;
                case CellState.Selected:
                    SetState(CellState.HoveredSelected);
                    break;
            }
        }

        private void OnPlaceableExit()
        {
            switch (_state)
            {
                case CellState.Hovered:
                    SetState(_previousState);
                    break;
                case CellState.HoveredSelected:
                    SetState(CellState.Selected);
                    break;
            }
        }

        private void SetState(CellState newState)
        {
            _state = newState;
            ApplyVisualForState(newState);
        }

        private void ApplyVisualForState(CellState state)
        {
            _cellRenderer.material.color = state switch
            {
                CellState.DisabledLayer => Color.black,
                CellState.ActiveLayer => Color.white,
                CellState.Hovered => Color.yellow,
                CellState.Selected => Color.green,
                CellState.HoveredSelected => Color.cyan,
                _ => _cellRenderer.material.color
            };
        }

        public void Activate()
        {
            enabled = true;
        }

        public void Deactivate()
        {
            SetState(CellState.DisabledLayer);
            enabled = false;
        }

        public void UpdateMode()
        {
            // Метод из интерфейса, пока не используется
        }
    }
}