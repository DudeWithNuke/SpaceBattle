using System.Collections.Generic;
using UnityEngine;

namespace FutureBattleflow.Models
{
    [CreateAssetMenu(fileName = "PlayerLoadout", menuName = "Game/FutureBattleflow/Player Loadout")]
    public class PlayerLoadoutAsset : ScriptableObject
    {
        [SerializeField] private List<LoadoutShipEntry> ships = new();

        public IReadOnlyList<LoadoutShipEntry> Ships => ships;
    }
}
