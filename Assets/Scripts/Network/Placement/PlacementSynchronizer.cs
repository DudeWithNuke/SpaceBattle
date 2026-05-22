using System;
using System.Collections.Generic;
using Network.Match;
using Network.Player;
using PlaceableObject;
using Utils;

namespace Network.Placement
{
    public readonly struct NetworkRemotePlacement
    {
        public readonly FieldObjectPlacedEventDto Event;
        public readonly PlaceableObject.PlaceableObject Prefab;
        public readonly PlaceableObject.PlaceableObject ExistingObject;

        public NetworkRemotePlacement(
            FieldObjectPlacedEventDto evt,
            PlaceableObject.PlaceableObject prefab,
            PlaceableObject.PlaceableObject existingObject)
        {
            Event = evt;
            Prefab = prefab;
            ExistingObject = existingObject;
        }
    }

    public sealed class PlacementSynchronizer
    {
        private readonly Dictionary<string, PlaceableObject.PlaceableObject> _objectsById = new();
        private readonly PlayerContext _playerContext;
        private readonly PrefabRegistry _prefabRegistry;

        public PlacementSynchronizer(PlayerContext playerContext, PrefabRegistry prefabRegistry)
        {
            _playerContext = playerContext;
            _prefabRegistry = prefabRegistry;
        }

        public FieldSnapshotDto CreateLocalFieldSnapshot(
            IEnumerable<PlaceableObject.PlaceableObject> trackedObjects,
            MatchPhase phase,
            int turnNumber)
        {
            if (!_playerContext.HasAssignedPlayer)
            {
                Log.Warn("[NetworkPlacementController] Cannot create field snapshot. Local player is not assigned yet.");
                return default;
            }

            var objects = new List<FieldObjectPlacedEventDto>();
            foreach (var placeableObject in trackedObjects)
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
                ownerPlayerIndex = _playerContext.LocalPlayerIndex,
                phase = phase,
                turnNumber = turnNumber,
                objects = objects.ToArray()
            };
        }

        public List<PlaceableObject.PlaceableObject> GetStaleObjects(FieldSnapshotDto snapshot)
        {
            var snapshotIds = new HashSet<string>();
            foreach (var fieldObject in snapshot.objects)
            {
                if (!string.IsNullOrWhiteSpace(fieldObject.objectId))
                    snapshotIds.Add(fieldObject.objectId);
            }

            var staleObjects = new List<PlaceableObject.PlaceableObject>();
            foreach (var (key, placeableObject) in _objectsById)
            {
                if (!placeableObject)
                    continue;
                if (placeableObject.OwnerPlayerIndex != snapshot.ownerPlayerIndex)
                    continue;
                if (snapshotIds.Contains(key))
                    continue;

                staleObjects.Add(placeableObject);
            }

            foreach (var staleObject in staleObjects)
                _objectsById.Remove(staleObject.NetworkObjectId);

            return staleObjects;
        }

        public bool TryCreateRemotePlacement(
            FieldObjectPlacedEventDto evt,
            out NetworkRemotePlacement placement)
        {
            placement = default;
            if (string.IsNullOrWhiteSpace(evt.objectId))
                return false;

            _objectsById.TryGetValue(evt.objectId, out var existingObject);
            if (existingObject)
            {
                if (_playerContext.IsLocalPlayer(existingObject.OwnerPlayerIndex))
                    return false;

                _objectsById.Remove(evt.objectId);
            }

            if (_playerContext.IsLocalPlayer(evt.objectOwnerPlayerIndex))
                return false;

            if (!_prefabRegistry.TryGetPrefab(evt.prefabId, out var prefab))
            {
                Log.Warn($"[NetworkPlacementController] Field object prefab not found: {evt.prefabId}.");
                return false;
            }

            placement = new NetworkRemotePlacement(evt, prefab, existingObject);
            return true;
        }

        public void RegisterRemoteObject(string objectId, PlaceableObject.PlaceableObject instance)
        {
            _objectsById[objectId] = instance;
        }

        private bool ShouldIncludeInLocalSnapshot(PlaceableObject.PlaceableObject placeableObject)
        {
            if (!placeableObject || placeableObject.State != PlaceableObjectState.Placed)
                return false;

            return placeableObject.OwnerPlayerIndex == 0 ||
                   _playerContext.IsLocalPlayer(placeableObject.OwnerPlayerIndex);
        }

        private void EnsureLocalIdentity(PlaceableObject.PlaceableObject placeableObject)
        {
            if (!string.IsNullOrWhiteSpace(placeableObject.NetworkObjectId))
                return;

            var objectId = Guid.NewGuid().ToString("N");
            var prefabId = PrefabRegistry.GetPrefabIdForInstance(placeableObject);
            placeableObject.SetNetworkIdentity(objectId, prefabId, _playerContext.LocalPlayerIndex, false);
        }

        private FieldObjectPlacedEventDto CreateFieldObjectState(
            PlaceableObject.PlaceableObject placeableObject,
            int fieldOwnerPlayerIndex)
        {
            placeableObject.EnsureShapeInitialized();
            return new FieldObjectPlacedEventDto
            {
                objectOwnerPlayerIndex = _playerContext.LocalPlayerIndex,
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
                return _playerContext.LocalPlayerIndex;

            return _playerContext.LocalPlayerIndex == 1 ? 2 : 1;
        }
    }
}
