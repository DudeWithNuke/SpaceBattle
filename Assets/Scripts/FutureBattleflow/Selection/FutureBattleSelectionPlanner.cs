using System;
using System.Collections.Generic;
using FutureBattleflow.Models;

namespace FutureBattleflow.Selection
{
    public sealed class FutureBattleSelectionPlanner
    {
        private readonly PreparationPlaceableSelectionProvider _preparationProvider;
        private readonly BattlePlaceableSelectionProvider _battleProvider;

        public FutureBattleSelectionPlanner(PlayerLoadoutAsset loadout, FleetInstanceModel fleet)
        {
            _preparationProvider = new PreparationPlaceableSelectionProvider(loadout);
            _battleProvider = new BattlePlaceableSelectionProvider(fleet);
        }

        public IReadOnlyList<PlaceableObject.PlaceableObject> BuildButtons(BattlePhase phase, BattlefieldViewSide side)
        {
            return phase == BattlePhase.Preparation
                ? _preparationProvider.Build(side)
                : _battleProvider.Build(side);
        }

        public static FleetInstanceModel BuildFleetFromLoadout(PlayerLoadoutAsset loadout)
        {
            var fleet = new FleetInstanceModel();
            if (loadout == null)
                return fleet;

            var ships = loadout.Ships;
            for (var i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ship == null || !ship.ShipPrefab)
                    continue;

                var instanceId = $"{ship.ShipId}_{i}_{Guid.NewGuid():N}";
                fleet.Add(new FleetShipInstance(instanceId, ship));
            }

            return fleet;
        }
    }
}
