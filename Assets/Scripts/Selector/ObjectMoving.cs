using System.Collections.Generic;
using System.Linq;
using GameBoard;
using ModestTree;
using UnityEngine;
using Utils;

namespace Selector
{
    public class ObjectMoving
    {
        private const float MoveSpeed = 30f;
        
        private readonly Collider[] _cellColliders;
        private readonly CursorPlane _cursorPlane;
        private readonly PlaceableObject.PlaceableObject _placeableObject;
        private readonly Camera _camera;
        
        private bool _isOverlappingCell;

        public ObjectMoving(List<Cell> cells, CursorPlane cursorPlane, PlaceableObject.PlaceableObject placeableObject)
        {
            _camera = Camera.main;

            _cellColliders = cells
                .Select(obj => obj.GetComponent<BoxCollider>())
                .ToArray<Collider>();

            _cursorPlane = cursorPlane;
            _placeableObject = placeableObject;
        }

        public void Run()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!_cursorPlane.Plane.Raycast(ray, out var distance))
                return;

            _isOverlappingCell = !_placeableObject.GetOverlappingCells(_cellColliders).IsEmpty();
            
            var targetPosition = CalculateCursorTargetPosition(ray.GetPoint(distance));
            MoveCursor(targetPosition);
        }

        private Vector3 CalculateCursorTargetPosition(Vector3 mousePositionOnPlane)
        {
            var xOffset = 0f;
            var yOffset = 0f;
            var zOffset = 0f;

            foreach (var collider in _placeableObject.BoxColliders)
            {
                if (MathUtils.IsGreaterThanOdd(collider.size.x))
                    xOffset = 0.5f;
                if (MathUtils.IsGreaterThanOdd(collider.size.y))
                    yOffset = 0.5f;
                if (MathUtils.IsGreaterThanOdd(collider.size.z))
                    zOffset = 0.5f;
            }

            var x = CalculatePlanarComponent(mousePositionOnPlane.x, xOffset);
            var z = CalculatePlanarComponent(mousePositionOnPlane.z, zOffset);
            return new Vector3(x, _cursorPlane.currentLayer + yOffset, z);
        }

        private float CalculatePlanarComponent(float value, float offset)
        {
            return _isOverlappingCell
                ? Mathf.RoundToInt(value) + offset
                : value;
        }

        private void MoveCursor(Vector3 targetPosition)
        {
            if (_placeableObject.transform.position == targetPosition)
                return;

            _placeableObject.transform.position = _isOverlappingCell
                ? Vector3.MoveTowards(_placeableObject.transform.position, targetPosition, MoveSpeed * Time.deltaTime)
                : targetPosition;
        }
    }
}