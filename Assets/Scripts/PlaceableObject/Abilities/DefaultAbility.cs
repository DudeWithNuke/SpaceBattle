namespace PlaceableObject.Abilities
{
    public class DefaultAbility : Ability
    {
        protected override PlaceableObjectDeploymentSide DeploymentSide => PlaceableObjectDeploymentSide.EnemyField;

        protected override void DefineShape(Shape shape)
        {
            shape.AddCell(0, 0, 0);
        }
    }
}
