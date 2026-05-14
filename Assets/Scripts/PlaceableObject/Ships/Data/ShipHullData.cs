using UnityEngine;

namespace PlaceableObject.Ships.Data
{
    public abstract class ShipHullData : ScriptableObject
    {
        [Header("Visuals")]
        [SerializeField] protected string displayName;
        [SerializeField] protected string description;
        [SerializeField] private Sprite icon;
        
        [Header("Accuracy Stats")]
        [SerializeField, Range(0f, 1f)] protected float accuracy;
        [SerializeField, Range(0f, 1f)] protected float accuracyDelta;
        
        [Header("Stealth Stats")]
        [SerializeField, Range(0f, 1f)] protected float stealth;
        [SerializeField, Range(0f, 1f)] protected float stealthDelta;
        
        [Header("Detection Stats")]
        [SerializeField, Range(0f, 1f)] protected float detection;
        [SerializeField, Range(0f, 1f)] protected float detectionDelta;
        
        [Header("Resistance Stats")]
        [SerializeField, Range(0f, 1f)] protected float resistance;
        [SerializeField, Range(0f, 1f)] protected float resistanceDelta;

        [Header("Energy Stats")]
        [SerializeField, Range(0, 12)] protected int energyCapacity;
        [SerializeField, Range(0, 12)] protected int energyRechargePerTurn;
        
        [Header("Configuration")]
        [SerializeField, Range(0, 3)] protected int modifierSlotsCount;
        
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public float Accuracy => accuracy;
        public float AccuracyDelta => accuracyDelta;
        public float Stealth => stealth;
        public float StealthDelta => stealthDelta;
        public float Detection => detection;
        public float DetectionDelta => detectionDelta;
        public float Resistance => resistance;
        public float ResistanceDelta => resistanceDelta;
        public int EnergyCapacity => energyCapacity;
        public int EnergyRechargePerTurn => energyRechargePerTurn;
        public int ModifierSlotsCount => modifierSlotsCount;
        
        protected abstract void SetupShipParameters();
        
        protected virtual void OnValidate()
        {
            SetupShipParameters();
        }
    }
}
