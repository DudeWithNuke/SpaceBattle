namespace PlaceableObject.Abilities
{
    public class FactionAbility : Ability
    {
        protected override PlaceableObjectDeploymentSide DeploymentSide => PlaceableObjectDeploymentSide.OwnField;

        protected override void DefineShape(Shape shape)
        {
            shape.AddCell(0, 0, 0);
        }
    }
}
