namespace PlaceableObject
{
    public class DefenseAbility : PlaceableObject
    {
        protected override PlaceableObjectDeploymentSide DeploymentSide => PlaceableObjectDeploymentSide.OwnField;
        protected override bool UsesCellOccupancy => false;

        protected override void DefineShape(PlaceableObjectShape shape)
        {
            for (var x = 0; x < 2; x++)
            for (var y = 0; y < 2; y++)
            for (var z = 0; z < 2; z++)
                shape.AddCell(x, y, z);
        }
    }
}
