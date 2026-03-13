using System.Collections.Generic;
using UnityEngine;

namespace PlaceableObject
{
    public class ShipRoster : MonoBehaviour
    {
        [SerializeField] private List<PlaceableObject> playerShips = new();
        [SerializeField] private List<PlaceableObject> enemyShips = new();

        public IReadOnlyList<PlaceableObject> PlayerShips => playerShips;
        public IReadOnlyList<PlaceableObject> EnemyShips => enemyShips;

        public IEnumerable<PlaceableObject> GetAll()
        {
            foreach (var ship in playerShips)
                yield return ship;
            foreach (var ship in enemyShips)
                yield return ship;
        }
    }
}
