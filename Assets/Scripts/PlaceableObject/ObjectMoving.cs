using GameBoard;
using Reflex.Attributes;
using UnityEngine;
using Utils;

namespace PlaceableObject
{
    public class ObjectMoving : MonoBehaviour
    {
        private Camera _camera;
        [Inject] 
        private CursorPlane _cursorPlane;
        [Inject] 
        private ObjectSelection _objectSelection;
        private PlaceableObject _placeableObject;

        private const float MoveSpeed = 50f;
        private const float MinimalSpeed = 0.01f;
        public bool IsMoving { get; private set; }
        private Vector3 _previousPosition;

        private void Awake()
        {
            _objectSelection.OnStateChanged += placeableObject => _placeableObject = placeableObject;
        }

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (!_placeableObject || _placeableObject.State != PlaceableObjectState.Picked)
                return;
            
            if (_placeableObject.Shape == null)
            {
                Debug.LogError("Shape is null in PlaceableObject!");
                return;
            }

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

            var bounds = _placeableObject.Shape.GetBounds();
            
            if (bounds.size.x % 2 == 0)
                xOffset = 0.5f;
            if (bounds.size.y % 2 == 0)
                yOffset = 0.5f;
            if (bounds.size.z % 2 == 0)
                zOffset = 0.5f;

            var x = Mathf.RoundToInt(mousePositionOnPlane.x) + xOffset;
            var z = Mathf.RoundToInt(mousePositionOnPlane.z) + zOffset;
            return new Vector3(x, _cursorPlane.currentLayer + yOffset, z);
        }

        private void Move(Vector3 targetPosition)
        {
            _placeableObject.transform.position = Vector3.MoveTowards(_placeableObject.transform.position, targetPosition, MoveSpeed * Time.deltaTime);
            
            // Обновляем текущую позицию объекта в его координатах
            var currentPos = _placeableObject.transform.position;
            _placeableObject.CurrentPosition = new Vector3Int(
                Mathf.RoundToInt(currentPos.x),
                Mathf.RoundToInt(currentPos.y),
                Mathf.RoundToInt(currentPos.z)
            );
        }

        private void CheckMoving()
        {
            IsMoving = !(Vector3.Distance(_previousPosition, _placeableObject.transform.position) < MinimalSpeed);
            _previousPosition = _placeableObject.transform.position;
        }
    }
}
