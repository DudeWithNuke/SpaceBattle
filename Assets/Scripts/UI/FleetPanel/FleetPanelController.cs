using System;
using System.Collections.Generic;
using System.Linq;
using Network;
using Network.Player;
using PlaceableObject;
using PlaceableObject.Ships;
using PlayerCamera;
using Reflex.Attributes;
using UI;
using UI.FleetPanel;
using UI.UnitCard;
using UnityEngine;

namespace PlaceableObjectManipulation
{
    [RequireComponent(typeof(Spawning))]
    public class FleetPanelController : MonoBehaviour
    {
        public event Action<PlaceableObject.PlaceableObject> OnObjectSpawned;

        [Inject] private ShipRoster _shipRoster;
        [Inject] private CameraMovement _cameraMovement;
        [Inject] private PlayerContext _playerContext;

        [SerializeField] private Spawning spawning;
        [SerializeField] private SpawnedObjectLifecycleTracker lifecycleTracker;
        [SerializeField] private AbilityPlacementController abilityPlacementController;
        [SerializeField] private Canvas buttonCanvas;

        private readonly List<UnitCardController> _shipButtons = new();
        private readonly Dictionary<PlaceableObject.PlaceableObject, UnitCardController> _objectToButton = new();
        private readonly List<ButtonBinding> _buttonBindings = new();

        private Transform _buttonPanel;
        private UnitCardController _buttonPrefab;
        private PlaceableObject.PlaceableObject _currentSelectedPlaceableObject;
        private bool _isPlayerBattlefieldActive = true;
        private bool _showPlacedObjects;
        private bool _isInitialized;
        private bool _isWaitingForPlayerAssignment;
        private bool _isTurnInteractionEnabled;
        private bool _isBattlePhase;

        private readonly struct ButtonBinding
        {
            public readonly PlaceableObject.PlaceableObject Prefab;
            public readonly UnitCardController Button;

            public ButtonBinding(PlaceableObject.PlaceableObject prefab, UnitCardController button)
            {
                Prefab = prefab;
                Button = button;
            }
        }

        public void Initialize(Transform buttonPanel, UnitCardController buttonPrefab)
        {
            if (_isInitialized)
                return;

            _buttonPanel = buttonPanel;
            _buttonPrefab = buttonPrefab;
            lifecycleTracker.OnPicked += HandleInstanceStateChanged;
            lifecycleTracker.OnPlaced += HandleInstanceStateChanged;
            lifecycleTracker.OnDestroyed += HandleInstanceDestroyed;
            abilityPlacementController.OnAbilitySpawned += HandleAbilitySpawned;

            EnsureUiInteractionReady();
            _playerContext.OnLocalPlayerAssigned += HandleLocalPlayerAssigned;

            CreateButtons();

            _cameraMovement.OnBattlefieldSideChanged += HandleBattlefieldSideChanged;
            _isPlayerBattlefieldActive = _cameraMovement.IsPlayerBattlefieldActive;

            RefreshButtonsForBattlefield(_isPlayerBattlefieldActive);
            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized || !_isWaitingForPlayerAssignment)
                return;

            if (_playerContext == null || !_playerContext.HasAssignedPlayer)
                return;

            _isWaitingForPlayerAssignment = false;
            RecreateButtons();
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

        public void SetTurnInteractionEnabled(bool isEnabled)
        {
            if (_isTurnInteractionEnabled == isEnabled)
                return;

            _isTurnInteractionEnabled = isEnabled;
            RefreshButtonsForBattlefield(_isPlayerBattlefieldActive);
        }

        public void SetBattlePhase(bool isBattlePhase)
        {
            if (_isBattlePhase == isBattlePhase)
                return;

            _isBattlePhase = isBattlePhase;
            RefreshButtonsForBattlefield(_isPlayerBattlefieldActive);
        }

        private void OnDestroy()
        {
            if (_cameraMovement != null)
                _cameraMovement.OnBattlefieldSideChanged -= HandleBattlefieldSideChanged;
            if (lifecycleTracker)
            {
                lifecycleTracker.OnPicked -= HandleInstanceStateChanged;
                lifecycleTracker.OnPlaced -= HandleInstanceStateChanged;
                lifecycleTracker.OnDestroyed -= HandleInstanceDestroyed;
            }
            if (abilityPlacementController)
                abilityPlacementController.OnAbilitySpawned -= HandleAbilitySpawned;
            if (_playerContext != null)
                _playerContext.OnLocalPlayerAssigned -= HandleLocalPlayerAssigned;

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
            if (_playerContext == null || !_playerContext.HasAssignedPlayer)
            {
                _isWaitingForPlayerAssignment = true;
                Debug.Log("[FleetPanelController] Ship buttons creation deferred. Local player is not assigned yet.");
                return;
            }

            _isWaitingForPlayerAssignment = false;
            var ships = _shipRoster.GetForPlayer(_playerContext.LocalPlayerIndex);

            foreach (var placeableObject in ships)
            {
                if (!placeableObject)
                    continue;

                var shipButton = Instantiate(_buttonPrefab, _buttonPanel);
                if (placeableObject is Ship ship)
                    shipButton.Initialize(ship);
                SubscribeButton(shipButton);
                abilityPlacementController.RegisterShipButton(shipButton);

                _shipButtons.Add(shipButton);
                _buttonBindings.Add(new ButtonBinding(placeableObject, shipButton));
            }

            RefreshButtonsForBattlefield(_isPlayerBattlefieldActive);
        }

