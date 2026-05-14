using UnityEngine;

namespace PlaceableObject.Abilities.Data
{
    public enum AbilityType
    {
        Default,
        Unit,
        Faction
    }
    
    public abstract class AbilityData : ScriptableObject
    {
        [Header("Visuals")]
        [SerializeField] protected string displayName;
        [SerializeField] protected string description;
        [SerializeField] private Sprite icon;

        [Header("Ship Stats")]
        [SerializeField, Range(0f, -1f)] protected float accuracyConsumption;
        [SerializeField, Range(0f, -1f)] protected float stealthConsumption; 
        [SerializeField, Range(0, 12)] protected int energyConsumption;
        
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;

        public float AccuracyConsumption => accuracyConsumption;
        public float StealthConsumption => stealthConsumption;
        public int EnergyConsumption => energyConsumption;
        
        protected abstract AbilityType AllowedAbilityType { get; }
        public AbilityType AbilityType => AllowedAbilityType;
        
        protected abstract void SetupAbilityParameters();

        protected virtual void OnValidate()
        {
            SetupAbilityParameters();
        }
    }
}
