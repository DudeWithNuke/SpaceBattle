namespace PlaceableObject.Ships
{
    public class SmallShip : Ship
    {
        protected override void DefineShape(Shape shape)
        {
            for (var x = 0; x < 2; x++)
            for (var z = 0; z < 3; z++)
                shape.AddCell(x, 0, z);
        }
    }
}
