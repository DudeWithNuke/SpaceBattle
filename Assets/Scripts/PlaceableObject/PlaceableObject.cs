using System.Collections.Generic;
using GameBoard;
using UnityEngine;

namespace PlaceableObject
{
    public class PlaceableObject : MonoBehaviour
    {
        public PlaceableObjectState State { get; private set; }
        [SerializeField] public BoxCollider[] BoxColliders;
        public bool IsOverlappingCell { get; private set; }

        private void Start()
        {
            //BoxColliders = GetComponents<BoxCollider>();
            State = PlaceableObjectState.Picked;
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (other.GetComponent<Cell>()) 
                IsOverlappingCell = true;
        }
        
        private void OnTriggerExit(Collider other)
        { 
            if (other.GetComponent<Cell>()) 
                IsOverlappingCell = false;
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