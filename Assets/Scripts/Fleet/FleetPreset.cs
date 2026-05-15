using System.Collections.Generic;
using System.Linq;
using Factions;
using PlaceableObject.Ships;
using UnityEngine;

namespace Fleet
{
    [CreateAssetMenu(fileName = "FleetPreset", menuName = "Game/Fleet/Fleet Preset")]
    public sealed class FleetPreset : ScriptableObject
    {
        [SerializeField] private FactionData faction;
        [SerializeField] private List<Ship> ships = new();

        public FactionData Faction => faction;
        public IReadOnlyList<Ship> Ships => ships;

        public int OccupiedCellCount
        {
            get
            {
                var count = 0;

                foreach (var ship in ships.Where(ship => ship))
                {
                    ship.EnsureShapeInitialized();
                    count += ship.Shape.occupiedOffsets.Count;
                }

                return count;
            }
        }
    }
}
