using System;
using PlaceableObject.Abilities;
using UnityEngine;

namespace PlaceableObject.Ships
{
    [Serializable]
    public sealed class AbilitySet
    {
        [SerializeField] private Ability defaultAbility;
        [SerializeField] private Ability unitAbility;
        [SerializeField] private Ability factionAbility;

        public Ability DefaultAbility => defaultAbility;
        public Ability UnitAbility => unitAbility;
        public Ability FactionAbility => factionAbility;
    }
}
