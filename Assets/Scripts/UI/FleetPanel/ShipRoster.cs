using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace UI.FleetPanel
{
    [MovedFrom("PlaceableObject")]
    public class ShipRoster : MonoBehaviour
    {
        [FormerlySerializedAs("playerShips")]
        [SerializeField] private List<PlaceableObject.PlaceableObject> player1Ships = new();

        [FormerlySerializedAs("enemyShips")]
        [SerializeField] private List<PlaceableObject.PlaceableObject> player2Ships = new();

        public IReadOnlyList<PlaceableObject.PlaceableObject> Player1Ships => player1Ships;
        public IReadOnlyList<PlaceableObject.PlaceableObject> Player2Ships => player2Ships;

        public IEnumerable<PlaceableObject.PlaceableObject> GetForPlayer(int playerIndex)
        {
            var ships = GetListForPlayer(playerIndex);
            foreach (var ship in ships)
                yield return ship;
        }

        public bool ContainsForPlayer(int playerIndex, PlaceableObject.PlaceableObject prefab)
        {
            return prefab && GetListForPlayer(playerIndex).Contains(prefab);
        }

        public IEnumerable<PlaceableObject.PlaceableObject> GetAll()
        {
            foreach (var ship in player1Ships)
                yield return ship;
            foreach (var ship in player2Ships)
                yield return ship;
        }

        private IReadOnlyList<PlaceableObject.PlaceableObject> GetListForPlayer(int playerIndex)
        {
            return playerIndex == 2 ? player2Ships : player1Ships;
        }
    }
}
