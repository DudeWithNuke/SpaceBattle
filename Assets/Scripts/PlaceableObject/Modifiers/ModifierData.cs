using UnityEngine;

namespace PlaceableObject.Modifiers
{
    public abstract class ModifierData : ScriptableObject
    {
        [Header("Visuals")]
        [SerializeField] protected string displayName;
        [SerializeField] protected string description;
        [SerializeField] private Sprite icon;
        
        [Header("Accuracy Passive Bonuses")]
        [SerializeField, Range(0f, 1f)] protected float accuracy;
        [SerializeField, Range(0f, 1f)] protected float accuracyIncreasePerRest;
        [SerializeField, Range(0f, -1f)] protected float accuracyDecreasePerAction;
        
        [Header("Stealth Passive Bonuses")]
        [SerializeField, Range(0f, 1f)] protected float stealth;
        [SerializeField, Range(0f, 1f)] protected float stealthIncreasePerRest;
        [SerializeField, Range(0f, -1f)] protected float stealthDecreasePerAction;
        
        [Header("Detection Passive Bonuses")]
        [SerializeField, Range(0f, 1f)] protected float detection;
        [SerializeField, Range(0f, 1f)] protected float detectionIncreasePerEnemyAction;
        [SerializeField, Range(0f, -1f)] protected float detectionDecreasePerRest;
        
        [Header("Resistance Passive Bonuses")]
        [SerializeField, Range(0f, 1f)] protected float resistance;
        [SerializeField, Range(0f, 1f)] protected float resistanceIncreasePerEnemyAction;
        [SerializeField, Range(0f, -1f)] protected float resistanceDecreasePerRest;

        [Header("Energy Passive Bonuses")]
        [SerializeField, Range(0, 12)] protected int energyCapacity;
        [SerializeField, Range(0, 12)] protected int energyRechargePerTurn;

        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public float AccuracyBonus => accuracy;
        public float AccuracyIncreasePerRestBonus => accuracyIncreasePerRest;
        public float AccuracyDecreasePerActionBonus => accuracyDecreasePerAction;
        public float StealthBonus => stealth;
        public float StealthIncreasePerRestBonus => stealthIncreasePerRest;
        public float StealthDecreasePerActionBonus => stealthDecreasePerAction;
        public float DetectionBonus => detection;
        public float DetectionIncreasePerEnemyActionBonus => detectionIncreasePerEnemyAction;
        public float DetectionDecreasePerRestBonus => detectionDecreasePerRest;
        public float ResistanceBonus => resistance;
        public float ResistanceIncreasePerEnemyActionBonus => resistanceIncreasePerEnemyAction;
        public float ResistanceDecreasePerRestBonus => resistanceDecreasePerRest;
        public int EnergyCapacityBonus => energyCapacity;
        public int EnergyRechargePerTurnBonus => energyRechargePerTurn;

        protected abstract void SetupModifierParameters();

        private void ResetModifierParameters()
        {
            accuracy = 0f;
            accuracyIncreasePerRest = 0f;
            accuracyDecreasePerAction = 0f;

            stealth = 0f;
            stealthIncreasePerRest = 0f;
            stealthDecreasePerAction = 0f;

            detection = 0f;
            detectionIncreasePerEnemyAction = 0f;
            detectionDecreasePerRest = 0f;

            resistance = 0f;
            resistanceIncreasePerEnemyAction = 0f;
            resistanceDecreasePerRest = 0f;

            energyCapacity = 0;
            energyRechargePerTurn = 0;
        }
        
        protected virtual void OnValidate()
        {
            ResetModifierParameters();
            SetupModifierParameters();
        }
        
        //todo добавить автоматическую событийную способность
        //[Header("Action")]
        //[SerializeField] private ModifierAction action;
    }
}
