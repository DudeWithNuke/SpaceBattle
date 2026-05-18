using System;
using PlaceableObject;
using PlaceableObject.Ships;
using UnityEngine;

namespace UI.UnitCard
{
    public class UnitCardDeploymentController : MonoBehaviour
    {
        public event Action<UnitCardDeploymentController, Ship> OnShipSpawnRequested;
        public event Action<UnitCardDeploymentController, Ship> OnShipPickRequested;
        public event Action<UnitCardDeploymentController> OnShipPlaced;
        public event Action<UnitCardDeploymentController> OnShipPicked;
        public event Action<UnitCardDeploymentController> OnShipDestroyed;

        public Ship ShipPrefab { get; private set; }

        public Ship ShipInstance { get; private set; }

        public bool HasInstance => ShipInstance != null;
        public PlaceableObjectState? CurrentState => ShipInstance ? ShipInstance.State : null;

        public void Initialize(Ship prefab)
        {
            ShipPrefab = prefab;
        }

        public void BindInstance(Ship instance)
        {
            if (ShipInstance == instance)
                return;

            UnsubscribeFromInstance();
            ShipInstance = instance;

            if (ShipInstance)
            {
                ShipInstance.OnPlaced += HandlePlaced;
                ShipInstance.OnPicked += HandlePicked;
                ShipInstance.OnDestroyed += HandleDestroyed;
            }

            NotifyStateChanged();
        }

        public void ReleaseInstance()
        {
            UnsubscribeFromInstance();
            ShipInstance = null;
            NotifyStateChanged();
        }

        public void RequestSpawn()
        {
            if (ShipPrefab)
                OnShipSpawnRequested?.Invoke(this, ShipPrefab);
        }

        public void RequestPick()
        {
            if (ShipInstance)
                OnShipPickRequested?.Invoke(this, ShipInstance);
        }

        public bool CanSpawn => !ShipInstance;
        public bool CanPick => ShipInstance && ShipInstance.State == PlaceableObjectState.Placed;

        private void HandlePlaced(PlaceableObject.PlaceableObject obj)
        {
            if (obj != ShipInstance)
                return;

            OnShipPlaced?.Invoke(this);
            NotifyStateChanged();
        }

        private void HandlePicked(PlaceableObject.PlaceableObject obj)
        {
            if (obj != ShipInstance)
                return;

            OnShipPicked?.Invoke(this);
            NotifyStateChanged();
        }

        private void HandleDestroyed(PlaceableObject.PlaceableObject obj)
        {
            if (obj != ShipInstance)
                return;

            OnShipDestroyed?.Invoke(this);
            ReleaseInstance();
        }

        private void UnsubscribeFromInstance()
        {
            if (!ShipInstance)
                return;

            ShipInstance.OnPlaced -= HandlePlaced;
            ShipInstance.OnPicked -= HandlePicked;
            ShipInstance.OnDestroyed -= HandleDestroyed;
        }

        private void NotifyStateChanged()
        {
            // State changed notification is handled through specific events
        }

        private void OnDestroy()
        {
            UnsubscribeFromInstance();
        }
    }
}
