using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlaceableObjectManipulation
{
    public class SpawnedObjectLifecycleTracker : MonoBehaviour
    {
        public event Action<PlaceableObject.PlaceableObject> OnPicked;
        public event Action<PlaceableObject.PlaceableObject> OnPlaced;
        public event Action<PlaceableObject.PlaceableObject> OnDestroyed;

        private readonly HashSet<PlaceableObject.PlaceableObject> _trackedObjects = new();

        public IEnumerable<PlaceableObject.PlaceableObject> TrackedObjects => _trackedObjects;

        public void Register(PlaceableObject.PlaceableObject placeableObject)
        {
            if (!placeableObject || !_trackedObjects.Add(placeableObject))
                return;

            placeableObject.OnPicked += HandlePicked;
            placeableObject.OnPlaced += HandlePlaced;
            placeableObject.OnDestroyed += HandleDestroyed;
        }

        public void Unregister(PlaceableObject.PlaceableObject placeableObject)
        {
            if (!placeableObject || !_trackedObjects.Remove(placeableObject))
                return;

            placeableObject.OnPicked -= HandlePicked;
            placeableObject.OnPlaced -= HandlePlaced;
            placeableObject.OnDestroyed -= HandleDestroyed;
        }

        private void HandlePicked(PlaceableObject.PlaceableObject placeableObject)
        {
            OnPicked?.Invoke(placeableObject);
        }

        private void HandlePlaced(PlaceableObject.PlaceableObject placeableObject)
        {
            OnPlaced?.Invoke(placeableObject);
        }

        private void HandleDestroyed(PlaceableObject.PlaceableObject placeableObject)
        {
            Unregister(placeableObject);
            OnDestroyed?.Invoke(placeableObject);
        }

        private void OnDestroy()
        {
            var trackedSnapshot = new List<PlaceableObject.PlaceableObject>(_trackedObjects);
            foreach (var placeableObject in trackedSnapshot)
                Unregister(placeableObject);
        }
    }
}
