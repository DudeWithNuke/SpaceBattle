using System;
using System.Collections;
using System.Collections.Generic;
using PlaceableObject;
using PlaceableObject.Abilities;
using PlaceableObject.Ships;
using PlaceableObjectManipulation;
using GameBoard;
using UnityEngine;

namespace Network
{
    public sealed class NetworkPlacementController : MonoBehaviour
    {
        [SerializeField] private Spawning spawning;
        [SerializeField] private SpawnedObjectLifecycleTracker lifecycleTracker;
        [SerializeField] private NetworkMatchController matchController;
        [SerializeField] private NetworkPlayerContext playerContext;
        [SerializeField] private NetworkPrefabRegistry prefabRegistry;
        [SerializeField] private CellGrid cellGrid;
        [SerializeField] private bool rotateOpponentFieldCoordinates;
        [SerializeField] private bool rotateOpponentObjects;

        private readonly Dictionary<string, PlaceableObject.PlaceableObject> _objectsById = new();

        private void Awake()
        {
            ResolveDependencies();
        }

        private void OnEnable()
        {
            ResolveDependencies();

            if (lifecycleTracker)
                lifecycleTracker.OnPlaced += HandleLocalObjectPlaced;

            if (matchController)
            {
                matchController.OnShipPlaced += HandleShipPlaced;
                matchController.OnAbilityPlaced += HandleAbilityPlaced;
            }
        }

        private void OnDisable()
        {
            if (lifecycleTracker)
                lifecycleTracker.OnPlaced -= HandleLocalObjectPlaced;

            if (matchController)
            {
                matchController.OnShipPlaced -= HandleShipPlaced;
                matchController.OnAbilityPlaced -= HandleAbilityPlaced;
            }
        }

        private void HandleLocalObjectPlaced(PlaceableObject.PlaceableObject placeableObject)
        {
            if (!placeableObject || placeableObject.SuppressNetworkPlacementEvent)
                return;
            if (!playerContext || !playerContext.HasAssignedPlayer)
            {
                Debug.LogWarning("[NetworkPlacementController] Local placement ignored. Local player is not assigned yet.");
                return;
            }

            EnsureLocalIdentity(placeableObject);
            _objectsById[placeableObject.NetworkObjectId] = placeableObject;

            if (placeableObject is Ship)
            {
                Debug.Log($"[NetworkPlacementController] Submitting ship placement: {placeableObject.NetworkPrefabId} at {placeableObject.CurrentPosition}.");
                matchController.SubmitShipPlacement(CreateShipPlacementRequest(placeableObject));
                return;
            }

            if (placeableObject is Ability)
            {
                Debug.Log($"[NetworkPlacementController] Submitting ability placement: {placeableObject.NetworkPrefabId} at {placeableObject.CurrentPosition}.");
                matchController.SubmitAbilityPlacement(CreateAbilityPlacementRequest(placeableObject));
            }
        }

        private void HandleShipPlaced(ShipPlacedEventDto evt)
        {
            if (ShouldSkipConfirmedLocalObject(evt.ownerPlayerIndex, evt.objectId))
                return;

            if (!prefabRegistry.TryGetPrefab(evt.prefabId, out var prefab))
            {
                Debug.LogWarning($"[NetworkPlacementController] Ship prefab not found: {evt.prefabId}.");
                return;
            }

            Debug.Log($"[NetworkPlacementController] Received ship placement: owner=Player{evt.ownerPlayerIndex}, prefab={evt.prefabId}, cell={evt.originCell}.");
            StartCoroutine(SpawnConfirmedObject(evt.objectId, evt.prefabId, evt.ownerPlayerIndex, evt.originCell, prefab));
        }

        private void HandleAbilityPlaced(AbilityPlacedEventDto evt)
        {
            if (ShouldSkipConfirmedLocalObject(evt.ownerPlayerIndex, evt.objectId))
                return;

            if (!prefabRegistry.TryGetPrefab(evt.prefabId, out var prefab))
            {
                Debug.LogWarning($"[NetworkPlacementController] Ability prefab not found: {evt.prefabId}.");
                return;
            }

            Debug.Log($"[NetworkPlacementController] Received ability placement: owner=Player{evt.ownerPlayerIndex}, prefab={evt.prefabId}, cell={evt.targetCell}.");
            StartCoroutine(SpawnConfirmedObject(evt.objectId, evt.prefabId, evt.targetOwnerPlayerIndex, evt.targetCell, prefab));
        }

