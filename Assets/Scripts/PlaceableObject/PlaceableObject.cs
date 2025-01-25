﻿
using System.Collections.Generic;
using System.Linq;
using GameBoard;
using UnityEngine;

namespace PlaceableObject
{
    public class PlaceableObject : MonoBehaviour
    {
        public BoxCollider[] BoxColliders {get; private set;}
        
        private void OnValidate()
        {
            BoxColliders = gameObject.GetComponentsInChildren<BoxCollider>();
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