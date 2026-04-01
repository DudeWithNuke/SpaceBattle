using System;
using System.Collections.Generic;
using PlaceableObject;
using PlaceableObject.Abilities;
using UnityEngine;

namespace FutureBattleflow.Models
{
    [Serializable]
    public class LoadoutShipEntry
    {
        [SerializeField] private string shipId;
        [SerializeField] private string displayName;
        [SerializeField] private PlaceableObject.PlaceableObject shipPrefab;
        [SerializeField] private List<Ability> equippedAbilities = new();

        public string ShipId => shipId;
        public string DisplayName => displayName;
        public PlaceableObject.PlaceableObject ShipPrefab => shipPrefab;
        public IReadOnlyList<Ability> EquippedAbilities => equippedAbilities;
    }
}
