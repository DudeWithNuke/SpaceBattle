using System;
using PlaceableObject;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ShipButton : MonoBehaviour
    {
        public static event Action<ShipButton, PlaceableObject.PlaceableObject> OnObjectSpawned;

        private PlaceableObject.PlaceableObject _placeableObjectPrefab;
        private PlaceableObject.PlaceableObject _placeableObjectInstance;
        private Button _button;

        public void Initialize(PlaceableObject.PlaceableObject prefab)
        {
            _placeableObjectPrefab = prefab;
            _button = GetComponent<Button>();

            name = _placeableObjectPrefab.name + " Button";

            _button.onClick.AddListener(HandleClick);
            DisableKeyboard();
        }

        public void DisableInteraction()
        {
            _button.interactable = false;
        }

        public void EnableInteraction()
        {
            _button.interactable = true;
        }

        private void DisableKeyboard()
        {
            var navigation = _button.navigation;
            navigation.mode = Navigation.Mode.None;
            _button.navigation = navigation;
        }

        private void HandleClick()
        {
            if (!_placeableObjectInstance)
            {
                SpawnPrefab();
                return;
            }

            if (_placeableObjectInstance.State == PlaceableObjectState.Placed)
            {
                _placeableObjectInstance.TryPick();
                DisableInteraction();
            }
        }

        private void SpawnPrefab()
        {
            _placeableObjectInstance = Instantiate(_placeableObjectPrefab, Vector3.zero, Quaternion.identity);
            if (!_placeableObjectInstance)
                return;

            var sceneContainer = gameObject.scene.GetSceneContainer();
            GameObjectInjector.InjectObject(_placeableObjectInstance.gameObject, sceneContainer);

            _placeableObjectInstance.OnPlaced += HandlePlaced;
            _placeableObjectInstance.OnPicked += HandlePicked;
            _placeableObjectInstance.OnDestroyed += HandleDestroyed;

            DisableInteraction();
            OnObjectSpawned?.Invoke(this, _placeableObjectInstance);
        }

        private void HandlePlaced(PlaceableObject.PlaceableObject placeableObject)
        {
            if (placeableObject == _placeableObjectInstance)
                EnableInteraction();
        }

        private void HandlePicked(PlaceableObject.PlaceableObject placeableObject)
        {
            if (placeableObject == _placeableObjectInstance)
                DisableInteraction();
        }

        private void HandleDestroyed(PlaceableObject.PlaceableObject placeableObject)
        {
            if (placeableObject != _placeableObjectInstance)
                return;

            UnsubscribeFromInstance();
            _placeableObjectInstance = null;
            EnableInteraction();
        }

        private void UnsubscribeFromInstance()
        {
            if (!_placeableObjectInstance)
                return;

            _placeableObjectInstance.OnPlaced -= HandlePlaced;
            _placeableObjectInstance.OnPicked -= HandlePicked;
            _placeableObjectInstance.OnDestroyed -= HandleDestroyed;
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClick);

            UnsubscribeFromInstance();
        }
    }
}