        private IEnumerator SpawnConfirmedObject(
            string objectId,
            string prefabId,
            int ownerPlayerIndex,
            Vector3Int cell,
            PlaceableObject.PlaceableObject prefab)
        {
            var isLocalPlayerObject = playerContext.IsLocalPlayer(ownerPlayerIndex);
            var localCell = playerContext.ToLocalViewCell(
                ownerPlayerIndex,
                cell,
                cellGrid.GridSize,
                rotateOpponentFieldCoordinates);
            var isPlayerObject = playerContext.IsPlayerObject(ownerPlayerIndex);
            var instance = spawning.SpawnAtCell(prefab, localCell, isPlayerObject);
            if (!instance)
                yield break;

            if (!isLocalPlayerObject && rotateOpponentObjects)
                instance.ApplyLocalViewRotation180();

            instance.CurrentPosition = localCell;
            instance.SetNetworkIdentity(objectId, prefabId, ownerPlayerIndex, true);
            _objectsById[objectId] = instance;
            lifecycleTracker.Register(instance);

            yield return null;

            instance.TryTakeFromStorage();
            instance.TryPlace();
        }

        private bool ShouldSkipConfirmedLocalObject(int ownerPlayerIndex, string objectId)
        {
            if (!playerContext.IsLocalPlayer(ownerPlayerIndex))
                return false;

            return !string.IsNullOrWhiteSpace(objectId) && _objectsById.ContainsKey(objectId);
        }

        private void EnsureLocalIdentity(PlaceableObject.PlaceableObject placeableObject)
        {
            if (!string.IsNullOrWhiteSpace(placeableObject.NetworkObjectId))
                return;

            var objectId = Guid.NewGuid().ToString("N");
            var prefabId = prefabRegistry.GetPrefabIdForInstance(placeableObject);
            placeableObject.SetNetworkIdentity(objectId, prefabId, playerContext.LocalPlayerIndex, false);
        }

        private PlaceShipRequestDto CreateShipPlacementRequest(PlaceableObject.PlaceableObject placeableObject)
        {
            placeableObject.EnsureShapeInitialized();
            return new PlaceShipRequestDto
            {
                objectId = placeableObject.NetworkObjectId,
                prefabId = placeableObject.NetworkPrefabId,
                originCell = placeableObject.CurrentPosition,
                occupiedCells = placeableObject.Shape.GetOccupiedCells(placeableObject.CurrentPosition)
            };
        }

        private PlaceAbilityRequestDto CreateAbilityPlacementRequest(PlaceableObject.PlaceableObject placeableObject)
        {
            return new PlaceAbilityRequestDto
            {
                objectId = placeableObject.NetworkObjectId,
                sourceShipId = string.Empty,
                abilityId = placeableObject.NetworkPrefabId,
                prefabId = placeableObject.NetworkPrefabId,
                targetOwnerPlayerIndex = GetTargetOwnerPlayerIndex(placeableObject),
                targetCell = placeableObject.CurrentPosition
            };
        }

        private int GetTargetOwnerPlayerIndex(PlaceableObject.PlaceableObject placeableObject)
        {
            if (placeableObject.IsPlayerObject)
                return playerContext.LocalPlayerIndex;

            return playerContext.LocalPlayerIndex == 1 ? 2 : 1;
        }

        private void ResolveDependencies()
        {
            if (!spawning)
                spawning = FindFirstObjectByType<Spawning>();
            if (!lifecycleTracker)
                lifecycleTracker = FindFirstObjectByType<SpawnedObjectLifecycleTracker>();
            if (!matchController)
                matchController = GetComponent<NetworkMatchController>() ?? FindFirstObjectByType<NetworkMatchController>();
            if (!playerContext)
                playerContext = GetComponent<NetworkPlayerContext>() ?? FindFirstObjectByType<NetworkPlayerContext>();
            if (!prefabRegistry)
                prefabRegistry = GetComponent<NetworkPrefabRegistry>() ?? FindFirstObjectByType<NetworkPrefabRegistry>();
            if (!cellGrid)
                cellGrid = FindFirstObjectByType<CellGrid>();
        }
    }
}
