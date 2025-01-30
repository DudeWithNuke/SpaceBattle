using System;
using ModestTree;
using UnityEngine;

namespace GameBoard
{
    public class Cell : MonoBehaviour
    {
        private GameObject _cellPrefab;
        private Renderer _cellRenderer;
        public Collider CellCollider {private set; get;}

        public Vector3Int Position { get; private set; }
        public CellState CurrentState { get; private set; }
        
        public CellState _previousState;

        public void Initialize(Vector3Int position)
        {
            _cellPrefab =  gameObject;
            _cellRenderer = GetComponent<Renderer>();
            CellCollider = GetComponent<Collider>();

            _cellPrefab.gameObject.layer = Position.y;
            _cellPrefab.name = $"X: {Position.x}, Y: {Position.y}, Z: {Position.z}";

            Position = position;

            UpdateState(CellState.DisabledLayer);
        }

        public void DestroyPrefab()
        {
            DestroyImmediate(_cellPrefab);
        }

        public void UpdateState(CellState newState)
        {
            CurrentState = newState;
            _cellRenderer.material.color = CurrentState switch
            {
                CellState.DisabledLayer => Color.black,
                CellState.ActiveLayer => Color.white,
                CellState.Hovered => Color.yellow,
                CellState.HoveredSelected => Color.cyan,
                _ => _cellRenderer.material.color
            };
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.GetComponent<PlaceableObject.PlaceableObject>().IsMoving)
                return;

            if (CurrentState is CellState.DisabledLayer or CellState.ActiveLayer)
            {
                _previousState = CurrentState;
                UpdateState(CellState.Hovered);
            }

            if (CurrentState == CellState.Selected)
                UpdateState(CellState.HoveredSelected);
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (CurrentState is CellState.Hovered or CellState.HoveredSelected)
                UpdateState(_previousState);
        }
    }
}