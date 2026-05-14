using UnityEngine;

namespace PlaceableObject.Ships.Data
{
    [CreateAssetMenu(fileName = "SmallShipData", menuName = "Game/Ships/Small Ship Data")]
    public class SmallShipHullData : ShipHullData
    {
        protected override void SetupShipParameters()
        { 
            displayName = "This is the Ship Name";
            description = "This is the Ship Description";
            
            accuracy = 0.8f;
            accuracyDelta = 0.2f; 

            stealth = 0.8f;
            stealthDelta = 0.2f;

            detection = 0.2f;
            detectionDelta = 0.2f;

            resistance = 0.2f;
            resistanceDelta = 0.2f;

            energyCapacity = 6;
            energyRechargePerTurn = 1;

            modifierSlotsCount = 1;
        }
    }
}
