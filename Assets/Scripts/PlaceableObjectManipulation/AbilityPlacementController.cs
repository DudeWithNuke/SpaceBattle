using System;
using System.Collections.Generic;
using PlaceableObject;
using PlaceableObject.Abilities;
using PlaceableObject.Ships;
using PlayerCamera;
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
            lifecycleTracker.OnDestroyed += HandleTrackedObjectDestroyed;
            _isPlayerBattlefieldActive = cameraMovement.IsPlayerBattlefieldActive;
            cameraMovement.OnBattlefieldSideChanged += HandleBattlefieldSideChanged;
        }

        private void OnDestroy()
        {
            cameraMovement.OnBattlefieldSideChanged -= HandleBattlefieldSideChanged;
            lifecycleTracker.OnDestroyed -= HandleTrackedObjectDestroyed;
        }

        public void RegisterShipButton(UI.ShipButton shipButton)
        {
            if (shipButton == null)
                return;

            shipButton.OnActionSelectionChanged += HandleActionSelectionChanged;
            shipButton.SetAbilityBattlefield(_isPlayerBattlefieldActive);
        }

        public void UnregisterShipButton(UI.ShipButton shipButton)
        {
            if (shipButton == null)
                return;

            shipButton.OnActionSelectionChanged -= HandleActionSelectionChanged;
        }

        private void HandleBattlefieldSideChanged(bool isPlayerBattlefieldActive)
        {
            _isPlayerBattlefieldActive = isPlayerBattlefieldActive;
        }

        private void HandleActionSelectionChanged(UI.ShipButton shipButton, UI.SelectedActionType selectedAction)
        {
            if (selectedAction == UI.SelectedActionType.None)
                return;

            var shipInstance = shipButton.Instance;
            if (!shipInstance)
                return;
            if (shipInstance.State != PlaceableObjectState.Placed)
                return;

            var abilityPrefab = GetAbilityPrefab(shipInstance, selectedAction);
            if (!abilityPrefab)
                return;
            if (!IsAllowedForCurrentBattlefield(abilityPrefab))
                return;

            SpawnOrReplaceActiveAbility(shipInstance, abilityPrefab);
        }

        private void SpawnOrReplaceActiveAbility(PlaceableObject.PlaceableObject shipInstance, Ability abilityPrefab)
        {
            if (_activeAbilities.TryGetValue(shipInstance, out var activeAbility) && activeAbility)
            {
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

            OnAbilitySpawned?.Invoke(abilityInstance);
        }

        private static Ability GetAbilityPrefab(Ship ship, UI.SelectedActionType selectedAction)
        {
            return selectedAction switch
            {
                UI.SelectedActionType.StandardAttack => ship.DefaultAbility,
                UI.SelectedActionType.AbilitySlot1 => ship.UnitAbility,
                UI.SelectedActionType.AbilitySlot2 => ship.FactionAbility,
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

            if (_activeAbilities.TryGetValue(owner.ShipInstance, out var activeAbility) && activeAbility == destroyedObject)
            {
                _activeAbilities.Remove(owner.ShipInstance);
                _activeAbilityPrefabs.Remove(owner.ShipInstance);
            }
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

        private bool IsAllowedForCurrentBattlefield(PlaceableObject.PlaceableObject placeableObject)
        {
            return _isPlayerBattlefieldActive
                ? placeableObject.AllowedDeploymentSide == PlaceableObjectDeploymentSide.OwnField
                : placeableObject.AllowedDeploymentSide == PlaceableObjectDeploymentSide.EnemyField;
        }
    }
}
