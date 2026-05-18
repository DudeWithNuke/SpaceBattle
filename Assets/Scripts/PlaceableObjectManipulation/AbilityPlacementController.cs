using System;
using System.Collections.Generic;
using PlaceableObject;
using PlaceableObject.Abilities;
using PlaceableObject.Ships;
using PlayerCamera;
using UI.UnitCard;
using UnityEngine;

namespace PlaceableObjectManipulation
{
    public class AbilityPlacementController : MonoBehaviour
    {
        public event Action<PlaceableObject.PlaceableObject> OnAbilitySpawned;

        [SerializeField] private Spawning spawning;
        [SerializeField] private SpawnedObjectLifecycleTracker lifecycleTracker;
        [SerializeField] private CameraMovement cameraMovement;

        private readonly Dictionary<PlaceableObject.PlaceableObject, PlaceableObject.PlaceableObject> _activeAbilities = new();
        private readonly Dictionary<PlaceableObject.PlaceableObject, Ability> _activeAbilityPrefabs = new();
        private readonly Dictionary<PlaceableObject.PlaceableObject, AbilityOwner> _abilityOwners = new();
        private readonly Dictionary<PlaceableObject.PlaceableObject, UnitCardController> _abilityOwnerButtons = new();
        private readonly HashSet<UnitCardController> _shipButtons = new();

        private bool _isPlayerBattlefieldActive = true;

        private readonly struct AbilityOwner
        {
            public readonly PlaceableObject.PlaceableObject ShipInstance;

            public AbilityOwner(PlaceableObject.PlaceableObject shipInstance)
            {
                ShipInstance = shipInstance;
            }
        }

        private void Awake()
        {
            lifecycleTracker.OnPicked += HandleTrackedObjectPicked;
            lifecycleTracker.OnPlaced += HandleTrackedObjectPlaced;
            lifecycleTracker.OnDestroyed += HandleTrackedObjectDestroyed;
            _isPlayerBattlefieldActive = cameraMovement.IsPlayerBattlefieldActive;
            cameraMovement.OnBattlefieldSideChanged += HandleBattlefieldSideChanged;
        }

        private void OnDestroy()
        {
            cameraMovement.OnBattlefieldSideChanged -= HandleBattlefieldSideChanged;
            lifecycleTracker.OnPicked -= HandleTrackedObjectPicked;
            lifecycleTracker.OnPlaced -= HandleTrackedObjectPlaced;
            lifecycleTracker.OnDestroyed -= HandleTrackedObjectDestroyed;
        }

        public void RegisterShipButton(UnitCardController unitCardController)
        {
            if (unitCardController == null)
                return;

            unitCardController.OnActionSelectionChanged += HandleActionSelectionChanged;
            _shipButtons.Add(unitCardController);
            unitCardController.SetAbilityBattlefield(_isPlayerBattlefieldActive);
            RefreshShipButtonsInteraction();
        }

        public void UnregisterShipButton(UnitCardController unitCardController)
        {
            if (unitCardController == null)
                return;

            unitCardController.OnActionSelectionChanged -= HandleActionSelectionChanged;
            _shipButtons.Remove(unitCardController);
        }

        public bool IsShipInteractionBlocked()
        {
            return HasPlacedAbility();
        }

        private void HandleBattlefieldSideChanged(bool isPlayerBattlefieldActive)
        {
            _isPlayerBattlefieldActive = isPlayerBattlefieldActive;
            RefreshShipButtonsInteraction();
        }

        private void HandleActionSelectionChanged(UnitCardController unitCardController, SelectedActionType selectedAction)
        {
            if (selectedAction == SelectedActionType.None)
                return;

            var shipInstance = unitCardController.Instance;
            if (!shipInstance)
                return;
            if (shipInstance.State != PlaceableObjectState.Placed)
                return;

            var abilityPrefab = GetAbilityPrefab(shipInstance, selectedAction);
            if (!abilityPrefab)
                return;
            if (!IsAllowedForCurrentBattlefield(abilityPrefab))
                return;
            if (HasPlacedAbilityForSide(abilityPrefab.AllowedDeploymentSide))
                return;

            SpawnOrReplaceActiveAbility(unitCardController, shipInstance, abilityPrefab);
        }

