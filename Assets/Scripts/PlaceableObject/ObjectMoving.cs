using GameBoard;
using UnityEngine;
using Utils;

namespace PlaceableObject
{
    public class ObjectMoving : MonoBehaviour
    {
        //todo структурировать в инспекторе
        
        private const float MoveSpeed = 30f;

        [SerializeField] private PlaceableObject placeableObject;
        private CursorPlane _cursorPlane;
        private Camera _camera;

        public void Initialize(CursorPlane cursorPlane)
        {
            _camera = Camera.main;
            _cursorPlane = cursorPlane;
        }

        private void Update()
        {
            if (placeableObject._state == PlaceableObjectState.Placed)
                return;
            
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!_cursorPlane.Plane.Raycast(ray, out var distance))
                return;
            
            var targetPosition = CalculateCursorTargetPosition(ray.GetPoint(distance));
            MoveCursor(targetPosition);
        }

        private Vector3 CalculateCursorTargetPosition(Vector3 mousePositionOnPlane)
        {
            var xOffset = 0f;
            var yOffset = 0f;
            var zOffset = 0f;

            foreach (var objectCollider in placeableObject.BoxColliders)
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
            return placeableObject.IsOverlappingCell
                ? Mathf.RoundToInt(value) + offset
                : value;
        }

        private void MoveCursor(Vector3 targetPosition)
        {
            if (placeableObject.transform.position == targetPosition)
                return;

            placeableObject.transform.position = placeableObject.IsOverlappingCell
                ? Vector3.MoveTowards(placeableObject.transform.position, targetPosition, MoveSpeed * Time.deltaTime)
                : targetPosition;
        }
    }
}