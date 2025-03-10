﻿using System;
using System.Collections.Generic;
using GameBoard;
using ModestTree;
using UnityEngine;

namespace PlaceableObject
{
    public class PlaceableObject : MonoBehaviour
    {
        public event Action<PlaceableObject> OnStateChange;
        
        public PlaceableObjectState State { get; private set; }
        [SerializeField] public BoxCollider[] BoxColliders;
        
        private void Start()
        {
            //BoxColliders = GetComponents<BoxCollider>();
            State = PlaceableObjectState.Picked;
        }
        
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
            OnStateChange?.Invoke(this);
        }
        
        private void Pick()
        {
            State = PlaceableObjectState.Picked;
            OnStateChange?.Invoke(this);
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