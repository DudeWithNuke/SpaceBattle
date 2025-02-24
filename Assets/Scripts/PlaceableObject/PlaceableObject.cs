using System;
using System.Collections.Generic;
using GameBoard;
using ModestTree;
using UnityEngine;

namespace PlaceableObject
{
    public class PlaceableObject : MonoBehaviour
    { 
        public event Action<PlaceableObject> OnPlaced;
        public event Action<PlaceableObject> OnPicked;
        
        public PlaceableObjectState State { get; private set; }
        [SerializeField] public BoxCollider[] BoxColliders;
        
        private void Start()
        {
            //BoxColliders = GetComponents<BoxCollider>();
            State = PlaceableObjectState.Picked;
        }
        
        
        // todo описать коллизии
        // 1. Нельзя разместить вне клетки
        // 2. Нельзя разместить внутри другого placeable object (если это два ship)
        private void OnMouseUp()
        {
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
        
        private void Place()
        {
            State = PlaceableObjectState.Placed;
            OnPlaced?.Invoke(this);
        }
        
        private void Pick()
        {
            State = PlaceableObjectState.Picked;
            OnPicked?.Invoke(this);
        }

        public HashSet<Cell> GetOverlappingCells(Collider[] cellColliders)
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