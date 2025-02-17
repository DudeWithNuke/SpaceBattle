using GameBoard;
using ModestTree;
using UnityEngine;
using Utils;

namespace PlaceableObject
{
    public class ObjectMoving : CustomMonoBehaviour<ObjectMoving>
    {
        private Camera _camera;
        private CursorPlane _cursorPlane;
        private PlaceableObject _placeableObject;
        
        private const float MoveSpeed = 30f;
        private const float MinimalSpeed = 0.01f;
        public bool IsMoving { get; private set; }
        private Vector3 _previousPosition;
        
        private void Awake()
        {
            Subscribe<CursorPlane>(cursorPlane => _cursorPlane = cursorPlane);
            Subscribe<ObjectSelection>(objectSelection => objectSelection.OnObjectPicked += OnPlaceableObjectPicked);
        }

        private void OnPlaceableObjectPicked(PlaceableObject newPlaceableObject)
        {
            _placeableObject = newPlaceableObject;
            Log.Info($"Object {_placeableObject.name} picked");
        }

        protected override void SetUp()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if(!_placeableObject)
                return;
            
            if (_placeableObject.State != PlaceableObjectState.Picked)
            {
                IsMoving = false;
                return;
            }
            IsMoving = true;
            
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!_cursorPlane.Plane.Raycast(ray, out var distance))
                return;
            
            var targetPosition = CalculateCursorTargetPosition(ray.GetPoint(distance));
            Move(targetPosition);
            CheckMoving();
        }

        private Vector3 CalculateCursorTargetPosition(Vector3 mousePositionOnPlane)
        {
            var xOffset = 0f;
            var yOffset = 0f;
            var zOffset = 0f;
            
            foreach (var objectCollider in _placeableObject.BoxColliders)
            {
                if (MathUtils.IsGreaterThanOdd(objectCollider.size.x))
                    xOffset = 0.5f;
                if (MathUtils.IsGreaterThanOdd(objectCollider.size.y))
                    yOffset = 0.5f;
                if (MathUtils.IsGreaterThanOdd(objectCollider.size.z))
                    zOffset = 0.5f;
            }

            var x = CalculatePlanarComponent(mousePositionOnPlane.x, xOffset);
            var z = CalculatePlanarComponent(mousePositionOnPlane.z, zOffset);
            return new Vector3(x, _cursorPlane.currentLayer + yOffset, z);
        }

        private float CalculatePlanarComponent(float value, float offset)
        {
            return _placeableObject.IsOverlappingCell
                ? Mathf.RoundToInt(value) + offset
                : value;
        }

        private void Move(Vector3 targetPosition)
        {
            _placeableObject.transform.position = _placeableObject.IsOverlappingCell
                ? Vector3.MoveTowards(_placeableObject.transform.position, targetPosition, MoveSpeed * Time.deltaTime)
                : targetPosition;
        }
        
        private void CheckMoving()
        {
            IsMoving = !(Vector3.Distance(_previousPosition, _placeableObject.transform.position) < MinimalSpeed);
            _previousPosition = _placeableObject.transform.position;
        }
    }
}