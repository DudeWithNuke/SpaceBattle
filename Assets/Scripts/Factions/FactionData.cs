using System.Collections.Generic;
using PlaceableObject.Abilities.Data;
using PlaceableObject.Ships.Data;
using UnityEngine;

namespace Factions
{
    [CreateAssetMenu(fileName = "FactionData", menuName = "Game/Factions/Faction Data")]
    public sealed class FactionData : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField] private int fleetCellLimit;
        [SerializeField] private List<ShipHullData> availableHulls = new();
        [SerializeField] private List<FactionAbilityData> availableFactionAbilities = new();

        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public int FleetCellLimit => fleetCellLimit;
        public IReadOnlyList<ShipHullData> AvailableHulls => availableHulls;
        public IReadOnlyList<FactionAbilityData> AvailableFactionAbilities => availableFactionAbilities;
    }
}
