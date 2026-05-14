using UnityEngine;

namespace PlaceableObject.Modifiers
{
    [CreateAssetMenu(fileName = "DefaultModifierData", menuName = "Game/Modifiers/Default Modifier Data")]
    public class DefaultModifierData : ModifierData
    {
        protected override void SetupModifierParameters()
        {
            displayName = "Default Modifier";
            description = "Default passive modifier data.";

            accuracy = 0.1f;
            accuracyIncreasePerRest = 0.1f;
            accuracyDecreasePerAction = -0.1f;

            stealth = 0.1f;
            stealthIncreasePerRest = 0.1f;
            stealthDecreasePerAction = -0.1f;

            detection = 0.1f;
            detectionIncreasePerEnemyAction = 0.1f;
            detectionDecreasePerRest = -0.1f;

            resistance = 0.1f;
            resistanceIncreasePerEnemyAction = 0.1f;
            resistanceDecreasePerRest = -0.1f;

            energyCapacity = 1;
            energyRechargePerTurn = 1;
        }
    }
}
