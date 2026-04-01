using UnityEngine;

namespace PlaceableObject.Ships
{
    public class Ship : PlaceableObject
    {
        [field: SerializeField, Range(0f, 1f)] public float Accuracy { get; private set; } = 0.9f;
        [field: SerializeField, Range(0f, 1f)] public float Stealth { get; private set; } = 0.9f;
        [field: SerializeField, Range(0f, 1f)] public float Sonar { get; private set; } = 0.5f;
        [field: SerializeField, Range(0f, 1f)] public float Resistance { get; private set; } = 0.5f;
        [field: SerializeField] public int Energy { get; private set; } = 6;

        protected override PlaceableObjectDeploymentSide DeploymentSide => PlaceableObjectDeploymentSide.OwnField;

        protected override void DefineShape(Shape shape)
        {
            for (var x = 0; x < 2; x++)
            for (var y = 0; y < 2; y++)
            for (var z = 0; z < 2; z++)
                shape.AddCell(x, y, z);
        }
    }
}
