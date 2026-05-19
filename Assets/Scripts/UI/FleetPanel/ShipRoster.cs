using System.Collections.Generic;
using UnityEngine;

namespace UI.FleetPanel
{
    public class ShipRoster : MonoBehaviour
    {
        [SerializeField] private List<PlaceableObject.PlaceableObject> playerShips = new();
        [SerializeField] private List<PlaceableObject.PlaceableObject> enemyShips = new();

        public IReadOnlyList<PlaceableObject.PlaceableObject> PlayerShips => playerShips;
        public IReadOnlyList<PlaceableObject.PlaceableObject> EnemyShips => enemyShips;

        public IEnumerable<PlaceableObject.PlaceableObject> GetForPlayer(int playerIndex)
        {
            var ships = playerIndex == 2 ? enemyShips : playerShips;
            foreach (var ship in ships)
                yield return ship;
        }

        public IEnumerable<PlaceableObject.PlaceableObject> GetAll()
        {
            foreach (var ship in playerShips)
                yield return ship;
            foreach (var ship in enemyShips)
                yield return ship;
        }
    }
}
