using System;
using System.Collections.Generic;
using GameBoard;
using Reflex.Attributes;
using UnityEngine;

namespace PlaceableObject
{
    public class PlaceableObject : MonoBehaviour
    { 
        public event Action<PlaceableObject> OnPlaced;
        public event Action<PlaceableObject> OnPicked;
      
        private Camera _camera;
        
        public PlaceableObjectState State { get; private set; }
        [SerializeField] public BoxCollider[] BoxColliders;
        
        //todo написать фабрику для создания placeable object, тут связать через нормальный DI
        private CellGrid _cellGrid;
        
        private void Start()
        {
            _camera = Camera.main;
            State = PlaceableObjectState.Picked;
        }
        
        private void Update()
        {
            if (!Input.GetMouseButtonUp(0))
                return;
            
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            
            var placeableObjectLayerMask = 1 << LayerMask.NameToLayer("PlaceableObjectLayer");
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, placeableObjectLayerMask) 
                || hit.collider.gameObject != gameObject) 
                return;
            
            switch (State)
            {
                case PlaceableObjectState.Picked:
                    Place();
                    break;
                case PlaceableObjectState.Placed:
                    Pick();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool IsWithinGridBounds()
        { 
            var allCells = _cellGrid.EnemyCells;
            var cellColliders = new Collider[allCells.Count];
            
            for (var i = 0; i < allCells.Count; i++)
                cellColliders[i] = allCells[i].CellCollider;

            var overlappingCells = GetOverlappingCells(cellColliders);
            return overlappingCells.Count > 0;
        }

        private void Place()
        {
            if (!IsWithinGridBounds())
                return;
            
            State = PlaceableObjectState.Placed;
            OnPlaced?.Invoke(this);
        }

        private void Pick()
        {
            State = PlaceableObjectState.Picked;
            OnPicked?.Invoke(this);
        }

        private HashSet<Cell> GetOverlappingCells(Collider[] cellColliders)
        {
            var overlappingCells = new HashSet<Cell>();

            foreach (var boxCollider in BoxColliders)
            {
                var hits = Physics.OverlapBoxNonAlloc(boxCollider.transform.position, boxCollider.bounds.extents, cellColliders, transform.rotation);
                for (var i = 0; i < hits; i++)
                    if (cellColliders[i] && cellColliders[i].TryGetComponent<Cell>(out var cell))
                        overlappingCells.Add(cell);
            }

            return overlappingCells;
        }
    }
}