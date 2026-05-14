using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlaceableObjectSettings", menuName = "Settings/Placeable Object Settings")]
    public sealed class PlaceableObjectSettings : ScriptableObject
    {
        [Header("Dynamic Movement")]
        public float positionThreshold = 0.01f;
        public float additionalMovementRange = 40f;
        public float moveSpeed = 50f;
        public float dynamicSpeedSmoothing = 12f;
        public float minDynamicSpeedFactor = 0.05f;
        public float maxDynamicSpeedFactor = 2.5f;
        public float cursorSpeedForMaxFactor = 20f;
        public float catchUpDistanceForMaxFactor = 3f;

        [Header("Visual")]
        public Color defaultColor = Color.white;
        public Color invalidPlacementColor = Color.red;
        
        [Header("Coordinate Conversion")]
        public float cellCenterOffset = 0.5f;
        public float coordinateRoundingOffset = 0.5f;
        
        [Header("Detection")]
        public string groundLayerName = "Ground";
        public string defaultLayerName = "Default";
        public string placeableObjectLayerName = "PlaceableObjectLayer";
    }
}
