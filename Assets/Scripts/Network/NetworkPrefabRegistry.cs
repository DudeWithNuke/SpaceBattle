using System;
using System.Collections.Generic;
using PlaceableObject.Abilities;
using PlaceableObject.Ships;
using UI.FleetPanel;
using UnityEngine;

namespace Network
{
    public sealed class NetworkPrefabRegistry : MonoBehaviour
    {
        [SerializeField] private ShipRoster shipRoster;
        [SerializeField] private List<Entry> additionalPrefabs = new();

        private readonly Dictionary<string, PlaceableObject.PlaceableObject> _prefabsById = new();
        private readonly Dictionary<PlaceableObject.PlaceableObject, string> _idsByPrefab = new();
        private bool _isInitialized;

        [Serializable]
        private struct Entry
        {
            public string id;
            public PlaceableObject.PlaceableObject prefab;
        }

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            if (!shipRoster)
                shipRoster = FindFirstObjectByType<ShipRoster>();

            RegisterAdditionalPrefabs();
            RegisterRosterPrefabs();
            _isInitialized = true;
        }

        public bool TryGetPrefab(string prefabId, out PlaceableObject.PlaceableObject prefab)
        {
            Initialize();
            return _prefabsById.TryGetValue(prefabId, out prefab);
        }

        public string GetPrefabId(PlaceableObject.PlaceableObject prefab)
        {
            Initialize();
            if (!prefab)
                return string.Empty;

            if (_idsByPrefab.TryGetValue(prefab, out var id))
                return id;

            id = CreatePrefabId(prefab);
            Register(id, prefab);
            return id;
        }

        public string GetPrefabIdForInstance(PlaceableObject.PlaceableObject instance)
        {
            if (!instance)
                return string.Empty;

            return $"{instance.GetType().Name}:{NormalizeInstanceName(instance.name)}";
        }

        private void RegisterAdditionalPrefabs()
        {
            foreach (var entry in additionalPrefabs)
                Register(entry.id, entry.prefab);
        }

        private void RegisterRosterPrefabs()
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

            if (!_prefabsById.ContainsKey(id))
                _prefabsById.Add(id, prefab);

            if (!_idsByPrefab.ContainsKey(prefab))
                _idsByPrefab.Add(prefab, id);
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
