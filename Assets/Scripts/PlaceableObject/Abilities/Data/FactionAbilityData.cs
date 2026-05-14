using UnityEngine;

namespace PlaceableObject.Abilities.Data
{
    [CreateAssetMenu(fileName = "FactionAbilityStubData", menuName = "Game/Abilities/Faction Ability Data")]
    public class FactionAbilityData : AbilityData
    {
        protected override AbilityType AllowedAbilityType => AbilityType.Faction;

        protected override void SetupAbilityParameters()
        {
            displayName = "Faction Ability Stub";
            description = "Temporary faction ability placeholder.";

            accuracyConsumption = -0.3f;
            stealthConsumption = -0.3f;
            energyConsumption = 3;
        }
    }
}
