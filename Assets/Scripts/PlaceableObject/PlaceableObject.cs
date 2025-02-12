﻿using System;
using System.Collections.Generic;
using GameBoard;
using ModestTree;
using UnityEngine;

namespace PlaceableObject
{
    public class PlaceableObject : MonoBehaviour
    {
        public event Action<PlaceableObjectState> OnStateChanged;
        
        public PlaceableObjectState _state;
        
        public BoxCollider[] BoxColliders { get; private set; }

        private Vector3 _previousPosition;
        private const float MinimalSpeed = 0.01f;

        public bool IsMoving { get; private set; }
        public bool IsOverlappingCell { get; private set; }

        private void OnValidate()
        {
            BoxColliders = gameObject.GetComponentsInChildren<BoxCollider>();
            _state = PlaceableObjectState.Picked;
        }

        private void Update()
        {
            IsMoving = !(Vector3.Distance(_previousPosition, transform.position) < MinimalSpeed);
            _previousPosition = transform.position;

            if (!Input.GetMouseButtonDown(0) && !IsOverlappingCell)
                return;

            print("Mouse Button is Down");
            
            if (_state == PlaceableObjectState.Picked)
                _state = PlaceableObjectState.Placed;
                
            OnStateChanged?.Invoke(_state);
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