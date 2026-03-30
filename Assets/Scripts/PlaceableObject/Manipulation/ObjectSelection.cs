using System;
using System.Collections.Generic;
using GameBoard;
using UI;
using UnityEngine;
using Reflex.Attributes;

namespace PlaceableObject.Manipulation
{
    [RequireComponent(typeof(PlaceableObjectButtonsController))]
    [RequireComponent(typeof(PlaceableObjectPicker))]
    public class ObjectSelection : MonoBehaviour
    {
        public event Action<PlaceableObject> OnStateChanged;

        [SerializeField] private Transform buttonPanel;
        [SerializeField] private ShipButton buttonPrefab;

        [Inject] private ObjectMoving _objectMoving;
        [Inject] private CursorPlane _cursorPlane;

        public PlaceableObject CurrentPickedObject { get; private set; }
        private readonly HashSet<PlaceableObject> _trackedObjects = new();
        private PlaceableObjectButtonsController _buttonsController;
        private PlaceableObjectPicker _picker;

        private void Awake()
        {
            _buttonsController = GetComponent<PlaceableObjectButtonsController>();
            _picker = GetComponent<PlaceableObjectPicker>();

            _buttonsController.Initialize(buttonPanel, buttonPrefab);
            _buttonsController.OnObjectSpawned += OnObjectSpawned;
        }

        private void OnDestroy()
        {
            if (_buttonsController != null)
                _buttonsController.OnObjectSpawned -= OnObjectSpawned;

            foreach (var placeableObject in _trackedObjects)
            {
                if (!placeableObject)
                    continue;

                placeableObject.OnPicked -= OnPicked;
                placeableObject.OnPlaced -= OnPlaced;
                placeableObject.OnDestroyed -= OnPlaceableObjectDestroyed;
            }
        }

        private void OnObjectSpawned(PlaceableObject placeableObject)
        {
            _trackedObjects.Add(placeableObject);

            placeableObject.OnPicked += OnPicked;
            placeableObject.OnPlaced += OnPlaced;
            placeableObject.OnDestroyed += OnPlaceableObjectDestroyed;

            OnPicked(placeableObject);
        }

        private void OnPicked(PlaceableObject placeableObject)
        {
            CurrentPickedObject = placeableObject;
            _buttonsController.HandleObjectPicked(placeableObject);
            OnStateChanged?.Invoke(placeableObject);
        }

        private void OnPlaced(PlaceableObject placeableObject)
        {
            if (CurrentPickedObject == placeableObject)
                CurrentPickedObject = null;

            _buttonsController.HandleSelectionCleared();
            OnStateChanged?.Invoke(null);
        }

        private void OnPlaceableObjectDestroyed(PlaceableObject placeableObject)
        {
            placeableObject.OnPicked -= OnPicked;
            placeableObject.OnPlaced -= OnPlaced;
            placeableObject.OnDestroyed -= OnPlaceableObjectDestroyed;

            _trackedObjects.Remove(placeableObject);
            _buttonsController.UnregisterSpawnedObject(placeableObject);

            if (CurrentPickedObject == placeableObject)
            {
                CurrentPickedObject = null;
                _buttonsController.HandleSelectionCleared();
                OnStateChanged?.Invoke(null);
            }
        }

        public bool TryPlaceCurrent()
        {
            if (!CurrentPickedObject)
                return false;
            if (_cursorPlane.IsTransitioning)
                return false;
            if (_objectMoving.IsMoving)
                return false;
            if (!_objectMoving.IsAtTargetPosition())
                return false;
            if (!_objectMoving.HasValidTarget)
                return false;
            
            return CurrentPickedObject.TryPlace();
        }

        public bool TryPickClosest(Ray ray)
        {
            if (CurrentPickedObject)
                return false;

            return _picker != null && _picker.TryPickClosest(ray);
        }

        public bool DestroyCurrentPickedObject()
        {
            if (!CurrentPickedObject)
                return false;

            Destroy(CurrentPickedObject.gameObject);
            CurrentPickedObject = null;
            return true;
        }
    }
}
