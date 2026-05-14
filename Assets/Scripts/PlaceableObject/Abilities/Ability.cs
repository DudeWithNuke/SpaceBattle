using PlaceableObject.Abilities.Data;
using UnityEngine;

namespace PlaceableObject.Abilities
{
    public abstract class Ability : PlaceableObject
    {
        [SerializeField] protected AbilityData abilityData;

        public AbilityData Data => abilityData;

        protected override bool UsesCellOccupancy => false;
    }
}
