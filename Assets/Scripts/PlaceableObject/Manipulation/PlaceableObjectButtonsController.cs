using System;
using System.Collections.Generic;
using PlayerCamera;
using Reflex.Attributes;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlaceableObject.Manipulation
{
    public class PlaceableObjectButtonsController : MonoBehaviour
    {
        public event Action<PlaceableObject> OnObjectSpawned;

        [Inject] private ShipRoster _shipRoster;
        [Inject] private CameraMovement _cameraMovement;

        private readonly List<ShipButton> _shipButtons = new();
        private readonly Dictionary<PlaceableObject, ShipButton> _objectToButton = new();
        private readonly List<ButtonBinding> _buttonBindings = new();

        private Transform _buttonPanel;
        private ShipButton _buttonPrefab;
        private PlaceableObject _currentSelectedPlaceableObject;
        private bool _isPlayerBattlefieldActive = true;
        private bool _isInitialized;

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

        public void Initialize(Transform buttonPanel, ShipButton buttonPrefab)
        {
            if (_isInitialized)
                return;

            _buttonPanel = buttonPanel;
            _buttonPrefab = buttonPrefab;
            EnsureUiInteractionReady();
            CreateButtons();
            ShipButton.OnObjectSpawned += HandleObjectSpawned;

            if (_cameraMovement != null)
            {
                _cameraMovement.OnBattlefieldSideChanged += HandleBattlefieldSideChanged;
                _isPlayerBattlefieldActive = _cameraMovement.IsPlayerBattlefieldActive;
            }

            RefreshButtonsForBattlefield(_isPlayerBattlefieldActive);
            _isInitialized = true;
        }

        public void HandleObjectPicked(PlaceableObject placeableObject)
        {
            _currentSelectedPlaceableObject = placeableObject;
            DisableAllButtonsExcept(placeableObject);
        }

        public void HandleSelectionCleared()
        {
            _currentSelectedPlaceableObject = null;
            EnableAllButtons();
        }

        public void UnregisterSpawnedObject(PlaceableObject placeableObject)
        {
            _objectToButton.Remove(placeableObject);
        }

        private void OnDestroy()
        {
            ShipButton.OnObjectSpawned -= HandleObjectSpawned;
            if (_cameraMovement != null)
                _cameraMovement.OnBattlefieldSideChanged -= HandleBattlefieldSideChanged;
        }

        private void HandleObjectSpawned(ShipButton sourceButton, PlaceableObject placeableObject)
        {
            _objectToButton[placeableObject] = sourceButton;
            OnObjectSpawned?.Invoke(placeableObject);
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
                _shipButtons.Add(shipButton);
                _buttonBindings.Add(new ButtonBinding(placeableObject, shipButton));
            }
        }

        private void HandleBattlefieldSideChanged(bool isPlayerBattlefieldActive)
        {
            _isPlayerBattlefieldActive = isPlayerBattlefieldActive;
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
    }
}
