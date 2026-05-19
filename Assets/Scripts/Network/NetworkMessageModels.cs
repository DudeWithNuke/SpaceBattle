using System;
using UnityEngine;

namespace Network
{
    public static class NetworkMessageNames
    {
        public const string SubmitFleetPreset = "SubmitFleetPreset";
        public const string UseAbilityRequest = "UseAbilityRequest";
        public const string EndTurnRequest = "EndTurnRequest";
        public const string MatchState = "MatchState";
        public const string PlayerAssigned = "PlayerAssigned";
        public const string PlaceShipRequest = "PlaceShipRequest";
        public const string ShipPlaced = "ShipPlaced";
        public const string PlaceAbilityRequest = "PlaceAbilityRequest";
        public const string AbilityPlaced = "AbilityPlaced";
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
    public struct UseAbilityRequestDto
    {
        public string sourceShipId;
        public string abilityId;
        public Vector3Int targetCell;
    }

    [Serializable]
    public struct EndTurnRequestDto
    {
        public int turnNumber;
    }

    [Serializable]
    public struct MatchStateDto
    {
        public int turnNumber;
        public int activePlayerIndex;
        public float remainingSeconds;
        public string status;
    }

    [Serializable]
    public struct PlayerAssignedEventDto
    {
        public int playerIndex;
    }

    [Serializable]
    public struct PlaceShipRequestDto
    {
        public string objectId;
        public string prefabId;
        public Vector3Int originCell;
        public Vector3Int[] occupiedCells;
    }

    [Serializable]
    public struct ShipPlacedEventDto
    {
        public int ownerPlayerIndex;
        public string objectId;
        public string prefabId;
        public Vector3Int originCell;
        public Vector3Int[] occupiedCells;
    }

    [Serializable]
    public struct PlaceAbilityRequestDto
    {
        public string objectId;
        public string sourceShipId;
        public string abilityId;
        public string prefabId;
        public int targetOwnerPlayerIndex;
        public Vector3Int targetCell;
    }

    [Serializable]
    public struct AbilityPlacedEventDto
    {
        public int ownerPlayerIndex;
        public string objectId;
        public string sourceShipId;
        public string abilityId;
        public string prefabId;
        public int targetOwnerPlayerIndex;
        public Vector3Int targetCell;
    }

    [Serializable]
    public struct NetworkEnvelope<T>
    {
        public T payload;
    }
}
