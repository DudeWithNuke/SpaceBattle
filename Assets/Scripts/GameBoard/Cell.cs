using System;
using UnityEngine;
using Utils;

namespace GameBoard
{
    public class Cell : MonoBehaviour
    {
        private Renderer _cellRenderer;
        private CellStateController _stateController;

        public Vector3Int Position { get; private set; }
        public bool IsPlayerCell { get; private set; }
        public bool IsSelected => _stateController.CurrentState is CellState.Selected or CellState.HoveredSelected;

        private void Awake()
        {
            _cellRenderer = GetComponent<Renderer>();
            _stateController = new CellStateController(_cellRenderer);
        }

        public void Initialize(Vector3Int initPosition, bool initIsPlayerCell)
        {
            Position = initPosition;
            gameObject.layer = Position.y;

            IsPlayerCell = initIsPlayerCell;
            gameObject.name = $" {(initIsPlayerCell ? "Player" : "Enemy")} " +
                              $"X: {Position.x}, " +
                              $"Y: {Position.y}, " +
                              $"Z: {Position.z}";
            Log.Info(_stateController.CurrentState.ToString());
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
                case CellState.Hovered:
                case CellState.HoveredSelected:
                    break;
                case CellState.EmptyAttacked:
                case CellState.ShipAttacked:
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
                case CellState.DisabledLayer:
                case CellState.ActiveLayer:
                case CellState.Selected:
                case CellState.EmptyAttacked:
                case CellState.ShipAttacked:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetSelected(bool isSelected, bool isActiveLayer)
        {
            if (isSelected)
            {
                _stateController.SetState(CellState.Selected);
                return;
            }

            var targetState = isActiveLayer ? CellState.ActiveLayer : CellState.DisabledLayer;

            if (_stateController.CurrentState == CellState.HoveredSelected)
                _stateController.SetState(CellState.Selected);
            if (_stateController.CurrentState == CellState.Selected || 
                _stateController.CurrentState == CellState.Hovered || 
                _stateController.CurrentState != targetState)
                _stateController.SetState(targetState);
        }
    }
}
