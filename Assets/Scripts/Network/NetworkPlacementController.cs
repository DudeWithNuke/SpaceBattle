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

            if (matchController)
            {
                matchController.OnFieldSnapshotReceived += HandleFieldSnapshotReceived;
            }
        }

        private void OnDisable()
        {
            if (matchController)
            {
                matchController.OnFieldSnapshotReceived -= HandleFieldSnapshotReceived;
            }
        }

        public FieldSnapshotDto CreateLocalFieldSnapshot(MatchPhase phase, int turnNumber)
        {
            if (!playerContext || !playerContext.HasAssignedPlayer)
            {
                Debug.LogWarning("[NetworkPlacementController] Cannot create field snapshot. Local player is not assigned yet.");
                return default;
            }

            var objects = new List<FieldObjectPlacedEventDto>();
            foreach (var placeableObject in lifecycleTracker.TrackedObjects)
            {
                if (!ShouldIncludeInLocalSnapshot(placeableObject))
                    continue;

                EnsureLocalIdentity(placeableObject);
                var fieldOwnerPlayerIndex = GetFieldOwnerPlayerIndex(placeableObject);
                placeableObject.SetNetworkFieldOwner(fieldOwnerPlayerIndex);
                _objectsById[placeableObject.NetworkObjectId] = placeableObject;
                objects.Add(CreateFieldObjectState(placeableObject, fieldOwnerPlayerIndex));
            }

            return new FieldSnapshotDto
            {
                ownerPlayerIndex = playerContext.LocalPlayerIndex,
                phase = phase,
                turnNumber = turnNumber,
                objects = objects.ToArray()
            };
        }

        public void SubmitLocalFieldSnapshot(MatchPhase phase, int turnNumber)
        {
            var snapshot = CreateLocalFieldSnapshot(phase, turnNumber);
            if (snapshot.ownerPlayerIndex <= 0)
                return;

            Debug.Log($"[NetworkPlacementController] Submitting field snapshot. Player{snapshot.ownerPlayerIndex}, Objects={snapshot.objects?.Length ?? 0}.");
            matchController.SubmitFieldSnapshot(snapshot);
        }

        private void HandleFieldSnapshotReceived(FieldSnapshotDto snapshot)
        {
            if (snapshot.objects == null)
                return;

            RemoveStaleSnapshotObjects(snapshot);
            foreach (var fieldObject in snapshot.objects)
                HandleFieldObjectState(fieldObject);
        }

        private void HandleFieldObjectState(FieldObjectPlacedEventDto evt)
        {
            if (_objectsById.ContainsKey(evt.objectId))
                return;

            if (!prefabRegistry.TryGetPrefab(evt.prefabId, out var prefab))
            {
                Debug.LogWarning($"[NetworkPlacementController] Field object prefab not found: {evt.prefabId}.");
                return;
            }

            Debug.Log($"[NetworkPlacementController] Applying field object snapshot: objectOwner=Player{evt.objectOwnerPlayerIndex}, fieldOwner=Player{evt.fieldOwnerPlayerIndex}, prefab={evt.prefabId}, cell={evt.originCell}.");
            StartCoroutine(SpawnConfirmedObject(evt, prefab));
        }

        private IEnumerator SpawnConfirmedObject(
            FieldObjectPlacedEventDto evt,
            PlaceableObject.PlaceableObject prefab)
        {
            var isLocalPlayerObject = playerContext.IsLocalPlayer(evt.objectOwnerPlayerIndex);
            var localCell = playerContext.ToLocalViewCell(
                evt.fieldOwnerPlayerIndex,
                evt.originCell,
                cellGrid.GridSize,
                rotateOpponentFieldCoordinates);
            var isPlayerObject = playerContext.IsPlayerObject(evt.fieldOwnerPlayerIndex);
            var instance = spawning.SpawnAtCell(prefab, localCell, isPlayerObject);
            if (!instance)
                yield break;

            instance.SetCellStateVisualizationEnabled(isLocalPlayerObject);

            if (!isLocalPlayerObject && rotateOpponentObjects)
                instance.ApplyLocalViewRotation180();

            instance.CurrentPosition = localCell;
            instance.SetNetworkIdentity(evt.objectId, evt.prefabId, evt.objectOwnerPlayerIndex, true);
            instance.SetNetworkFieldOwner(evt.fieldOwnerPlayerIndex);
            _objectsById[evt.objectId] = instance;
            lifecycleTracker.Register(instance);

            yield return null;

            instance.TryTakeFromStorage();
            instance.TryPlace();
        }

        private bool ShouldIncludeInLocalSnapshot(PlaceableObject.PlaceableObject placeableObject)
        {
            if (!placeableObject || placeableObject.State != PlaceableObjectState.Placed)
                return false;

            return placeableObject.OwnerPlayerIndex == 0 ||
                   playerContext.IsLocalPlayer(placeableObject.OwnerPlayerIndex);
        }

        private void EnsureLocalIdentity(PlaceableObject.PlaceableObject placeableObject)
        {
            if (!string.IsNullOrWhiteSpace(placeableObject.NetworkObjectId))
                return;

            var objectId = Guid.NewGuid().ToString("N");
            var prefabId = prefabRegistry.GetPrefabIdForInstance(placeableObject);
            placeableObject.SetNetworkIdentity(objectId, prefabId, playerContext.LocalPlayerIndex, false);
        }

        private FieldObjectPlacedEventDto CreateFieldObjectState(
            PlaceableObject.PlaceableObject placeableObject,
            int fieldOwnerPlayerIndex)
        {
            placeableObject.EnsureShapeInitialized();
            return new FieldObjectPlacedEventDto
            {
                objectOwnerPlayerIndex = playerContext.LocalPlayerIndex,
                fieldOwnerPlayerIndex = fieldOwnerPlayerIndex,
                objectId = placeableObject.NetworkObjectId,
                prefabId = placeableObject.NetworkPrefabId,
                originCell = placeableObject.CurrentPosition,
                occupiedCells = placeableObject.Shape.GetOccupiedCells(placeableObject.CurrentPosition)
            };
        }

        private int GetFieldOwnerPlayerIndex(PlaceableObject.PlaceableObject placeableObject)
        {
            if (placeableObject.IsPlayerObject)
                return playerContext.LocalPlayerIndex;

            return playerContext.LocalPlayerIndex == 1 ? 2 : 1;
        }

        private void RemoveStaleSnapshotObjects(FieldSnapshotDto snapshot)
        {
            var snapshotIds = new HashSet<string>();
            foreach (var fieldObject in snapshot.objects)
            {
                if (!string.IsNullOrWhiteSpace(fieldObject.objectId))
                    snapshotIds.Add(fieldObject.objectId);
            }

            var staleObjects = new List<PlaceableObject.PlaceableObject>();
            foreach (var pair in _objectsById)
            {
                var placeableObject = pair.Value;
                if (!placeableObject)
                    continue;
                if (placeableObject.OwnerPlayerIndex != snapshot.ownerPlayerIndex)
                    continue;
                if (snapshotIds.Contains(pair.Key))
                    continue;

                staleObjects.Add(placeableObject);
            }

            foreach (var staleObject in staleObjects)
            {
                _objectsById.Remove(staleObject.NetworkObjectId);
                Destroy(staleObject.gameObject);
            }
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