        private void RecreateButtons()
        {
            foreach (var shipButton in _shipButtons)
            {
                UnsubscribeButton(shipButton);
                if (shipButton)
                    Destroy(shipButton.gameObject);
            }

            _shipButtons.Clear();
            _objectToButton.Clear();
            _buttonBindings.Clear();
            CreateButtons();
            RefreshButtonsForBattlefield(_isPlayerBattlefieldActive);
        }

        private void SubscribeButton(UnitCardController unitCardController)
        {
            if (unitCardController == null)
                return;

            unitCardController.OnSpawnRequested += HandleSpawnRequested;
            unitCardController.OnPickRequested += HandlePickRequested;
        }

        private void UnsubscribeButton(UnitCardController unitCardController)
        {
            if (unitCardController == null)
                return;

            abilityPlacementController.UnregisterShipButton(unitCardController);
            unitCardController.OnSpawnRequested -= HandleSpawnRequested;
            unitCardController.OnPickRequested -= HandlePickRequested;
        }

        private void HandleSpawnRequested(UnitCardController sourceButton, Ship prefab)
        {
            if (sourceButton == null || prefab == null)
                return;

            var instance = spawning.Spawn(prefab);
            if (!instance)
                return;

            if (instance is Ship ship)
            {
                sourceButton.BindInstance(ship);
                _objectToButton[ship] = sourceButton;
            }
            lifecycleTracker.Register(instance);

            instance.TryTakeFromStorage();
            OnObjectSpawned?.Invoke(instance);
        }

        private static void HandlePickRequested(UnitCardController _, Ship ship)
        {
            if (!ship)
                return;

            ship.TryPick();
        }

        private void HandleBattlefieldSideChanged(bool isPlayerBattlefieldActive)
        {
            _isPlayerBattlefieldActive = isPlayerBattlefieldActive;
            RefreshButtonsForBattlefield(isPlayerBattlefieldActive);
        }

        private void HandleLocalPlayerAssigned(int _)
        {
            _isWaitingForPlayerAssignment = false;
            RecreateButtons();
        }

        private void RefreshButtonsForBattlefield(bool isPlayerBattlefieldActive)
        {
            var isInteractionBlockedByAbility = abilityPlacementController.IsShipInteractionBlockedByPlacedAbility();

            foreach (var binding in _buttonBindings)
            {
                var button = binding.Button;
                if (!button)
                    continue;

                button.SetAbilityInteractionEnabled(!isInteractionBlockedByAbility);
                button.SetTurnInteractionEnabled(_isTurnInteractionEnabled);
                button.SetBattlePhase(_isBattlePhase);
                button.SetAbilityBattlefield(isPlayerBattlefieldActive);
                button.gameObject.SetActive(IsButtonVisible(binding));
            }

            if (_currentSelectedPlaceableObject is Ship)
            {
                DisableAllButtonsExcept(_currentSelectedPlaceableObject);
                return;
            }

            EnableAllButtons();
        }

        private bool IsButtonVisible(ButtonBinding binding)
        {
            if (_showPlacedObjects)
                return IsPlaced(binding.Button.Instance);

            return !IsPlaced(binding.Button.Instance);
        }

        private static bool IsPlaced(Ship ship)
        {
            return ship && ship.State == PlaceableObjectState.Placed;
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

        private void HandleAbilitySpawned(PlaceableObject.PlaceableObject abilityInstance)
        {
            if (!abilityInstance)
                return;

            OnObjectSpawned?.Invoke(abilityInstance);
            RefreshButtonsForBattlefield(_isPlayerBattlefieldActive);
        }

        private void DisableAllButtonsExcept(PlaceableObject.PlaceableObject placeableObject)
        {
            if (placeableObject is not Ship)
                return;

            _objectToButton.TryGetValue(placeableObject, out var activeButton);

            foreach (var shipButton in _shipButtons)
            {
                if (!shipButton)
                    continue;

                shipButton.SetPickedShipInteractionBlocked(shipButton != activeButton);
            }
        }

        private void EnableAllButtons()
        {
            foreach (var shipButton in _shipButtons)
            {
                if (!shipButton)
                    continue;

                shipButton.SetPickedShipInteractionBlocked(false);
            }
        }
    }
}