        private void SpawnOrReplaceActiveAbility(UnitCardController unitCardController, PlaceableObject.PlaceableObject shipInstance, Ability abilityPrefab)
        {
            if (_activeAbilities.TryGetValue(shipInstance, out var activeAbility) && activeAbility)
            {
                if (activeAbility.State == PlaceableObjectState.Placed)
                    return;

                if (activeAbility.State == PlaceableObjectState.Picked &&
                    _activeAbilityPrefabs.TryGetValue(shipInstance, out var activePrefab) &&
                    activePrefab == abilityPrefab)
                    return;

                if (activeAbility.State == PlaceableObjectState.Picked)
                    Destroy(activeAbility.gameObject);
            }

            var abilityInstance = spawning.Spawn(abilityPrefab);
            if (!abilityInstance)
                return;

            lifecycleTracker.Register(abilityInstance);
            abilityInstance.TryTakeFromStorage();

            _activeAbilities[shipInstance] = abilityInstance;
            _activeAbilityPrefabs[shipInstance] = abilityPrefab;
            _abilityOwners[abilityInstance] = new AbilityOwner(shipInstance);
            _abilityOwnerButtons[abilityInstance] = unitCardController;

            OnAbilitySpawned?.Invoke(abilityInstance);
        }

        private static Ability GetAbilityPrefab(Ship ship, SelectedActionType selectedAction)
        {
            return selectedAction switch
            {
                SelectedActionType.Default => ship.DefaultAbility,
                SelectedActionType.Ship => ship.UnitAbility,
                SelectedActionType.Faction => ship.FactionAbility,
                _ => null
            };
        }

        private void HandleTrackedObjectDestroyed(PlaceableObject.PlaceableObject destroyedObject)
        {
            if (_activeAbilities.Remove(destroyedObject))
            {
                _activeAbilityPrefabs.Remove(destroyedObject);
                RemoveAbilitiesOwnedByShip(destroyedObject);
            }

            if (!_abilityOwners.TryGetValue(destroyedObject, out var owner))
                return;

            _abilityOwners.Remove(destroyedObject);
            SetOwnerButtonInteraction(destroyedObject, true);
            _abilityOwnerButtons.Remove(destroyedObject);

            if (_activeAbilities.TryGetValue(owner.ShipInstance, out var activeAbility) && activeAbility == destroyedObject)
            {
                _activeAbilities.Remove(owner.ShipInstance);
                _activeAbilityPrefabs.Remove(owner.ShipInstance);
            }

            RefreshShipButtonsInteraction();
        }

        private void HandleTrackedObjectPlaced(PlaceableObject.PlaceableObject placedObject)
        {
            if (!_abilityOwners.ContainsKey(placedObject))
                return;

            SetOwnerButtonInteraction(placedObject, false);
            RefreshShipButtonsInteraction();
        }

        private void HandleTrackedObjectPicked(PlaceableObject.PlaceableObject pickedObject)
        {
            if (!_abilityOwners.ContainsKey(pickedObject))
                return;

            SetOwnerButtonInteraction(pickedObject, true);
            RefreshShipButtonsInteraction();
        }

        private void RemoveAbilitiesOwnedByShip(PlaceableObject.PlaceableObject shipInstance)
        {
            var staleAbilities = new List<PlaceableObject.PlaceableObject>();
            foreach (var pair in _abilityOwners)
            {
                if (pair.Value.ShipInstance == shipInstance)
                    staleAbilities.Add(pair.Key);
            }

            foreach (var staleAbility in staleAbilities)
                _abilityOwners.Remove(staleAbility);
        }

        private void SetOwnerButtonInteraction(PlaceableObject.PlaceableObject abilityInstance, bool isEnabled)
        {
            if (!_abilityOwnerButtons.TryGetValue(abilityInstance, out var ownerButton) || !ownerButton)
                return;

            ownerButton.SetInteractionEnabled(isEnabled);
        }

        private void RefreshShipButtonsInteraction()
        {
            var isBlocked = IsShipInteractionBlocked();

            foreach (var shipButton in _shipButtons)
            {
                if (!shipButton)
                    continue;

                shipButton.SetInteractionEnabled(!isBlocked);
            }
        }

        private bool HasPlacedAbilityForSide(PlaceableObjectDeploymentSide deploymentSide)
        {
            foreach (var abilityInstance in _abilityOwners.Keys)
            {
                if (!abilityInstance)
                    continue;
                if (abilityInstance.State != PlaceableObjectState.Placed)
                    continue;
                if (abilityInstance.AllowedDeploymentSide == deploymentSide)
                    return true;
            }

            return false;
        }

        private bool HasPlacedAbility()
        {
            foreach (var abilityInstance in _abilityOwners.Keys)
            {
                if (abilityInstance && abilityInstance.State == PlaceableObjectState.Placed)
                    return true;
            }

            return false;
        }

        private bool IsAllowedForCurrentBattlefield(PlaceableObject.PlaceableObject placeableObject)
        {
            return _isPlayerBattlefieldActive
                ? placeableObject.AllowedDeploymentSide == PlaceableObjectDeploymentSide.OwnField
                : placeableObject.AllowedDeploymentSide == PlaceableObjectDeploymentSide.EnemyField;
        }
    }
}
