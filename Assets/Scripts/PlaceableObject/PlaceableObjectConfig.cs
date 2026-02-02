using UnityEngine;

namespace PlaceableObject
{
    public static class PlaceableObjectConfig
    {
        [Header("Movement")]
        public static float MoveSpeed = 50f;
        public static float PositionThreshold = 0.01f;
        
        [Header("Coordinate Conversion")]
        public static float CellCenterOffset = 0.5f;
        public static float CoordinateRoundingOffset = 0.5f;
        
        [Header("Detection")]
        public static string GroundLayerName = "Ground";
        public static string DefaultLayerName = "Default";
        public static string PlaceableObjectLayerName = "PlaceableObjectLayer";
    }
}
