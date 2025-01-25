using System;
using UnityEngine;

namespace GameBoard
{
    public class Cell : MonoBehaviour
    {
        private GameObject _cellPrefab;
        private Renderer _cellRenderer;
        public Collider CellCollider {private set; get;}

        public Vector3Int Position { get; private set; }
        public CellState State { get; private set; }
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
            State = newState;
            _cellRenderer.material.color = State switch
            {
                CellState.DisabledLayer => Color.black,
                CellState.ActiveLayer => Color.white,
                CellState.Hovered => Color.yellow,
                CellState.HoveredSelectedTarget => Color.cyan,
                _ => _cellRenderer.material.color
            };
        }

        private void OnTriggerStay(Collider other)
        {
            _previousState = State;
            //todo проверять is moving
            UpdateState(CellState.Hovered);
        }
        
        private void OnTriggerExit(Collider other)
        {
            UpdateState(_previousState);
        }
    }
}