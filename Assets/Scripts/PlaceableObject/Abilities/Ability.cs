namespace PlaceableObject.Abilities
{
    public class Ability : PlaceableObject
    {
        protected override PlaceableObjectDeploymentSide DeploymentSide => PlaceableObjectDeploymentSide.EnemyField;
        protected override bool UsesCellOccupancy => false;

        protected override void DefineShape(Shape shape)
        {
            for (var x = 0; x < 2; x++)
            for (var y = 0; y < 2; y++)
            for (var z = 0; z < 2; z++)
                shape.AddCell(x, y, z);
        }
    }
}
