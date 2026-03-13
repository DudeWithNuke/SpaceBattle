using System.Collections.Generic;
using FutureBattleflow.Models;
using PlaceableObject;

namespace FutureBattleflow.Selection
{
    public interface IPlaceableSelectionProvider
    {
        IReadOnlyList<PlaceableObject.PlaceableObject> Build(BattlefieldViewSide side);
    }
}
