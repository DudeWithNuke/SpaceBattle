using System.Collections.Generic;
using PlaceableObject.Abilities;
using PlaceableObject.Ships;
using UI.FleetPanel;

namespace Network.Placement
{
    public sealed class PrefabRegistry
    {
        private readonly Dictionary<string, PlaceableObject.PlaceableObject> _prefabsById = new();
        private readonly Dictionary<PlaceableObject.PlaceableObject, string> _idsByPrefab = new();
        private bool _isInitialized;

        public void Initialize(ShipRoster shipRoster)
        {
            if (_isInitialized)
                return;

            RegisterRosterPrefabs(shipRoster);
            _isInitialized = true;
        }

        public bool TryGetPrefab(string prefabId, out PlaceableObject.PlaceableObject prefab)
        {
            return _prefabsById.TryGetValue(prefabId, out prefab);
        }

        public string GetPrefabId(PlaceableObject.PlaceableObject prefab)
        {
            if (!prefab)
                return string.Empty;

            if (_idsByPrefab.TryGetValue(prefab, out var id))
                return id;

            id = CreatePrefabId(prefab);
            Register(id, prefab);
            return id;
        }

        public static string GetPrefabIdForInstance(PlaceableObject.PlaceableObject instance)
        {
            return !instance ? string.Empty : $"{instance.GetType().Name}:{NormalizeInstanceName(instance.name)}";
        }

        private void RegisterRosterPrefabs(ShipRoster shipRoster)
        {
            if (!shipRoster)
                return;

            foreach (var placeableObject in shipRoster.GetAll())
            {
                if (!placeableObject)
                    continue;

                Register(CreatePrefabId(placeableObject), placeableObject);

                if (placeableObject is Ship ship)
                    RegisterShipAbilities(ship);
            }
        }

        private void RegisterShipAbilities(Ship ship)
        {
            RegisterAbility(ship.DefaultAbility);
            RegisterAbility(ship.UnitAbility);
            RegisterAbility(ship.FactionAbility);
        }

        private void RegisterAbility(Ability ability)
        {
            if (ability)
                Register(CreatePrefabId(ability), ability);
        }

        private void Register(string id, PlaceableObject.PlaceableObject prefab)
        {
            if (string.IsNullOrWhiteSpace(id) || !prefab)
                return;

            _prefabsById.TryAdd(id, prefab);
            _idsByPrefab.TryAdd(prefab, id);
        }

        private static string CreatePrefabId(PlaceableObject.PlaceableObject prefab)
        {
            return $"{prefab.GetType().Name}:{prefab.name}";
        }

        private static string NormalizeInstanceName(string instanceName)
        {
            return instanceName.Replace("(Clone)", string.Empty).Trim();
        }
    }
}
