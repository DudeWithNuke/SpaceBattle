using System.Collections.Generic;
using FutureBattleflow.Models;
using PlaceableObject;

namespace FutureBattleflow.Selection
{
    public sealed class PreparationPlaceableSelectionProvider : IPlaceableSelectionProvider
    {
        private readonly PlayerLoadoutAsset _loadout;
        private readonly List<PlaceableObject.PlaceableObject> _buffer = new();

        public PreparationPlaceableSelectionProvider(PlayerLoadoutAsset loadout)
        {
            _loadout = loadout;
        }

        public IReadOnlyList<PlaceableObject.PlaceableObject> Build(BattlefieldViewSide side)
        {
            _buffer.Clear();
            if (_loadout == null)
                return _buffer;

            var ships = _loadout.Ships;
            for (var i = 0; i < ships.Count; i++)
            {
                var shipPrefab = ships[i].ShipPrefab;
                if (!shipPrefab)
                    continue;

                if (shipPrefab.AllowedDeploymentSide != PlaceableObjectDeploymentSide.OwnField)
                    continue;

                _buffer.Add(shipPrefab);
            }

            return _buffer;
        }
    }
}
