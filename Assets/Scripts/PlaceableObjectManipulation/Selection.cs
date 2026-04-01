using System;
using GameBoard;
using PlaceableObjectManipulation.Interaction;
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
        [SerializeField] private ButtonsController buttonsController;
        [SerializeField] private SpawnedObjectLifecycleTracker lifecycleTracker;

        [Inject] private Moving _moving;
        [Inject] private CursorPlane _cursorPlane;

        public PlaceableObject.PlaceableObject CurrentPickedObject { get; private set; }
        private Picking _picker;

        private void Awake()
        {
            _picker = new Picking();
            lifecycleTracker.OnPicked += OnPicked;
            lifecycleTracker.OnPlaced += OnPlaced;
            lifecycleTracker.OnDestroyed += OnPlaceableObjectDestroyed;

            buttonsController.Initialize(buttonPanel, buttonPrefab);
            buttonsController.OnObjectSpawned += OnObjectSpawned;
            
            listSwitch.OnValueChanged += HandleListSwitchChanged;
            HandleListSwitchChanged(listSwitch.IsOn);
        }

        private void OnDestroy()
        {
            buttonsController.OnObjectSpawned -= OnObjectSpawned;
            lifecycleTracker.OnPicked -= OnPicked;
            lifecycleTracker.OnPlaced -= OnPlaced;
            lifecycleTracker.OnDestroyed -= OnPlaceableObjectDestroyed;
            listSwitch.OnValueChanged -= HandleListSwitchChanged;
        }

        private void OnObjectSpawned(PlaceableObject.PlaceableObject placeableObject)
        {
            lifecycleTracker.Register(placeableObject);

            if (CurrentPickedObject != placeableObject)
                OnPicked(placeableObject);
        }

        private void HandleListSwitchChanged(bool isOn)
        {
            var showPlacedObjects = listSwitchOnShowsPlaced ? isOn : !isOn;
            buttonsController.SetShowPlacedObjects(showPlacedObjects);
        }

        private void OnPicked(PlaceableObject.PlaceableObject placeableObject)
        {
            CurrentPickedObject = placeableObject;
            buttonsController.HandleObjectPicked(placeableObject);
            OnStateChanged?.Invoke(placeableObject);
        }

        private void OnPlaced(PlaceableObject.PlaceableObject placeableObject)
        {
            if (CurrentPickedObject == placeableObject)
                CurrentPickedObject = null;

            buttonsController.HandleSelectionCleared();
            OnStateChanged?.Invoke(null);
        }

        private void OnPlaceableObjectDestroyed(PlaceableObject.PlaceableObject placeableObject)
        {
            if (CurrentPickedObject != placeableObject)
                return;
            
            CurrentPickedObject = null;
            buttonsController.HandleSelectionCleared();
            OnStateChanged?.Invoke(null);
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
            
            return CurrentPickedObject.TryPlace();
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
