using UnityEngine;

namespace PlaceableObject.Abilities.Data
{
    [CreateAssetMenu(fileName = "DefaultAttackAbilityData", menuName = "Game/Abilities/Default Attack Ability Data")]
    public class DefaultAttackAbilityData : AbilityData
    {
        protected override AbilityType AllowedAbilityType => AbilityType.Default;

        protected override void SetupAbilityParameters()
        {
            displayName = "Base Attack";
            description = "Default ship attack";

            accuracyConsumption = -0.1f;
            stealthConsumption = -0.1f;
            energyConsumption = 0;
        }
    }
}
