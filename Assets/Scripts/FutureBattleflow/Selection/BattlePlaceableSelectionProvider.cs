using System.Collections.Generic;
using FutureBattleflow.Models;
using PlaceableObject;

namespace FutureBattleflow.Selection
{
    public sealed class BattlePlaceableSelectionProvider : IPlaceableSelectionProvider
    {
        private readonly FleetInstanceModel _fleet;
        private readonly List<PlaceableObject.PlaceableObject> _buffer = new();

        public BattlePlaceableSelectionProvider(FleetInstanceModel fleet)
        {
            _fleet = fleet;
        }

        public IReadOnlyList<PlaceableObject.PlaceableObject> Build(BattlefieldViewSide side)
        {
            _buffer.Clear();
            if (_fleet == null)
                return _buffer;

            var ships = _fleet.Ships;
            for (var shipIndex = 0; shipIndex < ships.Count; shipIndex++)
            {
                var ship = ships[shipIndex];
                if (ship == null || !ship.IsDeployed || ship.SourceShip == null)
                    continue;

                var abilities = ship.SourceShip.EquippedAbilities;
                for (var abilityIndex = 0; abilityIndex < abilities.Count; abilityIndex++)
                {
                    var ability = abilities[abilityIndex];
                    if (!ability)
                        continue;

                    if (!IsAllowedForSide(ability.AllowedDeploymentSide, side))
                        continue;

                    _buffer.Add(ability);
                }
            }

            return _buffer;
        }

        private static bool IsAllowedForSide(PlaceableObjectDeploymentSide deploymentSide, BattlefieldViewSide side)
        {
            return side == BattlefieldViewSide.Own
                ? deploymentSide == PlaceableObjectDeploymentSide.OwnField
                : deploymentSide == PlaceableObjectDeploymentSide.EnemyField;
        }
    }
}
