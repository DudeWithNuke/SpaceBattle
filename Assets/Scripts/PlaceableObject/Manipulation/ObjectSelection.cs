using System;
using System.Collections.Generic;
using GameBoard;
using PlayerCamera;
using Reflex.Attributes;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlaceableObject.Manipulation
{
    public class ObjectSelection : MonoBehaviour
    {
        public event Action<PlaceableObject> OnStateChanged;

        [SerializeField] public Transform buttonPanel;
        [SerializeField] public ShipButton buttonPrefab;

        [Inject] private ShipRoster _shipRoster;
        [Inject] private CameraMovement _cameraMovement;
        [Inject] private ObjectMoving _objectMoving;
        [Inject] private CursorPlane _cursorPlane;

        public PlaceableObject CurrentPickedObject { get; private set; }
        private PlaceableObject _currentSelectedPlaceableObject;

        private readonly List<ShipButton> _shipButtons = new();
        private readonly Dictionary<PlaceableObject, ShipButton> _objectToButton = new();
        private readonly List<ButtonBinding> _buttonBindings = new();
        
        private readonly struct ButtonBinding
        {
            public readonly PlaceableObject Prefab;
            public readonly ShipButton Button;

            public ButtonBinding(PlaceableObject prefab, ShipButton button)
            {
                Prefab = prefab;
                Button = button;
            }
        }

        private void Awake()
        {
            EnsureUiInteractionReady();
            CreateButtons();
            ShipButton.OnObjectSpawned += OnObjectSpawned;

            if (_cameraMovement != null)
            {
                _cameraMovement.OnBattlefieldSideChanged += HandleBattlefieldSideChanged;
                RefreshButtonsForBattlefield(_cameraMovement.IsPlayerBattlefieldActive);
            }
            else RefreshButtonsForBattlefield(true);
        }

        private void EnsureUiInteractionReady()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystemObject = new GameObject("EventSystem");
                eventSystemObject.AddComponent<EventSystem>();
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }

            if (buttonPanel == null)
                return;

            var canvas = buttonPanel.GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            var scale = canvas.transform.localScale;
            if (Mathf.Approximately(scale.x, 0f) &&
                Mathf.Approximately(scale.y, 0f) &&
                Mathf.Approximately(scale.z, 0f))
                canvas.transform.localScale = Vector3.one;
        }

        private void OnDestroy()
        {
            ShipButton.OnObjectSpawned -= OnObjectSpawned;
            if (_cameraMovement != null)
                _cameraMovement.OnBattlefieldSideChanged -= HandleBattlefieldSideChanged;

            foreach (var placeableObject in _objectToButton.Keys)
            {
                if (!placeableObject)
                    continue;

                placeableObject.OnPicked -= OnPicked;
                placeableObject.OnPlaced -= OnPlaced;
                placeableObject.OnDestroyed -= OnPlaceableObjectDestroyed;
            }
        }

        private void OnObjectSpawned(ShipButton sourceButton, PlaceableObject placeableObject)
        {
            _objectToButton[placeableObject] = sourceButton;

            placeableObject.OnPicked += OnPicked;
            placeableObject.OnPlaced += OnPlaced;
            placeableObject.OnDestroyed += OnPlaceableObjectDestroyed;

            OnPicked(placeableObject);
        }

        private void OnPicked(PlaceableObject placeableObject)
        {
            _currentSelectedPlaceableObject = placeableObject;
            CurrentPickedObject = placeableObject;
            DisableAllButtonsExcept(placeableObject);
            OnStateChanged?.Invoke(placeableObject);
        }

        private void OnPlaced(PlaceableObject placeableObject)
        {
            if (_currentSelectedPlaceableObject == placeableObject)
                _currentSelectedPlaceableObject = null;

            if (CurrentPickedObject == placeableObject)
                CurrentPickedObject = null;

            EnableAllButtons();
            OnStateChanged?.Invoke(null);
        }

        private void OnPlaceableObjectDestroyed(PlaceableObject placeableObject)
        {
            placeableObject.OnPicked -= OnPicked;
            placeableObject.OnPlaced -= OnPlaced;
            placeableObject.OnDestroyed -= OnPlaceableObjectDestroyed;

            _objectToButton.Remove(placeableObject);

            if (_currentSelectedPlaceableObject == placeableObject)
            {
                _currentSelectedPlaceableObject = null;
                EnableAllButtons();
                OnStateChanged?.Invoke(null);
            }

            if (CurrentPickedObject == placeableObject)
                CurrentPickedObject = null;
        }

        private void DisableAllButtonsExcept(PlaceableObject placeableObject)
        {
            foreach (var shipButton in _shipButtons)
                shipButton.DisableInteraction();

            if (_objectToButton.TryGetValue(placeableObject, out var sourceButton))
                sourceButton.DisableInteraction();
        }

        private void EnableAllButtons()
        {
            foreach (var shipButton in _shipButtons)
                shipButton.EnableInteraction();
        }

        private void CreateButtons()
        {
            if (_shipRoster == null)
                return;

            foreach (var placeableObject in _shipRoster.GetAll())
            {
                if (!placeableObject)
                    continue;

                var shipButton = Instantiate(buttonPrefab, buttonPanel);
                shipButton.Initialize(placeableObject);
                _shipButtons.Add(shipButton);
                _buttonBindings.Add(new ButtonBinding(placeableObject, shipButton));
            }
        }

        private void HandleBattlefieldSideChanged(bool isPlayerBattlefieldActive)
        {
            RefreshButtonsForBattlefield(isPlayerBattlefieldActive);
        }

        private void RefreshButtonsForBattlefield(bool isPlayerBattlefieldActive)
        {
            foreach (var binding in _buttonBindings)
                binding.Button.gameObject.SetActive(IsPrefabAllowedForBattlefield(binding.Prefab, isPlayerBattlefieldActive));

            if (_currentSelectedPlaceableObject &&
                _objectToButton.TryGetValue(_currentSelectedPlaceableObject, out var sourceButton) &&
                sourceButton.gameObject.activeSelf)
            {
                DisableAllButtonsExcept(_currentSelectedPlaceableObject);
                return;
            }

            EnableAllButtons();
        }

        private static bool IsPrefabAllowedForBattlefield(PlaceableObject placeableObject, bool isPlayerBattlefieldActive)
        {
            return isPlayerBattlefieldActive
                ? placeableObject.AllowedDeploymentSide == PlaceableObjectDeploymentSide.OwnField
                : placeableObject.AllowedDeploymentSide == PlaceableObjectDeploymentSide.EnemyField;
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
        
        private readonly RaycastHit[] _raycastResults = new RaycastHit[16];
        public bool TryPickClosest(Ray ray)
        {
            if (CurrentPickedObject)
                return false;

            var hitCount = Physics.RaycastNonAlloc(ray, _raycastResults, Mathf.Infinity);
            if (hitCount == 0)
                return false;

            PlaceableObject closestObject = null;
            var closestDistance = float.PositiveInfinity;

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _raycastResults[i];

                if (hit.distance >= closestDistance)
                    continue;
                if (!hit.collider.TryGetComponent(out PlaceableObject placeableObject)) 
                    continue;
                if (placeableObject.State != PlaceableObjectState.Placed) 
                    continue;
                closestDistance = hit.distance;
                closestObject = placeableObject;
            }

            return closestObject && closestObject.TryPick();
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
