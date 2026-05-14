using UnityEngine;

namespace PlaceableObject.Abilities.Data
{
    [CreateAssetMenu(fileName = "UnitAbilityStubData", menuName = "Game/Abilities/Unit Ability Data")]
    public class UnitAbilityData : AbilityData
    {
        protected override AbilityType AllowedAbilityType => AbilityType.Unit;

        protected override void SetupAbilityParameters()
        {
            displayName = "Unit Ability Stub";
            description = "Temporary unit ability placeholder.";

            accuracyConsumption = -0.2f;
            stealthConsumption = -0.2f;
            energyConsumption = 2;
        }
    }
}
