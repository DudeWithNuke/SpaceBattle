using Reflex.Attributes;
using UnityEngine;

namespace PlaceableObject
{
    public class ObjectMoving : MonoBehaviour
    {
        [Inject] private ObjectSelection _objectSelection;

        private const float MinimalSpeed = 0.01f;
        public bool IsMoving { get; private set; }

        private PlaceableObject _placeableObject;
        private Vector3 _previousPosition;

        private void Awake()
        {
            _objectSelection.OnStateChanged += placeableObject =>
            {
                _placeableObject = placeableObject;
                _previousPosition = placeableObject ? placeableObject.transform.position : Vector3.zero;
                IsMoving = false;
            };
        }

        private void Update()
        {
            if (!_placeableObject || _placeableObject.State != PlaceableObjectState.Picked)
            {
                IsMoving = false;
                return;
            }

            var currentPosition = _placeableObject.transform.position;
            IsMoving = Vector3.Distance(_previousPosition, currentPosition) >= MinimalSpeed;
            _previousPosition = currentPosition;
        }
    }
}
