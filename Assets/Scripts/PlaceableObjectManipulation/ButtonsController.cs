﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using PlaceableObject;
using PlayerCamera;
using Reflex.Attributes;
using UI;
using UnityEngine;

namespace PlaceableObjectManipulation
{
    [RequireComponent(typeof(Spawning))]
    public class ButtonsController : MonoBehaviour
    {
        public event Action<PlaceableObject.PlaceableObject> OnObjectSpawned;

        [Inject] private ShipRoster _shipRoster;
        [Inject] private CameraMovement _cameraMovement;

        [SerializeField] private Spawning spawning;
        [SerializeField] private SpawnedObjectLifecycleTracker lifecycleTracker;
        [SerializeField] private Canvas buttonCanvas;

        private readonly List<ShipButton> _shipButtons = new();
        private readonly Dictionary<PlaceableObject.PlaceableObject, ShipButton> _objectToButton = new();
        private readonly List<ButtonBinding> _buttonBindings = new();

        private Transform _buttonPanel;
        private ShipButton _buttonPrefab;
        private PlaceableObject.PlaceableObject _currentSelectedPlaceableObject;
        private bool _isPlayerBattlefieldActive = true;
        private bool _showPlacedObjects;
        private bool _isInitialized;

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
            lifecycleTracker.OnPicked += HandleInstanceStateChanged;
            lifecycleTracker.OnPlaced += HandleInstanceStateChanged;
            lifecycleTracker.OnDestroyed += HandleInstanceDestroyed;

            EnsureUiInteractionReady();
            CreateButtons();

            _cameraMovement.OnBattlefieldSideChanged += HandleBattlefieldSideChanged;
            _isPlayerBattlefieldActive = _cameraMovement.IsPlayerBattlefieldActive;

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

        private void OnDestroy()
        {
            _cameraMovement.OnBattlefieldSideChanged -= HandleBattlefieldSideChanged;
            lifecycleTracker.OnPicked -= HandleInstanceStateChanged;
            lifecycleTracker.OnPlaced -= HandleInstanceStateChanged;
            lifecycleTracker.OnDestroyed -= HandleInstanceDestroyed;

            foreach (var shipButton in _shipButtons)
                UnsubscribeButton(shipButton);
        }

        private void EnsureUiInteractionReady()
        {
            var scale = buttonCanvas.transform.localScale;
            if (Mathf.Approximately(scale.x, 0f) &&
                Mathf.Approximately(scale.y, 0f) &&
                Mathf.Approximately(scale.z, 0f))
                buttonCanvas.transform.localScale = Vector3.one;
        }

        private void CreateButtons()
        {
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
            if (sourceButton == null || prefab == null)
                return;

            var instance = spawning.Spawn(prefab);
            if (!instance)
                return;

            sourceButton.BindInstance(instance);
            _objectToButton[instance] = sourceButton;
            lifecycleTracker.Register(instance);

            instance.TryTakeFromStorage();
            OnObjectSpawned?.Invoke(instance);
        }

        private static void HandlePickRequested(ShipButton _, PlaceableObject.PlaceableObject instance)
        {
            if (!instance)
                return;

            instance.TryPick();
        }

        private void HandleBattlefieldSideChanged(bool isPlayerBattlefieldActive)
        {
            _isPlayerBattlefieldActive = isPlayerBattlefieldActive;
            RefreshButtonsForBattlefield(isPlayerBattlefieldActive);
        }

        private void RefreshButtonsForBattlefield(bool isPlayerBattlefieldActive)
        {
            foreach (var binding in _buttonBindings)
            {
                var button = binding.Button;
                if (!button)
                    continue;

                button.gameObject.SetActive(IsButtonVisible(binding, isPlayerBattlefieldActive));
            }

            if (_currentSelectedPlaceableObject && _objectToButton.TryGetValue(_currentSelectedPlaceableObject, out var sourceButton) &&
                sourceButton && sourceButton.gameObject.activeSelf)
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

        private void HandleInstanceStateChanged(PlaceableObject.PlaceableObject _)
        {
            RefreshButtonsForBattlefield(_isPlayerBattlefieldActive);
        }

        private void HandleInstanceDestroyed(PlaceableObject.PlaceableObject placeableObject)
        {
            _objectToButton.Remove(placeableObject);
            RefreshButtonsForBattlefield(_isPlayerBattlefieldActive);
        }

        private void DisableAllButtonsExcept(PlaceableObject.PlaceableObject placeableObject)
        {
            foreach (var shipButton in _shipButtons.Where(shipButton => shipButton))
                shipButton.DisableInteraction();
            

            if (_objectToButton.TryGetValue(placeableObject, out var sourceButton) && sourceButton)
                sourceButton.DisableInteraction();
        }

        private void EnableAllButtons()
        {
            foreach (var shipButton in _shipButtons.Where(shipButton => shipButton))
                shipButton.EnableInteraction();
        }
    }
}
