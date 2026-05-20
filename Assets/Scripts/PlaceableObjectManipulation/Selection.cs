using System;
using GameBoard;
using PlaceableObjectManipulation.Interaction;
using PlayerCamera;
using Reflex.Attributes;
using UI.Animation;
using UI.UnitCard;
using UnityEngine;

namespace PlaceableObjectManipulation
{
    [RequireComponent(typeof(FleetPanelController))]
    public class Selection : MonoBehaviour
    {
        public event Action<PlaceableObject.PlaceableObject> OnStateChanged;

        [SerializeField] private Transform buttonPanel;
        [SerializeField] private UnitCardController buttonPrefab;
        [SerializeField] private ListSwitch listSwitch;
        [SerializeField] private bool listSwitchOnShowsPlaced = true;
        [SerializeField] private FleetPanelController fleetPanelController;
        [SerializeField] private SpawnedObjectLifecycleTracker lifecycleTracker;

        [Inject] private Moving _moving;
        [Inject] private CursorPlane _cursorPlane;
        [Inject] private CameraMovement _cameraMovement;

        public PlaceableObject.PlaceableObject CurrentPickedObject { get; private set; }
        private Picking _picker;
        private bool _isInteractionEnabled = true;
        private bool _canPickPlacedObjects = true;
        private bool _listSwitchValueBeforeEnemyField;
        private bool _isListSwitchLockedByEnemyField;

        private void Awake()
        {
            _picker = new Picking();
            lifecycleTracker.OnPicked += OnPicked;
            lifecycleTracker.OnPlaced += OnPlaced;
            lifecycleTracker.OnDestroyed += OnPlaceableObjectDestroyed;

            fleetPanelController.Initialize(buttonPanel, buttonPrefab);
            fleetPanelController.OnObjectSpawned += OnObjectSpawned;
            
            listSwitch.OnValueChanged += HandleListSwitchChanged;
            HandleListSwitchChanged(listSwitch.IsOn);
            _cameraMovement.OnBattlefieldSideChanged += HandleBattlefieldSideChanged;
            HandleBattlefieldSideChanged(_cameraMovement.IsPlayerBattlefieldActive);
        }

        private void OnDestroy()
        {
            fleetPanelController.OnObjectSpawned -= OnObjectSpawned;
            lifecycleTracker.OnPicked -= OnPicked;
            lifecycleTracker.OnPlaced -= OnPlaced;
            lifecycleTracker.OnDestroyed -= OnPlaceableObjectDestroyed;
            listSwitch.OnValueChanged -= HandleListSwitchChanged;
            _cameraMovement.OnBattlefieldSideChanged -= HandleBattlefieldSideChanged;
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
            fleetPanelController.SetShowPlacedObjects(showPlacedObjects);
        }

        private void HandleBattlefieldSideChanged(bool isPlayerBattlefieldActive)
        {
            if (!isPlayerBattlefieldActive)
            {
                _listSwitchValueBeforeEnemyField = listSwitch.IsOn;
                _isListSwitchLockedByEnemyField = true;

                listSwitch.SetValue(listSwitchOnShowsPlaced, false);
                listSwitch.SetInteractable(false);
                fleetPanelController.SetShowPlacedObjects(true);
                return;
            }

            listSwitch.SetInteractable(true);
            if (!_isListSwitchLockedByEnemyField)
                return;

            _isListSwitchLockedByEnemyField = false;
            listSwitch.SetValue(_listSwitchValueBeforeEnemyField, true);
        }

        private void OnPicked(PlaceableObject.PlaceableObject placeableObject)
        {
            CurrentPickedObject = placeableObject;
            fleetPanelController.HandleObjectPicked(placeableObject);
            OnStateChanged?.Invoke(placeableObject);
        }

        private void OnPlaced(PlaceableObject.PlaceableObject placeableObject)
        {
            if (CurrentPickedObject == placeableObject)
                CurrentPickedObject = null;

            fleetPanelController.HandleSelectionCleared();
            OnStateChanged?.Invoke(null);
        }

        private void OnPlaceableObjectDestroyed(PlaceableObject.PlaceableObject placeableObject)
        {
            if (CurrentPickedObject != placeableObject)
                return;
            
            CurrentPickedObject = null;
            fleetPanelController.HandleSelectionCleared();
            OnStateChanged?.Invoke(null);
        }

        public bool TryPlaceCurrent()
        {
            if (!_isInteractionEnabled)
                return false;
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
            if (!_isInteractionEnabled)
                return false;
            if (!_canPickPlacedObjects)
                return false;

            return !CurrentPickedObject && _picker.TryPickClosest(ray);
        }

        public bool DestroyCurrentPickedObject()
        {
            if (!_isInteractionEnabled)
                return false;
            if (!CurrentPickedObject)
                return false;

            ForceDestroyCurrentPickedObject();
            return true;
        }

        public bool ForceDestroyCurrentPickedObject()
        {
            if (!CurrentPickedObject)
                return false;

            Destroy(CurrentPickedObject.gameObject);
            CurrentPickedObject = null;
            fleetPanelController.HandleSelectionCleared();
            OnStateChanged?.Invoke(null);
            return true;
        }

        public void SetInteractionEnabled(bool isEnabled)
        {
            _isInteractionEnabled = isEnabled;
        }

        public void SetCanPickPlacedObjects(bool canPickPlacedObjects)
        {
            _canPickPlacedObjects = canPickPlacedObjects;
        }
    }
}
