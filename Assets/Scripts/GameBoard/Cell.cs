using System;
using UnityEngine;

namespace GameBoard
{
    public class Cell : MonoBehaviour
    {
        private Renderer _cellRenderer;
        private Vector3Int _position;
        private CellStateController _stateController;
        
        public bool IsPlayerCell { get; set; }

        public Vector3Int Position => _position;

        private void Awake()
        {
            _cellRenderer = GetComponent<Renderer>();
            _stateController = new CellStateController(_cellRenderer);
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
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
        }

        public void ActivateLayer()
        {
            if (_stateController.CurrentState != CellState.ActiveLayer)
                _stateController.SetState(CellState.ActiveLayer);
        }

        public void DisableLayer()
        {
            if (_stateController.CurrentState != CellState.DisabledLayer)
                _stateController.SetState(CellState.DisabledLayer);
        }

        public void SetPlaceableHover(bool isHovering)
        {
            if (isHovering)
                SetHoveredState();
            else
                RestoreNonHoveredState();
        }

        private void SetHoveredState()
        {
            switch (_stateController.CurrentState)
            {
                case CellState.DisabledLayer:
                case CellState.ActiveLayer:
                    _stateController.SetState(CellState.Hovered);
                    break;
                case CellState.Selected:
                    _stateController.SetState(CellState.HoveredSelected);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RestoreNonHoveredState()
        {
            switch (_stateController.CurrentState)
            {
                case CellState.Hovered:
                    _stateController.RestorePreviousState();
                    break;
                case CellState.HoveredSelected:
                    _stateController.SetState(CellState.Selected);
                    break;
            }
        }

        public void SetSelected(bool isSelected)
        {
            _stateController.SetState(CellState.Selected);
        }
    }
}
