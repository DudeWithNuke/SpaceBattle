﻿using System;
using System.Collections.Generic;
using PlaceableObject;
using PlayerCamera;
using Reflex.Attributes;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlaceableObjectManipulation
{
    [RequireComponent(typeof(Spawning))]
    public class ButtonsController : MonoBehaviour
    {
        public event Action<PlaceableObject.PlaceableObject> OnObjectSpawned;

        [Inject] private ShipRoster _shipRoster;
        [Inject] private CameraMovement _cameraMovement;
        [Inject] private StateCoordinator _stateCoordinator;

        private readonly List<ShipButton> _shipButtons = new();
        private readonly Dictionary<PlaceableObject.PlaceableObject, ShipButton> _objectToButton = new();
        private readonly List<ButtonBinding> _buttonBindings = new();

        private Transform _buttonPanel;
        private ShipButton _buttonPrefab;
        private PlaceableObject.PlaceableObject _currentSelectedPlaceableObject;
        private bool _isPlayerBattlefieldActive = true;
        private bool _showPlacedObjects;
        private bool _isInitialized;
        private Spawning _spawner;

        private readonly struct ButtonBinding
        {
            public readonly PlaceableObject.PlaceableObject Prefab;
            public readonly ShipButton Button;

            public ButtonBinding(PlaceableObject.PlaceableObject prefab, ShipButton button)
            {
                Prefab = prefab;
                Button = button;
            }
        }

        public void Initialize(Transform buttonPanel, ShipButton buttonPrefab)
        {
            if (_isInitialized)
                return;

            _buttonPanel = buttonPanel;
            _buttonPrefab = buttonPrefab;
            _spawner = GetComponent<Spawning>();

            EnsureUiInteractionReady();
            CreateButtons();

            if (_cameraMovement != null)
            {
                _cameraMovement.OnBattlefieldSideChanged += HandleBattlefieldSideChanged;
                _isPlayerBattlefieldActive = _cameraMovement.IsPlayerBattlefieldActive;
            }

            RefreshButtonsForBattlefield(_isPlayerBattlefieldActive);
            _isInitialized = true;
        }

        public void HandleObjectPicked(PlaceableObject.PlaceableObject placeableObject)
        {
            _currentSelectedPlaceableObject = placeableObject;
            DisableAllButtonsExcept(placeableObject);
        }

        public void HandleSelectionCleared()
        {
            _currentSelectedPlaceableObject = null;
            EnableAllButtons();
        }

        public void SetShowPlacedObjects(bool showPlacedObjects)
        {
            if (_showPlacedObjects == showPlacedObjects)
                return;

            _showPlacedObjects = showPlacedObjects;
            RefreshButtonsForBattlefield(_isPlayerBattlefieldActive);
        }

        public void UnregisterSpawnedObject(PlaceableObject.PlaceableObject placeableObject)
        {
            UnsubscribeInstance(placeableObject);
            _objectToButton.Remove(placeableObject);
            RefreshButtonsForBattlefield(_isPlayerBattlefieldActive);
        }

        private void OnDestroy()
        {
            if (_cameraMovement != null)
                _cameraMovement.OnBattlefieldSideChanged -= HandleBattlefieldSideChanged;

            foreach (var placeableObject in _objectToButton.Keys)
                UnsubscribeInstance(placeableObject);

            foreach (var shipButton in _shipButtons)
                UnsubscribeButton(shipButton);
        }

        private void EnsureUiInteractionReady()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystemObject = new GameObject("EventSystem");
                eventSystemObject.AddComponent<EventSystem>();
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }

            if (_buttonPanel == null)
                return;

            var canvas = _buttonPanel.GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            var scale = canvas.transform.localScale;
            if (Mathf.Approximately(scale.x, 0f) &&
                Mathf.Approximately(scale.y, 0f) &&
                Mathf.Approximately(scale.z, 0f))
                canvas.transform.localScale = Vector3.one;
        }

        private void CreateButtons()
        {
            if (_shipRoster == null || _buttonPanel == null || _buttonPrefab == null)
                return;

            foreach (var placeableObject in _shipRoster.GetAll())
            {
                if (!placeableObject)
                    continue;

                var shipButton = Instantiate(_buttonPrefab, _buttonPanel);
                shipButton.Initialize(placeableObject);
                SubscribeButton(shipButton);

                _shipButtons.Add(shipButton);
                _buttonBindings.Add(new ButtonBinding(placeableObject, shipButton));
            }
        }

        private void SubscribeButton(ShipButton shipButton)
        {
            if (shipButton == null)
                return;

            shipButton.OnSpawnRequested += HandleSpawnRequested;
            shipButton.OnPickRequested += HandlePickRequested;
        }

        private void UnsubscribeButton(ShipButton shipButton)
        {
            if (shipButton == null)
                return;

            shipButton.OnSpawnRequested -= HandleSpawnRequested;
            shipButton.OnPickRequested -= HandlePickRequested;
        }

        private void HandleSpawnRequested(ShipButton sourceButton, PlaceableObject.PlaceableObject prefab)
        {
            if (sourceButton == null || prefab == null || _spawner == null || _stateCoordinator == null)
                return;

            var instance = _spawner.Spawn(prefab);
            if (!instance)
                return;

            sourceButton.BindInstance(instance);
            _objectToButton[instance] = sourceButton;
            SubscribeInstance(instance);
            _stateCoordinator.TryTakeFromStorage(instance);
            OnObjectSpawned?.Invoke(instance);
        }

        private void HandlePickRequested(ShipButton _, PlaceableObject.PlaceableObject instance)
        {
            if (_stateCoordinator == null || !instance)
                return;

            _stateCoordinator.TryPick(instance);
        }

        private void HandleBattlefieldSideChanged(bool isPlayerBattlefieldActive)
        {
            _isPlayerBattlefieldActive = isPlayerBattlefieldActive;
            RefreshButtonsForBattlefield(isPlayerBattlefieldActive);
        }

        private void RefreshButtonsForBattlefield(bool isPlayerBattlefieldActive)
        {
            foreach (var binding in _buttonBindings)
                binding.Button.gameObject.SetActive(IsButtonVisible(binding, isPlayerBattlefieldActive));

            if (_currentSelectedPlaceableObject &&
                _objectToButton.TryGetValue(_currentSelectedPlaceableObject, out var sourceButton) &&
                sourceButton.gameObject.activeSelf)
            {
                DisableAllButtonsExcept(_currentSelectedPlaceableObject);
                return;
            }

            EnableAllButtons();
        }

        private static bool IsPrefabAllowedForBattlefield(PlaceableObject.PlaceableObject placeableObject, bool isPlayerBattlefieldActive)
        {
            return isPlayerBattlefieldActive
                ? placeableObject.AllowedDeploymentSide == PlaceableObjectDeploymentSide.OwnField
                : placeableObject.AllowedDeploymentSide == PlaceableObjectDeploymentSide.EnemyField;
        }

        private bool IsButtonVisible(ButtonBinding binding, bool isPlayerBattlefieldActive)
        {
            if (!IsPrefabAllowedForBattlefield(binding.Prefab, isPlayerBattlefieldActive))
                return false;

            return _showPlacedObjects ? IsPlaced(binding.Button.Instance) : !IsPlaced(binding.Button.Instance);
        }

        private static bool IsPlaced(PlaceableObject.PlaceableObject placeableObject)
        {
            return placeableObject && placeableObject.State == PlaceableObjectState.Placed;
        }

        private void SubscribeInstance(PlaceableObject.PlaceableObject placeableObject)
        {
            if (!placeableObject)
                return;

            placeableObject.OnPlaced += HandleInstanceStateChanged;
            placeableObject.OnPicked += HandleInstanceStateChanged;
            placeableObject.OnDestroyed += HandleInstanceDestroyed;
        }

        private void UnsubscribeInstance(PlaceableObject.PlaceableObject placeableObject)
        {
            if (!placeableObject)
                return;

            placeableObject.OnPlaced -= HandleInstanceStateChanged;
            placeableObject.OnPicked -= HandleInstanceStateChanged;
            placeableObject.OnDestroyed -= HandleInstanceDestroyed;
        }

        private void HandleInstanceStateChanged(PlaceableObject.PlaceableObject _)
        {
            RefreshButtonsForBattlefield(_isPlayerBattlefieldActive);
        }

        private void HandleInstanceDestroyed(PlaceableObject.PlaceableObject placeableObject)
        {
            UnsubscribeInstance(placeableObject);
            RefreshButtonsForBattlefield(_isPlayerBattlefieldActive);
        }

        private void DisableAllButtonsExcept(PlaceableObject.PlaceableObject placeableObject)
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
    }
}
