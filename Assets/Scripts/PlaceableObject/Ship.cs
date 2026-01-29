using UnityEngine;

namespace PlaceableObject
{
    public class Ship : PlaceableObject
    {
        protected override void DefineShape(PlaceableObjectShape shape)
        {
            // Создаем форму 3x1x2
            for (var x = 0; x < 3; x++)
            for (var y = 0; y < 1; y++)
            for (var z = 0; z < 2; z++)
                shape.AddCell(x, y, z);
        }
    }
}