using System;
using Network.Match;
using UnityEngine;

namespace Network
{
    public static class NetworkMessageNames
    {
        public const string SubmitFleetPreset = "SubmitFleetPreset";
        public const string EndTurnRequest = "EndTurnRequest";
        public const string MatchState = "MatchState";
        public const string PlayerAssigned = "PlayerAssigned";
        public const string SubmitFieldSnapshot = "SubmitFieldSnapshot";
        public const string FieldSnapshot = "FieldSnapshot";
    }

    [Serializable]
    public struct FleetPresetDto
    {
        public string presetId;
        public ShipPlacementDto[] ships;
    }

    [Serializable]
    public struct ShipPlacementDto
    {
        public string shipId;
        public string hullId;
        public Vector3Int originCell;
        public Vector3Int[] occupiedCells;
        public string[] modifierIds;
        public string defaultAbilityId;
        public string unitAbilityId;
        public string factionAbilityId;
    }

    [Serializable]
    public struct EndTurnRequestDto
    {
        public int turnNumber;
    }

    [Serializable]
    public struct MatchStateDto
    {
        public MatchPhase phase;
        public int turnNumber;
        public int activePlayerIndex;
        public float remainingSeconds;
        public MatchStatus status;
    }

    [Serializable]
    public struct PlayerAssignedEventDto
    {
        public int playerIndex;
    }

    [Serializable]
    public struct FieldObjectPlacedEventDto
    {
        public int objectOwnerPlayerIndex;
        public int fieldOwnerPlayerIndex;
        public string objectId;
        public string prefabId;
        public Vector3Int originCell;
        public Vector3Int[] occupiedCells;
    }

    [Serializable]
    public struct FieldSnapshotDto
    {
        public int ownerPlayerIndex;
        public MatchPhase phase;
        public int turnNumber;
        public FieldObjectPlacedEventDto[] objects;
    }

    [Serializable]
    public struct NetworkEnvelope<T>
    {
        public T payload;
    }
}
