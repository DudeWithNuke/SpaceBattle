using System;
using GameBoard;
using Reflex.Attributes;
using UnityEngine;

namespace PlaceableObject
{
    public abstract class PlaceableObject : MonoBehaviour
    { 
        public event Action<PlaceableObject> OnPlaced;
        public event Action<PlaceableObject> OnPicked;
      
        private Camera _camera;
        
        public PlaceableObjectState State { get; private set; }
        public PlaceableObjectShape Shape { get; private set; }
        public Vector3Int CurrentPosition { get; set; }
        
        [Inject] private CellGrid _cellGrid;
        
        private void Awake()
        {
            Shape = ScriptableObject.CreateInstance<PlaceableObjectShape>();
            DefineShape(Shape);
        }
        
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
            var occupiedCells = Shape.GetOccupiedCells(CurrentPosition);
            
            foreach (var cellPos in occupiedCells)
                if (cellPos.x < 0 || cellPos.x >= _cellGrid.GridSize.x ||
                    cellPos.y < 0 || cellPos.y >= _cellGrid.GridSize.y ||
                    cellPos.z < 0 || cellPos.z >= _cellGrid.GridSize.z)
                    return false;
            
            return true;
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
        
        protected abstract void DefineShape(PlaceableObjectShape shape);
    }
}
