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
        public Vector3Int Position => _position;

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

        private void SetState(CellState newState)
        {
            _state = newState;
            CellVisualManager.ApplyVisualState(_cellRenderer, newState);
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

        public void SetCursorHover(bool isHovering)
        {
            if (_state != CellState.Selected)
                SetState(isHovering ? CellState.ActiveLayer : CellState.DisabledLayer);
        }

        public void SetPlaceableHover(bool isHovering)
        {
            switch (_state)
            {
                case CellState.DisabledLayer:
                    if (isHovering)
                    {
                        _previousState = _state;
                        SetState(CellState.Hovered);
                    }
                    break;
                case CellState.ActiveLayer:
                    if (isHovering)
                    {
                        _previousState = _state;
                        SetState(CellState.Hovered);
                    }
                    else
                    {
                        SetState(_previousState);
                    }
                    break;
                case CellState.Selected:
                    SetState(isHovering ? CellState.HoveredSelected : CellState.Selected);
                    break;
                case CellState.Hovered:
                    if (!isHovering)
                        SetState(_previousState);
                    break;
                case CellState.HoveredSelected:
                    if (!isHovering)
                        SetState(CellState.Selected);
                    break;
            }
        }

        public void SetSelected(bool isSelected)
        {
            SetState(isSelected ? CellState.Selected : CellState.DisabledLayer);
        }
    }
}
