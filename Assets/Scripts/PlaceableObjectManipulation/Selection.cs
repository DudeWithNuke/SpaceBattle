using System;
using System.Collections.Generic;
using GameBoard;
using Reflex.Attributes;
using UI;
using UI.Animation;
using UnityEngine;

namespace PlaceableObjectManipulation
{
    [RequireComponent(typeof(ButtonsController))]
    public class Selection : MonoBehaviour
    {
        public event Action<PlaceableObject.PlaceableObject> OnStateChanged;

        [SerializeField] private Transform buttonPanel;
        [SerializeField] private ShipButton buttonPrefab;
        [SerializeField] private ListSwitch listSwitch;
        [SerializeField] private bool listSwitchOnShowsPlaced = true;

        [Inject] private Moving _moving;
        [Inject] private CursorPlane _cursorPlane;
        [Inject] private StateCoordinator _stateCoordinator;

        public PlaceableObject.PlaceableObject CurrentPickedObject { get; private set; }
        private readonly HashSet<PlaceableObject.PlaceableObject> _trackedObjects = new();
        private ButtonsController _buttonsController;
        private Picking _picker;

        private void Awake()
        {
            _picker = new Picking(_stateCoordinator);

            _buttonsController = GetComponent<ButtonsController>();

            _buttonsController.Initialize(buttonPanel, buttonPrefab);
            _buttonsController.OnObjectSpawned += OnObjectSpawned;

            if (listSwitch != null)
            {
                listSwitch.OnValueChanged += HandleListSwitchChanged;
                HandleListSwitchChanged(listSwitch.IsOn);
            }
        }

        private void OnDestroy()
        {
            if (_buttonsController != null)
                _buttonsController.OnObjectSpawned -= OnObjectSpawned;
            
            if (listSwitch != null)
                listSwitch.OnValueChanged -= HandleListSwitchChanged;

            foreach (var placeableObject in _trackedObjects)
            {
                if (!placeableObject)
                    continue;

                placeableObject.OnPicked -= OnPicked;
                placeableObject.OnPlaced -= OnPlaced;
                placeableObject.OnDestroyed -= OnPlaceableObjectDestroyed;
            }
        }

        private void OnObjectSpawned(PlaceableObject.PlaceableObject placeableObject)
        {
            _trackedObjects.Add(placeableObject);

            placeableObject.OnPicked += OnPicked;
            placeableObject.OnPlaced += OnPlaced;
            placeableObject.OnDestroyed += OnPlaceableObjectDestroyed;

            OnPicked(placeableObject);
        }

        private void HandleListSwitchChanged(bool isOn)
        {
            if (_buttonsController == null)
                return;

            var showPlacedObjects = listSwitchOnShowsPlaced ? isOn : !isOn;
            _buttonsController.SetShowPlacedObjects(showPlacedObjects);
        }

        private void OnPicked(PlaceableObject.PlaceableObject placeableObject)
        {
            CurrentPickedObject = placeableObject;
            _buttonsController.HandleObjectPicked(placeableObject);
            OnStateChanged?.Invoke(placeableObject);
        }

        private void OnPlaced(PlaceableObject.PlaceableObject placeableObject)
        {
            if (CurrentPickedObject == placeableObject)
                CurrentPickedObject = null;

            _buttonsController.HandleSelectionCleared();
            OnStateChanged?.Invoke(null);
        }

        private void OnPlaceableObjectDestroyed(PlaceableObject.PlaceableObject placeableObject)
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
            if (_moving.IsMoving)
                return false;
            if (!_moving.IsAtTargetPosition())
                return false;
            if (!_moving.HasValidTarget)
                return false;
            
            return _stateCoordinator != null && _stateCoordinator.TryPlace(CurrentPickedObject);
        }

        public bool TryPickClosest(Ray ray)
        {
            return !CurrentPickedObject && _picker.TryPickClosest(ray);
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
