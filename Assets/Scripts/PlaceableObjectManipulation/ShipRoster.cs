using System.Collections.Generic;
using UnityEngine;

namespace PlaceableObjectManipulation
{
    public class ShipRoster : MonoBehaviour
    {
        [SerializeField] private List<PlaceableObject.PlaceableObject> playerShips = new();
        [SerializeField] private List<PlaceableObject.PlaceableObject> enemyShips = new();

        public IReadOnlyList<PlaceableObject.PlaceableObject> PlayerShips => playerShips;
        public IReadOnlyList<PlaceableObject.PlaceableObject> EnemyShips => enemyShips;

        public IEnumerable<PlaceableObject.PlaceableObject> GetAll()
        {
            foreach (var ship in playerShips)
                yield return ship;
            foreach (var ship in enemyShips)
                yield return ship;
        }
    }
}
