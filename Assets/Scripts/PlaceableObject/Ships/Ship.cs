using System.Collections.Generic;
using PlaceableObject.Abilities;
using PlaceableObject.Modifiers;
using PlaceableObject.Ships.Data;
using UnityEngine;

namespace PlaceableObject.Ships
{
    public abstract class Ship : PlaceableObject
    {
        protected override PlaceableObjectDeploymentSide DeploymentSide => PlaceableObjectDeploymentSide.OwnField;

        [SerializeField] private ShipHullData hull;
        [SerializeField] private List<ModifierData> modifiers = new();
        [SerializeField] private AbilitySet abilitySet = new();

        public ShipHullData Hull => hull;
        public IReadOnlyList<ModifierData> Modifiers => modifiers;
        public AbilitySet AbilitySet => abilitySet;

        public string DisplayName => hull ? hull.DisplayName : string.Empty;
        public string Description => hull ? hull.Description : string.Empty;
        public Sprite Icon => hull ? hull.Icon : null;

        public float Accuracy => hull ? hull.Accuracy + GetTotalModifierBonus(modifier => modifier.AccuracyBonus) : 0f;
        public float AccuracyIncreasePerTurn => hull ? hull.AccuracyDelta + GetTotalModifierBonus(modifier => modifier.AccuracyIncreasePerRestBonus) : 0f;
        public float AccuracyDecreasePerTurn => hull ? hull.AccuracyDelta + GetTotalModifierBonus(modifier => modifier.AccuracyDecreasePerActionBonus) : 0f;

        public float Stealth => hull ? hull.Stealth + GetTotalModifierBonus(modifier => modifier.StealthBonus) : 0f;
        public float StealthIncreasePerTurn => hull ? hull.StealthDelta + GetTotalModifierBonus(modifier => modifier.StealthIncreasePerRestBonus) : 0f;
        public float StealthDecreasePerTurn => hull ? hull.StealthDelta + GetTotalModifierBonus(modifier => modifier.StealthDecreasePerActionBonus) : 0f;

        public float Detection => hull ? hull.Detection + GetTotalModifierBonus(modifier => modifier.DetectionBonus) : 0f;
        public float DetectionIncreasePerTurn => hull ? hull.DetectionDelta + GetTotalModifierBonus(modifier => modifier.DetectionIncreasePerEnemyActionBonus) : 0f;
        public float DetectionDecreasePerTurn => hull ? hull.DetectionDelta + GetTotalModifierBonus(modifier => modifier.DetectionDecreasePerRestBonus) : 0f;

        public float Resistance => hull ? hull.Resistance + GetTotalModifierBonus(modifier => modifier.ResistanceBonus) : 0f;
        public float ResistanceIncreasePerTurn => hull ? hull.ResistanceDelta + GetTotalModifierBonus(modifier => modifier.ResistanceIncreasePerEnemyActionBonus) : 0f;
        public float ResistanceDecreasePerTurn => hull ? hull.ResistanceDelta + GetTotalModifierBonus(modifier => modifier.ResistanceDecreasePerRestBonus) : 0f;

        public int Health
        {
            get
            {
                EnsureShapeInitialized();
                return Shape.GetIntactCellsCount();
            }
        }

        public int Energy => hull ? hull.EnergyCapacity + GetTotalModifierBonus(modifier => modifier.EnergyCapacityBonus) : 0;
        public int EnergyRechargePerTurn => hull ? hull.EnergyRechargePerTurn + GetTotalModifierBonus(modifier => modifier.EnergyRechargePerTurnBonus) : 0;

        public Ability DefaultAbility => abilitySet?.DefaultAbility;
        public Ability UnitAbility => abilitySet?.UnitAbility;
        public Ability FactionAbility => abilitySet?.FactionAbility;

        private float GetTotalModifierBonus(System.Func<ModifierData, float> selector)
        {
            var totalBonus = 0f;

            for (var i = 0; i < modifiers.Count; i++)
            {
                var modifier = modifiers[i];
                if (modifier == null)
                    continue;

                totalBonus += selector(modifier);
            }

            return totalBonus;
        }

        private int GetTotalModifierBonus(System.Func<ModifierData, int> selector)
        {
            var totalBonus = 0;

            for (var i = 0; i < modifiers.Count; i++)
            {
                var modifier = modifiers[i];
                if (modifier == null)
                    continue;

                totalBonus += selector(modifier);
            }

            return totalBonus;
        }

        private void OnValidate()
        {
            if (!hull)
                return;

            modifiers ??= new List<ModifierData>(hull.ModifierSlotsCount);

            if (modifiers.Count < hull.ModifierSlotsCount)
                for (var i = modifiers.Count; i < hull.ModifierSlotsCount; i++)
                    modifiers.Add(null);
            else if (modifiers.Count > hull.ModifierSlotsCount)
                modifiers.RemoveRange(hull.ModifierSlotsCount, modifiers.Count - hull.ModifierSlotsCount);
        }
    }
}
